using System.Diagnostics.CodeAnalysis;
using Deckster.Core.Collections;
using Deckster.Core.Games.Common;
using Deckster.Core.Games.Gabong;

namespace Deckster.Games.Gabong;

public partial class GabongGame : GameObject
{
   
    public const int INITIAL_HAND_SIZE = 7;
    public const int MAX_CARDS_IN_HAND = 20;
    public const int MAX_MILLIS_PER_TURN = 5_000;
    public const int MAX_SECONDS_BEFORE_STARTING_ROUND = 60;

    private void InfoLog(string msg)
    {
        
    }


    private int _lastPlayMadeByPlayerIndex = 0;
    public int LastPlayMadeByPlayerIndex
    {
        get => _lastPlayMadeByPlayerIndex;
        set
        {
            _lastPlayMadeAt = DateTime.UtcNow;
            _lastPlayMadeByPlayerIndex = value;
        }
    }

    protected override GameState GetState()
    {
        if (Players.Any(p => p.Score >= 100))
        {
            return GameState.Finished;
        }

        if (Players.Any(p => p.Cards.Count == 0))
        {
            return GameState.RoundFinished;
        }
        return GameState.Running;
    }


 



    public static GabongGame Instantiate(GabongGameCreatedEvent created)
    {
        var game = new GabongGame
        {
            Id = created.Id,
            Name = created.Name,
            StartedTime = created.StartedTime,
            IsBetweenRounds = true,
            Players = created.Players.Select(p => new GabongPlayer
            {
                Id = p.Id,
                Name = p.Name
            }).ToList(),
            Deck = created.Deck,
            Seed = created.InitialSeed,
        };
        return game;
    }

    public void NewRound()
    {
        IsBetweenRounds = true;
        IncrementSeed();
        foreach (var player in Players)
        {
            player.Cards.Clear();
        }

        LastPlay = GabongPlay.RoundStarted;
        LastPlayMadeByPlayerIndex = GetPlayerIndex(GabongMasterId);

        StockPile.Clear();
        StockPile.PushRange(Deck);
        for (var ii = 0; ii < INITIAL_HAND_SIZE; ii++)
        {
            foreach (var player in Players)
            {
                player.Cards.Add(StockPile.Pop());
            }
        }

        DiscardPile.Clear();
        var startingCard = StockPile.Pop();
        var toReshuffle = new List<Card>();
        while (startingCard.IsASpecialCard()) //we don't want to start with a special card
        {
            toReshuffle.Add(startingCard);
            startingCard = StockPile.Pop();
        }

        DiscardPile.Push(startingCard);
        StockPile.PushRange(toReshuffle);
        ShufflePileIfNecessary();
        IsBetweenRounds = false;
    }



   

    private async Task<PlayerViewOfGame?> HandleMaybeRoundEnded(bool outsideFactorsSaysItsFinished = false)
    {
        if (State == GameState.RoundFinished || outsideFactorsSaysItsFinished)
        {
            await EndRound();
            if (State == GameState.Finished)
            {
                await GameEnded.InvokeOrDefault(() => new GameEndedNotification
                {
                    Players = Players.Select(p => p.ToPlayerData()).ToList()
                });
                return new PlayerViewOfGame("Game Over");
            }
            else
            {
                await RoundEnded.InvokeOrDefault(() => new RoundEndedNotification
                {
                    Players = Players.Select(p => p.ToPlayerData()).ToList()
                });
                NewRound();
                await NotifyThatRoundHasStarted();
            }

            return new PlayerViewOfGame("New Round Started");
        }

        return null;
    }


    public async Task NotifyThatRoundHasStarted()
    {
        foreach (var player in Players)
        {
            await RoundStarted.InvokeOrDefault(player.Id, () => new RoundStartedNotification
            {
                PlayerViewOfGame = GetPlayerViewOfGame(player),
                StartingPlayerId = Players[LastPlayMadeByPlayerIndex].Id
            });
        }
    }

    
    
    private async Task EndRound()
    {
        IsBetweenRounds = true;
        Players.ForEach(p => p.ScoreRound());
    }

    private async Task<PlayerViewOfGame> PenalizePlayer(GabongPlayer player, int amount, string message,
        PlayerLostTurnReason reason, PenaltyReason penaltyReason)
    {
        if (IsBetweenRounds)
        {
            var abortPenaltyResponse = GetPlayerViewOfGame(player, message);
            await RespondAsync(player.Id, abortPenaltyResponse);
            return abortPenaltyResponse;
        }
    
        var playerIndex = Players.IndexOf(player);
        for (var i = 0; i < amount; i++)
        {
            player.Cards.Add(StockPile.Pop());
            await PlayerDrewPenaltyCard.InvokeOrDefault(() => new PlayerDrewPenaltyCardNotification
                { PlayerId = player.Id, PenaltyReason = penaltyReason});
            player.Penalties++;
        }

        while (CardsToDraw>0)
        {
            CardsToDraw--;
            player.Cards.Add(StockPile.Pop());
            await PlayerDrewCard.InvokeOrDefault(() => new PlayerDrewCardNotification(){ PlayerId = player.Id});
        }
        
        if (CurrentPlayer == player)
        {
            LastPlay = GabongPlay.TurnLost;
            LastPlayMadeByPlayerIndex = playerIndex;
            await PlayerLostTheirTurn.InvokeOrDefault(() => new PlayerLostTheirTurnNotification
            {
                PlayerId = player.Id,
                LostTurnReason = reason
            });
            player.TurnsLost++;
        }

        if (player.Cards.Count > MAX_CARDS_IN_HAND)
        {
            await PlayerHasTooManyCards.InvokeOrDefault(() => new PenalizePlayerForTooManyCardsRequest
                { PlayerId = player.Id });
        }

        var response = GetPlayerViewOfGame(player, message);
        await RespondAsync(player.Id, response);
        return response;
    }

   

