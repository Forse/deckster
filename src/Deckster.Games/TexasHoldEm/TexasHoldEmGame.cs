using Deckster.Core.Games.Common;
using Deckster.Core.Games.TexasHoldEm;
using System.Diagnostics.CodeAnalysis;
using Deckster.Core.Collections;

namespace Deckster.Games.TexasHoldEm;

public class TexasHoldEmGame : GameObject
{
    public event NotifyPlayer<GameStartedNotification>? GameStarted;
    public event NotifyPlayer<ItsYourTurnNotification>? ItsYourTurn;

    public event NotifyPlayer<NewRoundStartedNotification>? RoundStarted;
    public event NotifyAll<RoundEndedNotification>? RoundEnded;

    public event NotifyAll<PlayerBettedNotification>? PlayerBetted;
    public event NotifyAll<PlayerAllInNotification>? PlayerAllIn;
    public event NotifyAll<PlayerCheckedNotification>? PlayerChecked;
    public event NotifyAll<PlayerFoldedNotification>? PlayerFolded;
    public event NotifyAll<PlayerCalledNotification>? PlayerCalled;
    public event NotifyAll<NewCardsRevealed>? NewCardsRevealed;
    public event NotifyAll<GameEndedNotification>? GameEnded;

    // "constants" for game
    public int StartingBigBlind { get; private init; }
    public int StartingStackSize { get; private init; }
    public int NumbersOfRoundsBeforeBlindDoubling { get; private init; }
    public int MaxNumberOfHands { get; private init; }
    public List<TexasHoldEmPlayer> Players { get; init; } = [];

    // Game state
    public bool IsStarted { get; private set; }
    public int StartingPlayerIndex { get; set; }
    public int CurrentPlayerIndex { get; set; }
    public List<Card> Deck { get; private set; } = new();
    public List<Card> CardsInPlay { get; private set; } = new();
    public int CurrentBigBlind { get; private set; }
    public int CurrentSmallBlind { get; private set; }
    public int RoundNumber { get; private set; }

    public TexasHoldEmPlayer CurrentPlayer => State == GameState.Finished ? TexasHoldEmPlayer.Null : Players[CurrentPlayerIndex];

    public static TexasHoldEmGame Instantiate(TexasHoldEmCreatedEvent created)
    {
        var game = new TexasHoldEmGame
        {
            Id = created.Id,
            Name = created.Name,
            StartedTime = created.StartedTime,
            Seed = created.InitialSeed,
            Deck = created.Deck,
            Players = created.Players.Select(p => new TexasHoldEmPlayer(p.Id, p.Name, created.StackSize)).ToList(),
            StartingStackSize = created.StackSize,
            StartingBigBlind = created.BigBlindSize,
            NumbersOfRoundsBeforeBlindDoubling = 4,
            MaxNumberOfHands = created.NumberOfRounds,
            IsStarted = false,
            CurrentBigBlind = created.BigBlindSize,
            CurrentSmallBlind = created.BigBlindSize / 2,
    };

        return game;
    }

    private void StartNewRound()
    {
        RoundNumber++;

        if (RoundNumber % NumbersOfRoundsBeforeBlindDoubling == 0)
        {
            CurrentBigBlind *= 2;
            CurrentSmallBlind *= 2;
        }

        Deck.AddRange(CardsInPlay.PopUpTo(CardsInPlay.Count));
        foreach (var player in Players)
        {
            Deck.AddRange(player.Cards.PopUpTo(2));
        }

        Deck = Deck.KnuthShuffle(Seed);
        CurrentPlayerIndex = StartingPlayerIndex;
        foreach (var player in Players)
        {
            var cards = Deck.PopUpTo(2);
            player.ResetRound(cards, CurrentBigBlind);
        }

        CurrentPlayer.Bet(CurrentSmallBlind);
        MoveToNextPlayer();
        CurrentPlayer.Bet(CurrentBigBlind);
        MoveToNextPlayer();
    }

