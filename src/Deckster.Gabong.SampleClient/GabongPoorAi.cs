using Deckster.Client.Games.Gabong;
using Deckster.Client.Logging;
using Deckster.Core.Games.Common;
using Deckster.Core.Games.Gabong;
using Microsoft.Extensions.Logging;

namespace Deckster.Gabong.SampleClient;

public class GabongPoorAi
{
    private readonly ILogger _logger;
    
    private PlayerViewOfGame _view = new();
    private readonly TaskCompletionSource _tcs = new();

    private readonly GabongClient _client;

    private bool _weArePlaying = false;
    
    private Task _playTask = Task.CompletedTask;
    private int _kingsPlayed  = 0;

    private List<string> _roundLog = new();
    public GabongPoorAi(GabongClient client)
    {
        _client = client;
        _logger = Log.Factory.CreateLogger(client.PlayerData.Name);
        client.GameStarted += OnGameStarted;
        client.RoundStarted += OnRoundStarted;
        client.PlayerPutCard += OnPlayerPutCard;
        client.PlayerDrewCard += OnPlayerDrewCard;
        client.PlayerLostTheirTurn += OnPlayerLostTheirTurn;
        client.PlayerDrewPenaltyCard += OnPlayerDrewPenaltyCard;
        client.RoundEnded += OnRoundEnded;
        client.GameEnded += OnGameEnded;
    }

    private void OnPlayerDrewPenaltyCard(PlayerDrewPenaltyCardNotification obj)
    {
        if(obj.IsFor(_client.PlayerData.Id))
        {
            _roundLog.Add($"==> {GetPlayer(obj.PlayerId)} Drew a penalty card");
        }
    }

    private void OnRoundStarted(RoundStartedNotification obj)
    {
        _kingsPlayed = 0;
        _view = obj.PlayerViewOfGame;
        _weArePlaying = true;
    }

    private void OnGameStarted(GameStartedNotification obj)
    {
        _roundLog.Add("==> Game started");
        _playTask = Task.Run(async () =>
        {
            while (!_tcs.Task.IsCompleted)
            {
                await ThinkAboutDoingSomething(_view);
                await Task.Delay(100+new Random().Next(50));
            }
        });
    }

    public Task PlayAsync() => _tcs.Task;

    private void OnGameEnded(GameEndedNotification obj)
    {
        _roundLog.Add("==> Game ended");
        _tcs.SetResult();
    }

    private void OnRoundEnded(RoundEndedNotification obj)
    {
        _roundLog.Add("==> Round ended");
        _weArePlaying = false;
    }

    private void OnPlayerLostTheirTurn(PlayerLostTheirTurnNotification obj)
    {
        _view.LastPlay = GabongPlay.TurnLost;
        _view.LastPlayMadeByPlayerId = obj.PlayerId;
        
        var lostTurnReason = obj.LostTurnReason switch
        {
            PlayerLostTurnReason.FinishedDrawingCardDebt => "drew their card debt",
            PlayerLostTurnReason.Passed => "passed",
            PlayerLostTurnReason.WrongPlay => "made a wrong play",
            PlayerLostTurnReason.TookTooLong => "took too long",
            _ => "unknown"
        };
        if(obj.IsFor(_client.PlayerData.Id))
        {
            _roundLog.Add($"==> {GetPlayer(obj.PlayerId).Name} lost their turn since they {lostTurnReason}");
        }

    }

    private SlimGabongPlayer? GetPlayer(Guid playerId)
    {
        return _view.Players.FirstOrDefault(p => p.Id == playerId);
    }

    private void OnPlayerDrewCard(PlayerDrewCardNotification obj)
    {
        if(obj.IsFor(_client.PlayerData.Id))
        {    
            _roundLog.Add($"==> {GetPlayer(obj.PlayerId).Name} Drew");
       }
    }
    
    private void OnPlayerPutCard(PlayerPutCardNotification evt)
    {
        _view.TopOfPile = evt.Card;
        _view.CurrentSuit = evt.NewSuit ?? evt.Card.Suit;
        _view.LastPlay = GabongPlay.CardPlayed;
        _view.LastPlayMadeByPlayerId = evt.PlayerId;

        if (evt.Card.Rank == 13)
        {
            _kingsPlayed++;
        }
        var newSuitText = evt.NewSuit == null ? "" : $" and changed suit to {evt.NewSuit}";
        if(evt.IsFor(_client.PlayerData.Id))
        {
            _roundLog.Add($"==> {GetPlayer(evt.PlayerId).Name} Played {evt.Card} {newSuitText}");
        }
    }

