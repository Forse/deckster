using Deckster.Client.Games.Bullshit;
using Deckster.Core.Collections;
using Deckster.Core.Games.Bullshit;
using Deckster.Core.Games.Common;
using Microsoft.Extensions.Logging;

namespace Deckster.Bullshit.SampleClient;

public class BullshitPoorAi
{
    private readonly ILogger _logger;
    private readonly TaskCompletionSource _tcs = new();
    private readonly BullshitClient _client;
    private readonly BullshitState _state = new(); 

    public BullshitPoorAi(BullshitClient client, ILogger logger)
    {
        _client = client;
        _logger = logger;
        client.PlayerPassed += PlayerPassed;
        client.PlayerDrewCard += PlayerDrewCard;
        client.PlayerPutCard += PlayerPutCard;
        
        client.ItsYourTurn += ItsMyTurn;
        client.ItsPlayersTurn += ItsPlayersTurn;
        client.GameStarted += GameStarted;
        client.GameEnded += GameEnded;
        client.PlayerAccusedFalseBullshit += PlayerAccusedFalseBullshit;
        client.PlayersBullshitHasBeenCalled += PlayersBullshitHasBeenCalled;
        client.YourBullshitHasBeenCalled += MyBullshitHasBeenCalled;
    }

    private void ItsPlayersTurn(ItsPlayersTurnNotification n)
    {
        if (!_state.OtherPlayers.TryGetValue(n.PlayerId, out var player))
        {
            return;
        }
        _logger.LogInformation("It's {player}'s turn", player);
    }

    private void MyBullshitHasBeenCalled(BullshitPlayerNotification n)
    {
        _state.Cards.AddRange(n.PunishmentCards.Append(n.Card));
        _state.DiscardPile.Remove(n.Card);
    }

    private void PlayersBullshitHasBeenCalled(BullshitBroadcastNotification n)
    {
        if (!_state.OtherPlayers.TryGetValue(n.PlayerId, out var player))
        {
            return;
        }
        _logger.LogInformation("{player}'s bullshit: claimed: {claimed}, actual: {actual}", n.ClaimedToBeCard, n.ActualCard);
        player.KnownCards.Add(n.ActualCard);
        player.CardCount += n.PunishmentCardCount + 1;
        _state.DiscardPile.Remove(n.ClaimedToBeCard);
    }

    private void PlayerAccusedFalseBullshit(FalseBullshitCallNotification n)
    {
        if (!_state.OtherPlayers.TryGetValue(n.PlayerId, out var player))
        {
            return;
        }

        var accusedName = _state.OtherPlayers.TryGetValue(n.AccusedPlayerId, out var accused) ? accused.Name : "me";
        _logger.LogInformation("{player} accused false bullshit on {accused}", player, accusedName);
        player.CardCount += n.PunishmentCardCount;
        _state.StockPileCount -= n.PunishmentCardCount;
    }

    private void GameEnded(GameEndedNotification n)
    {
        _logger.LogInformation("Game ended. Loser: {loser}", n.LoserName);
        _tcs.TrySetResult();
    }

    private void GameStarted(GameStartedNotification n)
    {
        _logger.LogInformation("Game started. ");

        var my = n.PlayerViewOfGame;
        _state.Cards = my.Cards;
        _state.OtherPlayers = my.OtherPlayers.ToDictionary(p => p.PlayerId, p => new OtherPlayer
        {
            Id = p.PlayerId,
            Name = p.Name,
            CardCount = p.NumberOfCards,
            KnownCards = []
        });

        _state.StockPileCount = my.StockPileCount;
    }

    private readonly SemaphoreSlim _semaphore = new(1, 1);
    
    private async void ItsMyTurn(ItsYourTurnNotification n)
    {
        await _semaphore.WaitAsync();
        try
        {
            _logger.LogInformation("It's my turn");

            if (_state.TryGetCard(out var card))
            {
                _logger.LogInformation("Putting {card} on {top}", card, _state.ClaimedToBeTopOfPile);
                await _client.PutCardOrThrowAsync(card, card);
                _state.Cards.Remove(card);
                _state.DiscardPile.Add(card);
                return;
            }

            for (var ii = 0; ii < 3; ii++)
            {
                var drawn = await _client.DrawCardOrThrowAsync();
                _logger.LogInformation("Drew card {card}", card);
                _state.Cards.Add(drawn);

                if (_state.TryGetCard(out card))
                {
                    _logger.LogInformation("Putting {card} on {top}", card, _state.ClaimedToBeTopOfPile);
                    await _client.PutCardOrThrowAsync(card, card);
                    _state.Cards.Remove(card);
                    _state.DiscardPile.Add(card);
                    return;
                }
            }

            var random = new Random();

            if (random.Next(0, 100) > 50)
            {
                var top = _state.ClaimedToBeTopOfPile.GetValueOrDefault();
                var claimedRank = top.Rank;

                while (claimedRank == top.Rank)
                {
                    claimedRank = random.Next(1, 13);
                }

                card = _state.Cards.First();
                var claimedToBeCard = new Card(claimedRank, top.Suit);
                _logger.LogInformation("Putting {card} as {claimed} on {top}", card, claimedToBeCard, _state.ClaimedToBeTopOfPile);
                await _client.PutCardOrThrowAsync(card, claimedToBeCard);
                _state.Cards.Remove(card);
                _state.DiscardPile.Add(claimedToBeCard);
                return;
            }

            _logger.LogInformation("Passing");
            await _client.PassOrThrowAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "OH NOES!");
            throw;
        }
        finally
        {
            _semaphore.Release();
        }
        
    }

    private void PlayerPutCard(PlayerPutCardNotification n)
    {
        if (_state.OtherPlayers.TryGetValue(n.PlayerId, out var player))
        {
            _logger.LogInformation("{player} put card {card}", player, n.ClaimedToBeCard);
            player.CardCount--;
            _state.DiscardPile.Push(n.ClaimedToBeCard);
        }
    }

    private void PlayerDrewCard(PlayerDrewCardNotification n)
    {
        if (_state.OtherPlayers.TryGetValue(n.PlayerId, out var player))
        {
            _logger.LogInformation("{player} drew card", player);
            player.CardCount++;
        }
    }

    private void PlayerPassed(PlayerPassedNotification n)
    {
        if (_state.OtherPlayers.TryGetValue(n.PlayerId, out var player))
        {
            _logger.LogInformation("{player} passed", player);
        }
    }

    public Task PlayAsync(CancellationToken cancellationToken)
    {
        cancellationToken.Register(_tcs.SetCanceled);
        return _tcs.Task;
    }
}