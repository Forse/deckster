using System.Diagnostics.CodeAnalysis;
using Deckster.Core.Collections;
using Deckster.Core.Games.Common;
using Deckster.Core.Games.CrazyEights;

namespace Deckster.Games.CrazyEights;

public class CrazyEightsGame : GameObject
{
    public event NotifyPlayer<GameStartedNotification>? GameStarted;
    public event NotifyAll<PlayerDrewCardNotification>? PlayerDrewCard;
    public event NotifyPlayer<ItsYourTurnNotification>? ItsYourTurn;
    public event NotifyAll<ItsPlayersTurnNotification>? ItsPlayersTurn;
    public event NotifyAll<PlayerPassedNotification>? PlayerPassed;
    public event NotifyAll<PlayerPutCardNotification>? PlayerPutCard;
    public event NotifyAll<PlayerPutEightNotification>? PlayerPutEight;
    public event NotifyAll<GameEndedNotification>? GameEnded;
    public event NotifyAll<PlayerIsDoneNotification>? PlayerIsDone;
    public event NotifyAll<DiscardPileShuffledNotification>? DiscardPileShuffled; 
    
    public int InitialCardsPerPlayer { get; set; } = 5;
    public int CurrentPlayerIndex { get; set; }
    public int CardsDrawn { get; set; }
    
    /// <summary>
    /// Done players
    /// </summary>
    public List<CrazyEightsPlayer> DonePlayers { get; init; } = [];
    
    protected override GameState GetState() => Players.Count(p => p.IsStillPlaying()) > 1 ? GameState.Running : GameState.Finished;

    /// <summary>
    /// All the (shuffled) cards in the game
    /// </summary>
    public List<Card> Deck { get; init; } = [];

    /// <summary>
    /// Where players draw cards from
    /// </summary>
    public List<Card> StockPile { get; init; } = new();
    
    /// <summary>
    /// Where players put cards
    /// </summary>
    public List<Card> DiscardPile { get; init; } = new();

    /// <summary>
    /// All the players
    /// </summary>
    public List<CrazyEightsPlayer> Players { get; init; } = [];
    public List<CrazyEightsSpectator> Spectators { get; init; } = [];

    public Suit? NewSuit { get; set; }
    public Card TopOfPile => DiscardPile.Peek();
    public Suit CurrentSuit => NewSuit ?? TopOfPile.Suit;

    public CrazyEightsPlayer CurrentPlayer => State == GameState.Finished ? CrazyEightsPlayer.Null : Players[CurrentPlayerIndex];

    public static CrazyEightsGame Instantiate(CrazyEightsGameCreatedEvent created)
    {
        var game = new CrazyEightsGame
        {
            Id = created.Id,
            Name = created.Name,
            StartedTime = created.StartedTime,
            Seed = created.InitialSeed,
            Deck = created.Deck,
            Players = created.Players.Select(p => new CrazyEightsPlayer
            {
                Id = p.Id,
                Name = p.Name
            }).ToList()
        };
        game.Reset();

        return game;
    }
    
    private void Reset()
    {
        foreach (var player in Players)
        {
            player.Cards.Clear();
        }
        
        DonePlayers.Clear();
        StockPile.Clear();
        StockPile.PushRange(Deck);
        for (var ii = 0; ii < InitialCardsPerPlayer; ii++)
        {
            foreach (var player in Players)
            {
                player.Cards.Add(StockPile.Pop());
            }
        }
        
        DiscardPile.Clear();
        DiscardPile.Push(StockPile.Pop());
        DonePlayers.Clear();
        CurrentPlayerIndex = new Random(Seed).Next(0, Players.Count);
    }

    public async Task<PlayerViewOfGame> PutCard(PutCardRequest request)
    {
        IncrementSeed();
        var playerId = request.PlayerId;
        var card = request.Card;
        
        PlayerViewOfGame response;
        if (!TryGetCurrentPlayer(playerId, out var player))
        {
            response = new PlayerViewOfGame { Error = "It is not your turn" };
            await RespondAsync(playerId, response);
            return response;
        }

        if (!player.HasCard(card))
        {
            response = new PlayerViewOfGame { Error = $"You don't have '{card}'" };
            await RespondAsync(playerId, response);
            return response;
        }

        if (!CanPut(card))
        {
            response = new PlayerViewOfGame{ Error = $"Cannot put '{card}' on '{TopOfPile}'" };
            await RespondAsync(playerId, response);
            return response;
        }
        
        player.Cards.Remove(card);
        DiscardPile.Push(card);
        NewSuit = null;
        if (!player.Cards.Any())
        {
            DonePlayers.Add(player);
        }

        response = GetPlayerViewOfGame(player);
        await RespondAsync(playerId, response);

        await PlayerPutCard.InvokeOrDefault(() => new PlayerPutCardNotification {PlayerId = playerId, Card = card});

        await MoveToNextPlayerOrFinishAsync();
        
        return response;
    }

