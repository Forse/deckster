using System.Diagnostics.CodeAnalysis;
using Deckster.Core.Collections;
using Deckster.Core.Games.Bullshit;
using Deckster.Core.Games.Common;


namespace Deckster.Games.Bullshit;

public class BullshitGame : GameObject
{
    public event NotifyPlayer<GameStartedNotification>? GameStarted;
    public event NotifyAll<PlayerDrewCardNotification>? PlayerDrewCard;
    public event NotifyPlayer<ItsYourTurnNotification>? ItsYourTurn;
    public event NotifyAll<ItsPlayersTurnNotification>? ItsPlayersTurn;
    public event NotifyAll<PlayerPassedNotification>? PlayerPassed;
    public event NotifyAll<PlayerPutCardNotification>? PlayerPutCard;
    
    public event NotifyAll<GameEndedNotification>? GameEnded;
    public event NotifyAll<PlayerIsDoneNotification>? PlayerIsDone;
    public event NotifyAll<DiscardPileShuffledNotification>? DiscardPileShuffled;

    public event NotifyAll<BullshitBroadcastNotification>? PlayersBullshitHasBeenCalled;
    public event NotifyPlayer<BullshitPlayerNotification>? YourBullshitHasBeenCalled;

    public event NotifyAll<FalseBullshitCallNotification>? PlayerAccusedFalseBullshit; 
    
    private const int InitialCardCount = 5;
    public List<Card> Deck { get; init; } = [];
    public List<Card> StockPile { get; init; } = [];
    public List<Card> DiscardPile { get; init; } = [];

    public List<BullshitPlayer> Players { get; init; } = [];
    public List<BullshitPlayer> DonePlayers { get; init; } = [];
    public int CurrentPlayerIndex { get; set; }
    
    public Card? ClaimedTopOfPile { get; set; }
    public Card? ActualTopOfPile => DiscardPile.Count == 0 ? null : DiscardPile.Peek();
    public int CardsDrawn { get; set; }
    public PotentialBullshit? PotentialBullshit { get; set; }
    
    public BullshitPlayer CurrentPlayer => State == GameState.Finished ? BullshitPlayer.Null : Players[CurrentPlayerIndex];
    
    protected override GameState GetState() => Players.Count(p => p.IsStillPlaying()) > 1
        ? GameState.Running
        : GameState.Finished;

    public void Deal()
    {
        StockPile.Clear();
        DiscardPile.Clear();
        StockPile.PushRange(Deck);
        for (var ii = 0; ii < InitialCardCount; ii++)
        {
            foreach (var player in Players)
            {
                player.Cards.Push(StockPile.Pop());
            }
        }

        CurrentPlayerIndex = new Random(Seed).Next(0, Players.Count);
    }

    public async Task<EmptyResponse> PutCard(PutCardRequest request)
    {
        IncrementSeed();

        EmptyResponse response;
        if (!TryGetCurrentPlayer(request.PlayerId, out var player))
        {
            response = new EmptyResponse("It is not your turn");
            await RespondAsync(player.Id, response);
            return response;
        }

        if (!player.HasCard(request.ActualCard))
        {
            response = new EmptyResponse("You don't have that card.");
            await RespondAsync(player.Id, response);
            return response;
        }

        if (!CanPut(request.ClaimedToBeCard))
        {
            response = new EmptyResponse($"Cannot put '{request.ClaimedToBeCard}' on '{ClaimedTopOfPile}'.");
            await RespondAsync(player.Id, response);
            return response;
        }

        player.Cards.Remove(request.ActualCard);
        DiscardPile.Push(request.ActualCard);
        ClaimedTopOfPile = request.ClaimedToBeCard;
        response = new EmptyResponse();
        await RespondAsync(player.Id, response);
        
        if (!player.Cards.Any())
        {
            DonePlayers.Add(player);
            await PlayerIsDone.InvokeOrDefault(() => new PlayerIsDoneNotification
            {
                PlayerId = player.Id
            });
        }

        await PlayerPutCard.InvokeOrDefault(() => new PlayerPutCardNotification
        {
            PlayerId = player.Id,
            ClaimedToBeCard = request.ClaimedToBeCard
        });

        await MoveToNextPlayerOrFinishAsync(new PotentialBullshit
        {
            PlayerId = player.Id,
            ClaimedToBeCard = request.ClaimedToBeCard
        });

        return response;
    }
    
    private bool CanPut(Card card)
    {
        if (!ClaimedTopOfPile.HasValue)
        {
            return true;
        }

        var top = ClaimedTopOfPile.Value;
        return top.Suit == card.Suit || top.Rank == card.Rank;
    }