    private GabongPlayer? ResolvePlayerById(Guid playerId)
    {
        return Players.FirstOrDefault(x => x.Id == playerId);
    }

    private PlayerViewOfGame GetPlayerViewOfGame(GabongPlayer player, string errorString = null)
    {
        return new PlayerViewOfGame
        {
            Error = errorString,
            Cards = player.Cards,
            TopOfPile = TopOfPile,
            CurrentSuit = CurrentSuit,
            DiscardPileCount = DiscardPile.Count,
            RoundStarted = !IsBetweenRounds,
            StockPileCount = StockPile.Count,
            Players = Players.Select(ToSlimPlayer).ToList(),
            LastPlayMadeByPlayerId = Players[LastPlayMadeByPlayerIndex].Id,
            LastPlay = LastPlay,
            PlayersOrder = Players.Select(x => x.Id).ToList(),
            CardDebtToDraw = CardsToDraw,
        };
    }

    private bool TryGetCurrentPlayer(Guid playerId, [MaybeNullWhen(false)] out GabongPlayer player)
    {
        var p = CurrentPlayer;
        if (p.Id != playerId)
        {
            player = default;
            return false;
        }

        player = p;
        return true;
    }

    private bool TryGetPlayer(Guid playerId, [MaybeNullWhen(false)] out GabongPlayer player)
    {
        player = Players.FirstOrDefault(x => x.Id == playerId);
        return player != null;
    }

 

  

    private static SlimGabongPlayer ToSlimPlayer(GabongPlayer player)
    {
        return new SlimGabongPlayer
        {
            Id = player.Id,
            Name = player.Name,
            NumberOfCards = player.Cards.Count
        };
    }

    private CancellationTokenSource _playIsOngoing = new();
    private Task _clockTask = Task.CompletedTask;
    private DateTime _lastPlayMadeAt = DateTime.UtcNow;


    private string prevTurnMsg = "";
    public override async Task StartAsync()
    {
        _playIsOngoing = new CancellationTokenSource();
        _lastPlayMadeAt = DateTime.UtcNow;

        if (true)
        {
            _clockTask = Task.Run(async () =>
            {
                while (!_playIsOngoing.Token.IsCancellationRequested)
                {
                    await Task.Delay(MAX_MILLIS_PER_TURN / 10);
                    if (IsBetweenRounds)
                    {
                        continue;
                    }

                    var msg = $"Current player: {CurrentPlayer.Name}";
                    if(msg!=prevTurnMsg)
                    {
                        Console.WriteLine(msg);
                        prevTurnMsg = msg;
                    }
                    else
                    {
                        Console.Write(".");
                    }
                    
                    if (State == GameState.Running && LastPlay == GabongPlay.RoundStarted && _lastPlayMadeAt <
                        DateTime.UtcNow.AddSeconds(-MAX_SECONDS_BEFORE_STARTING_ROUND))
                    {
                        await PlayerTookTooLong.InvokeOrDefault(() => new PenalizePlayerForTakingTooLongRequest
                            { PlayerId = CurrentPlayer.Id });
                    }
                    else if (State == GameState.Running &&
                             _lastPlayMadeAt < DateTime.UtcNow.AddMilliseconds(-MAX_MILLIS_PER_TURN))
                    {
                        await PlayerTookTooLong.InvokeOrDefault(() => new PenalizePlayerForTakingTooLongRequest
                            { PlayerId = CurrentPlayer.Id });
                    }
                }

            });
        }

        await NotifyThatRoundHasStarted();
        await GameStarted.InvokeOrDefault(() => new GameStartedNotification { GameId = Id, });
    }

    

    public Task PenalizeSlowPlayer(PenalizePlayerForTakingTooLongRequest e)
    {
        if (!IsBetweenRounds && State == GameState.Running && TryGetPlayer(e.PlayerId, out var player) && player.Id == CurrentPlayer.Id)
        {
            return PenalizePlayer(player, 1, "You took too long to play", PlayerLostTurnReason.TookTooLong, PenaltyReason.TookTooLong);
        }

        return Task.CompletedTask;
    }

    public Task PenalizePlayerWithTooManyCards(PenalizePlayerForTooManyCardsRequest e)
    {
        if (State == GameState.Running && TryGetPlayer(e.PlayerId, out var player) &&
            player.Cards.Count > MAX_CARDS_IN_HAND)
        {
            return HandleMaybeRoundEnded(true);
        }

        return Task.CompletedTask;
    }
}