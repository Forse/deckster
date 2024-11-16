using Deckster.Core.Collections;
using Deckster.Core.Games.Common;
using Deckster.Core.Games.Gabong;

namespace Deckster.Games.Gabong;

public partial class GabongGame
{
    private GabongPlayer CalculateCurrentPlayer()
    {
        if (State == GameState.Waiting)
        {
            return GabongPlayer.Null;
        }

        if (State == GameState.Finished)
        {
            return GabongPlayer.Null;
        }

        if (LastPlay == GabongPlay.RoundStarted)
        {
            return PlayerIndexAdjustedBy(0);
        }

        if (LastPlay == GabongPlay.CardPlayed)
        {
            var adjust = DiscardPile.TryPeek(out var top)
                ? top.Rank == 3 ? 2 : 1
                : 1;
            return PlayerIndexAdjustedBy(adjust);
        }

        return PlayerIndexAdjustedBy(1);
    }
    
    private async Task<PlayerViewOfGame> HandlePlay(Card card, GabongPlayer player, Suit? newSuit)
    {
        DiscardPile.Push(card);
        LastPlayMadeByPlayerIndex = Players.IndexOf(player);
        LastPlay = GabongPlay.CardPlayed;
        NewSuit = newSuit;

        if (card.Rank == 2)
        {
            CardsToDraw += 2;
        }
        else
        {
            CardsToDraw = 0;
        }

        if (card.Rank == 13)
        {
            GameDirection *= -1;
        }

        var response = GetPlayerViewOfGame(player);
        await RespondAsync(player.Id, response);

        await PlayerPutCard.InvokeOrDefault(() => new PlayerPutCardNotification
        {
            Card = card,
            PlayerId = player.Id,
            NewSuit = newSuit
        });

        return response;
    }
    
    public async Task<PlayerViewOfGame> DrawCard(DrawCardRequest request)
    {
        IncrementSeed();
        var playerId = request.PlayerId;
        var player = ResolvePlayerById(request.PlayerId);
        if (player == null)
        {
            return new PlayerViewOfGame("You don't exist");
        }

        ShufflePileIfNecessary();

        var card = StockPile.Pop();
        player.Cards.Add(card);
        var drawnCard = StockPile.Pop();
        player.Cards.Add(drawnCard);
        if (CardsToDraw == 0)
        {
            CardsDrawn++;
        }

        if (CurrentPlayer.Id == playerId && CardsToDraw > 0)
        {
            CardsToDraw--;
            player.DebtDrawn++;
            if (CardsToDraw == 0)
            {
                LastPlay = GabongPlay.TurnLost;
                LastPlayMadeByPlayerIndex = Players.IndexOf(player);
                await PlayerLostTheirTurn.InvokeOrDefault(() => new PlayerLostTheirTurnNotification()
                {
                    PlayerId = playerId,
                    LostTurnReason = PlayerLostTurnReason.FinishedDrawingCardDebt
                });
            }
        }

        await PlayerDrewCard.InvokeOrDefault(() => new PlayerDrewCardNotification
        {
            PlayerId = playerId
        });


        var response = GetPlayerViewOfGame(player).WithCardsAddedNotification(drawnCard);
        await RespondAsync(playerId, response);

        if (player.Cards.Count > MAX_CARDS_IN_HAND)
        {
            await PlayerHasTooManyCards.InvokeOrDefault(() => new PenalizePlayerForTooManyCardsRequest
                { PlayerId = player.Id });
        }

        return response;
    }
    
    private bool CanPut(Card card)
    {
        return CurrentSuit == card.Suit ||
               TopOfPile.Rank == card.Rank ||
               card.Rank == 8;
    }

    public void PickFirstGabongMaster()
    {
        //only ever run once on game start - starting player will only change on "Gabong"
        IncrementSeed();
        var random = new Random(Seed);
        var firstGabongMasterIndex = random.Next(Players.Count);
        GabongMasterId = Players[firstGabongMasterIndex].Id;
    }

    public async Task<PlayerViewOfGame> PlayGabong(PlayGabongRequest request)
    {
        var playerId = request.PlayerId;
        var player = ResolvePlayerById(playerId);
        if (player == null)
        {
            return new PlayerViewOfGame("You don't exist");
        }

        if (GabongCalculator.IsGabong(TopOfPile.Rank, player.Cards.Select(x => x.Rank)))
        {
            player.Gabongs++;
            player.Score -= 5;
            player.Cards.Clear();
            GabongMasterId = playerId;
            return await HandleMaybeRoundEnded()
                   ?? new PlayerViewOfGame("Round ended");
        }
        else
        {
            return await PenalizePlayer(player, 2, "NO! You don't have Gabong", PlayerLostTurnReason.WrongPlay, PenaltyReason.WrongGabong);
        }
    }