    public async Task<CardResponse> DrawCard(DrawCardRequest request)
    {
        IncrementSeed();

        CardResponse response;
        if (!TryGetCurrentPlayer(request.PlayerId, out var player))
        {
            response = new CardResponse{Error = "It is not your turn"};
            await RespondAsync(request.PlayerId, response);
            return response;
        }
        
        if (CardsDrawn > 2)
        {
            response = new CardResponse{ Error = "You can only draw 3 cards" };
            await RespondAsync(player.Id, response);
            return response;
        }
        
        if (ShufflePileIfNecessary())
        {
            await DiscardPileShuffled.InvokeOrDefault(() => new DiscardPileShuffledNotification());
        }
        
        if (!StockPile.Any())
        {
            response = new CardResponse{ Error = "Stock pile is empty" };
            await RespondAsync(player.Id, response);
            return response;
        }
        
        var card = StockPile.Pop();
        player.Cards.Add(card);
        CardsDrawn++;
        
        response = new CardResponse{ Card = card };
        await RespondAsync(player.Id, response);
        
        await PlayerDrewCard.InvokeOrDefault(new PlayerDrewCardNotification
        {
            PlayerId = player.Id
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
        
        await MoveToNextPlayerOrFinishAsync(null);
        return response;
    }

    public async Task<BullshitResponse> CallBullshit(BullshitRequest request)
    {
        IncrementSeed();
        BullshitResponse response;
        var player = Players.SingleOrDefault(p => p.Id == request.PlayerId);
        if (player == null)
        {
            response = new BullshitResponse {Error = "You don't play this game" };
            await RespondAsync(request.PlayerId, response);
            return response;
        }

        var bullshit = PotentialBullshit;
        if (bullshit == null)
        {
            response = new BullshitResponse { Error = "There is no current potential bullshit" };
            await RespondAsync(request.PlayerId, response);
            return response;
        }
        
        var accused = Players.SingleOrDefault(p => p.Id == bullshit.PlayerId);
        
        if (accused == null)
        {
            // ¯\_(ツ)_/¯
            response = new BullshitResponse { Error = "There is no current potential bullshit" };
            await RespondAsync(request.PlayerId, response);
            return response;
        }

        if (ShufflePileIfNecessary())
        {
            await DiscardPileShuffled.InvokeOrDefault(() => new DiscardPileShuffledNotification());
        }
        
        if (ActualTopOfPile != bullshit.ClaimedToBeCard)
        {
            var card = DiscardPile.Pop();

            var punishmentCards = StockPile.PopUpTo(3).ToArray();

            await YourBullshitHasBeenCalled.InvokeOrDefault(accused.Id, () => new BullshitPlayerNotification
            {
                CalledByPlayerId = player.Id,
                Card = card,
                PunishmentCards = punishmentCards,
            });

            await PlayersBullshitHasBeenCalled.InvokeOrDefault(() => new BullshitBroadcastNotification
            {
                PlayerId = accused.Id,
                ClaimedToBeCard = bullshit.ClaimedToBeCard,
                ActualCard = card,
                PunishmentCardCount = punishmentCards.Length
            });

            response = new BullshitResponse();
            await RespondAsync(request.PlayerId, response);
        }
        else
        {
            response = new BullshitResponse
            {
                PunishmentCards = StockPile.PopUpTo(3).ToArray()
            };
            await RespondAsync(request.PlayerId, response);

            await PlayerAccusedFalseBullshit.InvokeOrDefault(() => new FalseBullshitCallNotification
            {
                PlayerId = player.Id,
                AccusedPlayerId = accused.Id,
                PunishmentCardCount = response.PunishmentCards.Length
            });

        }
        
        return response;
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

    private async Task MoveToNextPlayerOrFinishAsync(PotentialBullshit? potentialBullshit)
    {
        PotentialBullshit = potentialBullshit;
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
                await ItsYourTurn.InvokeOrDefault(CurrentPlayer.Id, new ItsYourTurnNotification());
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
    
    private bool TryGetCurrentPlayer(Guid playerId, [MaybeNullWhen(false)] out BullshitPlayer player)
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
        await ItsYourTurn.InvokeOrDefault(current.Id, () => new ItsYourTurnNotification());
        await ItsPlayersTurn.InvokeOrDefault(() => new ItsPlayersTurnNotification { PlayerId = current.Id });
    }
    
    private PlayerViewOfGame GetPlayerViewOfGame(BullshitPlayer player)
    {
        return new PlayerViewOfGame
        {
            Cards = player.Cards,
            ClaimedTopOfPile = ClaimedTopOfPile,
            DiscardPileCount = DiscardPile.Count,
            StockPileCount = StockPile.Count,
            OtherPlayers = Players.Where(p => p.Id != player.Id).Select(ToOtherPlayer).ToList()
        };
    }

    private static OtherBullshitPlayer ToOtherPlayer(BullshitPlayer player)
    {
        return new OtherBullshitPlayer
        {
            PlayerId = player.Id,
            Name = player.Name,
            NumberOfCards = player.Cards.Count
        };
    }
}