    public async Task<PlayerViewOfGame> PlayerBetRequest(BetRequest request)
    {
        IncrementSeed();
        var playerId = request.PlayerId;
        var bet = request.Bet;
        var maxBet = Players.Max(i => i.CurrentRoundBet);

        PlayerViewOfGame response;
        if (!TryGetCurrentPlayer(playerId, out var player))
        {
            response = new PlayerViewOfGame { Error = "It is not your turn" };
            await RespondAsync(playerId, response);
            return response;
        }

        if (player.CurrentRoundBet + bet <= maxBet && CurrentPlayer.StackSize < bet)
        {
            response = new PlayerViewOfGame { Error = "Insufficient bet. You are not going all in or exceeding biggest bet." };
            await RespondAsync(playerId, response);
            return response;
        }

        player.Bet(bet);

        response = GetPlayerViewOfGame(player);
        await RespondAsync(playerId, response);

        if (player.IsAllIn)
        {
            await PlayerAllIn.InvokeOrDefault(new PlayerAllInNotification { PlayerId = playerId });
        }
        else
        {
            await PlayerBetted.InvokeOrDefault(new PlayerBettedNotification { BetSize = bet, PlayerId = playerId });
        }

        await MoveToNextPlayerOrFinishAsync();
        return response;
    }

    public async Task<PlayerViewOfGame> PlayerFoldRequest(FoldRequest request)
    {
        IncrementSeed();
        var playerId = request.PlayerId;
       
        PlayerViewOfGame response;
        if (!TryGetCurrentPlayer(playerId, out var player))
        {
            response = new PlayerViewOfGame { Error = "It is not your turn" };
            await RespondAsync(playerId, response);
            return response;
        }

        player.Fold();

        response = GetPlayerViewOfGame(player);
        await RespondAsync(playerId, response);

        await PlayerFolded.InvokeOrDefault(new PlayerFoldedNotification { PlayerId = playerId });

        await MoveToNextPlayerOrFinishAsync();
        return response;
    }

    public async Task<PlayerViewOfGame> PlayerCheckRequest(CheckRequest request)
    {
        IncrementSeed();
        var playerId = request.PlayerId;
        var maxBet = Players.Max(i => i.CurrentRoundBet);

        PlayerViewOfGame response;
        if (!TryGetCurrentPlayer(playerId, out var player))
        {
            response = new PlayerViewOfGame { Error = "It is not your turn" };
            await RespondAsync(playerId, response);
            return response;
        }

        if (player.CurrentRoundBet != maxBet)
        {
            response = new PlayerViewOfGame { Error = "You can not check if your current bet does not match biggest bet" };
            await RespondAsync(playerId, response);
            return response;
        }

        player.Check();

        response = GetPlayerViewOfGame(player);
        await RespondAsync(playerId, response);

        await PlayerChecked.InvokeOrDefault(new PlayerCheckedNotification { PlayerId = playerId });

        await MoveToNextPlayerOrFinishAsync();
        return response;
    }

    public async Task<PlayerViewOfGame> PlayerCallRequest(CallRequest request)
    {
        IncrementSeed();
        var playerId = request.PlayerId;
        var maxBet = Players.Max(i => i.CurrentRoundBet);

        PlayerViewOfGame response;
        if (!TryGetCurrentPlayer(playerId, out var player))
        {
            response = new PlayerViewOfGame { Error = "It is not your turn" };
            await RespondAsync(playerId, response);
            return response;
        }

        player.Call(maxBet);
        
        response = GetPlayerViewOfGame(player);
        await RespondAsync(playerId, response);

        await PlayerCalled.InvokeOrDefault(new PlayerCalledNotification { PlayerId = playerId });
        
        await MoveToNextPlayerOrFinishAsync();
        return response;
    }

    protected override GameState GetState()
    {
        if (!IsStarted)
        {
            return GameState.Waiting;
        }

        var eliminatedPlayers = Players.Where(i => i.IsEliminated).ToList();
        if (eliminatedPlayers.Count == Players.Count - 1)
        {
            return GameState.Finished;
        }

        if (RoundNumber > MaxNumberOfHands)
        {
            return GameState.Finished;
        }

        if (IsBettingDone() && CardsInPlay.Count == 5)
        {
            return GameState.RoundFinished;
        }

        return GameState.Running;
    }

    public override async Task StartAsync()
    {
        IsStarted = true;
        StartNewRound();

        foreach (var player in Players)
        {
            await GameStarted.InvokeOrDefault(player.Id, () => new GameStartedNotification
            {
                GameId = Id,
                PlayerViewOfGame = GetPlayerViewOfGame(player)
            });
        }

        await ItsYourTurn.InvokeOrDefault(CurrentPlayer.Id, () => new ItsYourTurnNotification
        {
            PlayerViewOfGame = GetPlayerViewOfGame(CurrentPlayer)
        });
    }

    private bool TryGetCurrentPlayer(Guid playerId, [MaybeNullWhen(false)] out TexasHoldEmPlayer player)
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