    public async Task<PlayerViewOfGame> PlayBonga(PlayBongaRequest request)
    {
        var playerId = request.PlayerId;
        var player = ResolvePlayerById(playerId);
        if (player == null)
        {
            return new PlayerViewOfGame("You don't exist");
        }

        int i = 1;
        bool goOn = true;
        bool success = false;
        while (!success && goOn && i < DiscardPile.Count)
        {
            var target = DiscardPile.Take(i).Sum(x => x.Rank);
            if (target > 14)
            {
                goOn = false;
            }

            success = GabongCalculator.IsGabong(target, player.Cards.Select(x => x.Rank));
        }

        if (success)
        {
            player.Bongas++;
            player.Score -= 5;
            player.Cards.Clear();
            return await HandleMaybeRoundEnded()
                   ?? new PlayerViewOfGame("Round ended");
        }

        return await PenalizePlayer(player, 2, "NO! You don't have bonga", PlayerLostTurnReason.WrongPlay, PenaltyReason.WrongBonga);
    }
    
    public async Task<PlayerViewOfGame> Pass(PassRequest request)
    {
        IncrementSeed();
        var playerId = request.PlayerId;
        if (!TryGetCurrentPlayer(playerId, out var player))
        {
            var errorResponse = new PlayerViewOfGame("It is not your turn");
            await RespondAsync(playerId, errorResponse);
            return errorResponse;
        }

        if (CardsDrawn == 0)
        {
            var errorResponse = await PenalizePlayer(player, 1, "You have to draw a card first", PlayerLostTurnReason.WrongPlay, PenaltyReason.PassWithoutDrawing);
            return errorResponse;
        }

        await PlayerLostTheirTurn.InvokeOrDefault(() => new PlayerLostTheirTurnNotification()
        {
            PlayerId = playerId,
            LostTurnReason = PlayerLostTurnReason.Passed
        });
        player.Passes++;
        var okResponse = GetPlayerViewOfGame(player);
        await RespondAsync(playerId, okResponse);
        return okResponse;
    }
    
    public async Task<PlayerViewOfGame> PutCard(PutCardRequest request)
    {
        IncrementSeed();

        var card = request.Card;

        if (card.IsJoker())
        {
            Console.Write("WHAT?? SERVER GOT JOKER??");
        }
        
        TryGetPlayer(request.PlayerId, out var player);
        if (player == null)
        {
            var wtf = new PlayerViewOfGame($"You don't exist");
            await RespondAsync(request.PlayerId, wtf);
            return wtf;
        }

        if (!player.HasCard(card))
        {
            return await PenalizePlayer(player, 1, $"NO! You don't have '{card}'", PlayerLostTurnReason.WrongPlay, PenaltyReason.WrongPlay);
        }

        if (CurrentPlayer != player && !card.Equals(TopOfPile))
        {
            return await PenalizePlayer(player, 1, "NO! It is not your turn", PlayerLostTurnReason.WrongPlay, PenaltyReason.PlayOutOfTurn);
        }

        if (CurrentPlayer != player && card.Equals(TopOfPile))
        {
            player.Shots++;
        }

        if (!CanPut(card))
        {
            return await PenalizePlayer(player, 1, $"NO! Cannot put '{card}' on '{TopOfPile}'", PlayerLostTurnReason.WrongPlay, PenaltyReason.WrongPlay);
        }

        if (card.Rank != 8 && request.NewSuit.HasValue)
        {
            return await PenalizePlayer(player, 1, $"NO! Cannot change suit with a '{card}'", PlayerLostTurnReason.WrongPlay, PenaltyReason.WrongPlay);
        }

        if (CardsToDraw > 0 && card.Rank != 2)
        {
            return await PenalizePlayer(player, 1, $"NO! You have to draw {Math.Abs(CardsToDraw)} more cards", PlayerLostTurnReason.WrongPlay, PenaltyReason.UnpaidDebt);
        }

        player.CardsPlayed++;
        player.Cards.Remove(card);
        return await HandleMaybeRoundEnded()
               ?? await HandlePlay(card, player, null);
    }
}