    private async Task ThinkAboutDoingSomething(PlayerViewOfGame? obj)
    {
        try
        {
            
        if (!_weArePlaying)
        {
            return;
        }
        if (obj == null)
        {
            return;
        }
        if(IBelieveItsMyTurn(obj))
        {
            _roundLog.Add($"i believe it's my turn. Top: {_view.TopOfPile} ({_view.CurrentSuit}). I have: {string.Join(", ", _view.Cards)}");
            await DoSomePlay(obj);
        }
        }catch(Exception e)
        {
            _logger.LogError(e, "Argh");
        }
    }

    private async Task DoSomePlay(PlayerViewOfGame viewOfGame)
    {
        try{
            if (viewOfGame.CardDebtToDraw > 0)
            {
                viewOfGame.CardDebtToDraw--;
                var result = await _client.DrawCardAsync(new DrawCardRequest());
                UpdateView(result);
                return;
            }
            
            var cardToPlay = FindCardToPlay(viewOfGame);
            if (cardToPlay != null)
            {
                var canChangeSuit = cardToPlay.Value.Rank == 8;
                var newSuit = canChangeSuit
                    ? viewOfGame.Cards.GroupBy(x => x.Suit).OrderByDescending(x => x.Count()).First().Key
                    : (Suit?)null;
                _logger.LogInformation("Trying to play card {card}", cardToPlay.Value);
                _view.Cards.Remove(cardToPlay.Value);
                _view.TopOfPile = cardToPlay.Value;
                _view.LastPlayMadeByPlayerId = _client.PlayerData.Id;
                _view.LastPlay = GabongPlay.CardPlayed;
                _view = await _client.PutCardAsync(new PutCardRequest { Card = cardToPlay.Value, NewSuit = newSuit });
                
            }
            else
            {
                _view = await _client.DrawCardAsync(new DrawCardRequest());   
                
                var cardToPlayAfterDraw = FindCardToPlay(viewOfGame);
                if (cardToPlayAfterDraw == null)
                {
                    _view.LastPlayMadeByPlayerId = _client.PlayerData.Id;
                    _view.LastPlay = GabongPlay.TurnLost;
                    await _client.PassAsync(new PassRequest()); 
                }//else next iteration will have something to do
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Argh");
            throw;
        }
    }

    private void UpdateList<T>(List<T> list, List<T> newList)
    {
        list.Clear();
        list.AddRange(newList);
    }
    private void UpdateView(PlayerViewOfGame result)
    {
        _view.TopOfPile = result.TopOfPile;
        _view.CurrentSuit = result.CurrentSuit;
        _view.StockPileCount = result.StockPileCount;
        _view.DiscardPileCount = result.DiscardPileCount;
        _view.LastPlayMadeByPlayerId = result.LastPlayMadeByPlayerId;
        _view.LastPlay = result.LastPlay;
        
        UpdateList(_view.Cards, result.Cards);
        UpdateList(_view.Players, result.Players);
        UpdateList(_view.PlayersOrder, result.PlayersOrder);
        UpdateList(_view.CardsAdded, result.CardsAdded);
    }


  
    private bool IBelieveItsMyTurn(PlayerViewOfGame viewOfGame)
    {
        var direction = _kingsPlayed % 2 == 0 ? 1 : -1;
        if (!viewOfGame.RoundStarted)
        {
            return false;
        }
        var delta = viewOfGame.TopOfPile.Rank == 3 ? 2 : 1;
        if (viewOfGame.LastPlay == GabongPlay.RoundStarted)
        {
            delta = 0;
        }

        var offset = direction * delta;
        var myIndex = viewOfGame.PlayersOrder.IndexOf(_client.PlayerData.Id);
        var lastplayerIndex = viewOfGame.PlayersOrder.IndexOf(viewOfGame.LastPlayMadeByPlayerId);
        
        return (viewOfGame.PlayersOrder.Count + lastplayerIndex + offset) % viewOfGame.PlayersOrder.Count == myIndex;
    }

    private Card? FindCardToPlay(PlayerViewOfGame viewOfGame)
    {
        foreach (var c in viewOfGame.Cards.Where(c => c.Suit == _view.CurrentSuit || c.Rank == _view.TopOfPile.Rank))
        {
            return c;
        }
        return null;
    }

}