    public async Task<PlayerViewOfGame> PutEight(PutEightRequest request)
    {
        IncrementSeed();
        var playerId = request.PlayerId;
        var card = request.Card;
        var newSuit = request.NewSuit;
        
        PlayerViewOfGame response;
        if (!TryGetCurrentPlayer(playerId, out var player))
        {
            response = new PlayerViewOfGame("It is not your turn");
            await RespondAsync(playerId, response);
            return response;
        }

        if (!player.HasCard(card))
        {
            response = new PlayerViewOfGame($"You don't have '{card}'");
            await RespondAsync(playerId, response);
            return response;
        }
        
        if (card.Rank != 8)
        {
            response = new PlayerViewOfGame("Card rank must be '8'");
            await RespondAsync(playerId, response);
            return response;
        }

        if (!CanPut(card))
        {
            response = NewSuit.HasValue
                ? new PlayerViewOfGame($"Cannot put '{card}' on '{TopOfPile}' (new suit: '{NewSuit.Value}')")
                : new PlayerViewOfGame($"Cannot put '{card}' on '{TopOfPile}'");
            await RespondAsync(playerId, response);
            return response;
        }

        player.Cards.Remove(card);
        DiscardPile.Push(card);
        NewSuit = newSuit != card.Suit ? newSuit : null;
        
        response = GetPlayerViewOfGame(player);
        await RespondAsync(playerId, response);

        await PlayerPutEight.InvokeOrDefault(new PlayerPutEightNotification
        {
            PlayerId = player.Id,
            Card = request.Card,
            NewSuit = request.NewSuit
        });
        
        if (!player.Cards.Any())
        {
            DonePlayers.Add(player);
            
            await PlayerIsDone.InvokeOrDefault(new PlayerIsDoneNotification
            {
                PlayerId = playerId
            });
        }

        await MoveToNextPlayerOrFinishAsync();
        return response;
    }

    

    public async Task<CardResponse> DrawCard(DrawCardRequest request)
    {
        IncrementSeed();
        var playerId = request.PlayerId;
        CardResponse response;
        if (!TryGetCurrentPlayer(playerId, out var player))
        {
            response = new CardResponse{ Error = "It is not your turn" }; 
            await RespondAsync(playerId, response);
            return response;
        }
        
        if (CardsDrawn > 2)
        {
            response = new CardResponse{ Error = "You can only draw 3 cards" };
            await RespondAsync(playerId, response);
            return response;
        }

        if (ShufflePileIfNecessary())
        {
            await DiscardPileShuffled.InvokeOrDefault(() => new DiscardPileShuffledNotification());
        }
        
        if (!StockPile.Any())
        {
            response = new CardResponse{ Error = "Stock pile is empty" };
            await RespondAsync(playerId, response);
            return response;
        }
        var card = StockPile.Pop();
        player.Cards.Add(card);
        CardsDrawn++;
        
        response = new CardResponse(card);
        await RespondAsync(playerId, response);

        await PlayerDrewCard.InvokeOrDefault(new PlayerDrewCardNotification
        {
            PlayerId = playerId
        });
        return response;
    }

    public async Task<EmptyResponse> Pass(PassRequest request)
    {
        IncrementSeed();
        var playerId = request.PlayerId;
        EmptyResponse response;
        if (!TryGetCurrentPlayer(playerId, out var player))
        {
            response = new EmptyResponse("It is not your turn");
            await RespondAsync(playerId, response);
            return response;
        }

        if (player.Cards.Any(CanPut))
        {
            response = new EmptyResponse("You must play one of your cards");
            await RespondAsync(playerId, response);
            return response;
        }

        if (CardsDrawn < 3)
        {
            response = new EmptyResponse("You must draw a card");
            await RespondAsync(playerId, response);
            return response;
        }
        
        response = EmptyResponse.Ok;
        await RespondAsync(playerId, response);

        await PlayerPassed.InvokeOrDefault(new PlayerPassedNotification
        {
            PlayerId = playerId
        });
        
        await MoveToNextPlayerOrFinishAsync();
        return response;
    }
    