    private PlayerViewOfGame GetPlayerViewOfGame(TexasHoldEmPlayer player)
    {
        return new PlayerViewOfGame
        {
            Cards = player.Cards,
            CardsOnTable = CardsInPlay,
            StackSize = player.StackSize,
            CurrentBet = player.CurrentRoundBet,
            BigBlind = CurrentBigBlind,
            NumberOfRoundsUntilBigBlindIncreases = -1,
            OtherPlayers = Players.Where(p => p.Id != player.Id).Select(ToOtherPlayer).ToList()
        };
    }

    private static OtherPokerPlayers ToOtherPlayer(TexasHoldEmPlayer player)
    {
        return new OtherPokerPlayers
        {
            PlayerId = player.Id,
            Name = player.Name,
            CurrentBet = player.CurrentRoundBet,
            HasChecked = player.HasChecked,
            HasFolded = player.HasFolded,
            IsAllIn = player.IsAllIn,
            StackSize = player.StackSize,
        };
    }

    private async Task MoveToNextPlayerOrFinishAsync()
    {
        var gameState = GetState();
        if (gameState == GameState.Finished)
        {
            await GameEnded.InvokeOrDefault(new GameEndedNotification());
            return;
        }

        if (gameState == GameState.Waiting)
        {
            return;
        }

        if (gameState == GameState.RoundFinished)
        {
            var winner = Winner();
            var allWinnings = Players.Sum(i => i.CurrentRoundBet);
            winner.GiveWinnings(allWinnings);
            await RoundEnded.InvokeOrDefault(new RoundEndedNotification { WinnerId = winner.Id, Winnings = allWinnings });

            StartNewRound();
            var updatedGameState = GetState();
            if (updatedGameState == GameState.Finished)
            {
                await GameEnded.InvokeOrDefault(new GameEndedNotification());
                return;
            }

            foreach (var player in Players)
            {
                await RoundStarted.InvokeOrDefault(player.Id, () => new NewRoundStartedNotification()
                {
                    GameId = Id,
                    PlayerViewOfGame = GetPlayerViewOfGame(player)
                });
            }

            await ItsYourTurn.InvokeOrDefault(CurrentPlayer.Id, new ItsYourTurnNotification
            {
                PlayerViewOfGame = GetPlayerViewOfGame(CurrentPlayer)
            });

            return;
        }

        if (IsBettingDone())
        {
            List<Card> cards = [];
            if (CardsInPlay.Count == 0)
            {
                cards.AddRange(Deck.PopUpTo(3));
            }
            else
            {
                cards.Add(Deck.Pop());
            }

            CardsInPlay.AddRange(cards);
            await NewCardsRevealed.InvokeOrDefault(new NewCardsRevealed { Cards = cards });
            CurrentPlayerIndex = StartingPlayerIndex;
            if (!CurrentPlayer.IsStillInRound())
            {
                CurrentPlayerIndex = GetNextPlayerIndex();
            }

            await ItsYourTurn.InvokeOrDefault(CurrentPlayer.Id, new ItsYourTurnNotification
            {
                PlayerViewOfGame = GetPlayerViewOfGame(CurrentPlayer)
            });
        }
        else
        {
            MoveToNextPlayer();
            await ItsYourTurn.InvokeOrDefault(CurrentPlayer.Id, new ItsYourTurnNotification
            {
                PlayerViewOfGame = GetPlayerViewOfGame(CurrentPlayer)
            });
        }
    }

    private void MoveToNextPlayer()
    {
        CurrentPlayerIndex = GetNextPlayerIndex();
    }

    private int GetNextPlayerIndex()
    {
        var index = CurrentPlayerIndex;
        var foundNext = false;
        while (!foundNext)
        {
            index++;
            if (index >= Players.Count)
            {
                index = 0;
            }

            foundNext =  Players[index].IsStillInRound() && !Players[index].IsAllIn;
        }

        return index;
    }

    private bool IsBettingDone()
    {
        var playersStillInGame = Players.Where(p => p.IsStillInRound()).ToList();
        if (playersStillInGame.Count == 1)
        {
            return true;
        }

        var currentBet = playersStillInGame.Max(i => i.CurrentRoundBet);
        foreach (var player in playersStillInGame)
        {
            if (player.CurrentRoundBet == currentBet || player.IsAllIn)
            {
                continue;
            }
            else
            {
                return false;
            }
        }

        return true;
    }

    private TexasHoldEmPlayer Winner()
    {
        var playersStillInGame = Players.Where(p => p.IsStillInRound()).ToList();
        if (playersStillInGame.Count == 1)
        {
            return playersStillInGame.Single();
        }

        return playersStillInGame.OrderBy(i => i.HandScore()).First();
    }
}