    private async Task MoveToNextPlayerOrFinishAsync()
    {
        var stillPlaying = Players.Where(p => p.IsStillPlaying()).ToArray();
        switch (stillPlaying.Length)
        {
            case 0:
                await GameEnded.InvokeOrDefault(() => new GameEndedNotification
                {
                    LoserId = default // ¯\_(ツ)_/¯
                });
                break;
            case 1:
                await GameEnded.InvokeOrDefault(() => new GameEndedNotification
                {
                    LoserId = stillPlaying[0].Id,
                    LoserName = stillPlaying[0].Name,
                    Players = Players.Select(p => new PlayerData
                    {
                        Id = p.Id,
                        Name = p.Name,
                        CardsInHand = p.Cards.Count
                    }).ToList()
                });
                break;
            default:
                MoveToNextPlayer();
                await ItsYourTurn.InvokeOrDefault(CurrentPlayer.Id, new ItsYourTurnNotification
                {
                    PlayerViewOfGame = GetPlayerViewOfGame(CurrentPlayer)
                });
                await ItsPlayersTurn.InvokeOrDefault(() => new ItsPlayersTurnNotification
                {
                    PlayerId = CurrentPlayer.Id
                });
            break;
        }
    }

    private void MoveToNextPlayer()
    {
        if (Players.Count(p => p.IsStillPlaying()) < 2)
        {
            return;
        }

        var foundNext = false;
        
        var index = CurrentPlayerIndex;
        while (!foundNext)
        {
            index++;
            if (index >= Players.Count)
            {
                index = 0;
            }

            foundNext = Players[index].IsStillPlaying();
        }

        CurrentPlayerIndex = index;
        CardsDrawn = 0;
    }    

    private PlayerViewOfGame GetPlayerViewOfGame(CrazyEightsPlayer player)
    {
        return new PlayerViewOfGame
        {
            Cards = player.Cards,
            TopOfPile = TopOfPile,
            CurrentSuit = CurrentSuit,
            DiscardPileCount = DiscardPile.Count,
            StockPileCount = StockPile.Count,
            OtherPlayers = Players.Where(p => p.Id != player.Id).Select(ToOtherPlayer).ToList()
        };
    }

    private bool TryGetCurrentPlayer(Guid playerId, [MaybeNullWhen(false)] out CrazyEightsPlayer player)
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

    private bool CanPut(Card card)
    {
        return CurrentSuit == card.Suit ||
               TopOfPile.Rank == card.Rank ||
               card.Rank == 8;
    }
    
    private bool ShufflePileIfNecessary()
    {
        if (StockPile.Any())
        {
            return false;
        }
        
        if (DiscardPile.Count < 2)
        {
            return false;
        }

        var topOfPile = DiscardPile.Pop();
        var reshuffledCards = DiscardPile.ToList().KnuthShuffle(Seed);
        DiscardPile.Clear();
        DiscardPile.Push(topOfPile);
        StockPile.PushRange(reshuffledCards);
        return true;
    }

    private static OtherCrazyEightsPlayer ToOtherPlayer(CrazyEightsPlayer player)
    {
        return new OtherCrazyEightsPlayer
        {
            PlayerId = player.Id,
            Name = player.Name,
            NumberOfCards = player.Cards.Count
        };
    }

    public override async Task StartAsync()
    {
        foreach (var player in Players)
        {
            await GameStarted.InvokeOrDefault(player.Id, () => new GameStartedNotification
            {
                GameId = Id,
                PlayerViewOfGame = GetPlayerViewOfGame(player)
            });
        }

        var current = CurrentPlayer;
        await ItsYourTurn.InvokeOrDefault(current.Id, () => new ItsYourTurnNotification
        {
            PlayerViewOfGame = GetPlayerViewOfGame(current)
        });
        await ItsPlayersTurn.InvokeOrDefault(() => new ItsPlayersTurnNotification {PlayerId = current.Id});
    }
}

public class CrazyEightsSpectator
{
    public Guid Id { get; init; }
    public string Name { get; init; } = "";

    public static readonly CrazyEightsSpectator Null = new()
    {
        Id = Guid.Parse("6D31A8DA-5766-458A-B113-8D0444BBEDBD"),
        Name = "PST"
    };
}