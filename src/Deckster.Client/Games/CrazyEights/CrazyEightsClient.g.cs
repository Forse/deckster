using Deckster.Core.Games.CrazyEights;
using Deckster.Core.Games.Common;
using System.Diagnostics;
using Deckster.Core.Communication;
using Deckster.Core.Protocol;
using Deckster.Core.Extensions;

namespace Deckster.Client.Games.CrazyEights;

/**
 * Autogenerated by really, really eager small hamsters.
*/

[DebuggerDisplay("CrazyEightsClient {PlayerData}")]
public class CrazyEightsClient(IClientChannel channel) : GameClient(channel)
{
    public event Action<GameStartedNotification>? GameStarted;
    public event Action<PlayerDrewCardNotification>? PlayerDrewCard;
    public event Action<ItsYourTurnNotification>? ItsYourTurn;
    public event Action<ItsPlayersTurnNotification>? ItsPlayersTurn;
    public event Action<PlayerPassedNotification>? PlayerPassed;
    public event Action<PlayerPutCardNotification>? PlayerPutCard;
    public event Action<PlayerPutEightNotification>? PlayerPutEight;
    public event Action<GameEndedNotification>? GameEnded;
    public event Action<PlayerIsDoneNotification>? PlayerIsDone;
    public event Action<DiscardPileShuffledNotification>? DiscardPileShuffled;

    public Task<PlayerViewOfGame> PutCardAsync(PutCardRequest request, CancellationToken cancellationToken = default)
    {
        return SendAsync<PlayerViewOfGame>(request, false, cancellationToken);
    }

    public Task<PlayerViewOfGame> PutEightAsync(PutEightRequest request, CancellationToken cancellationToken = default)
    {
        return SendAsync<PlayerViewOfGame>(request, false, cancellationToken);
    }

    public Task<CardResponse> DrawCardAsync(DrawCardRequest request, CancellationToken cancellationToken = default)
    {
        return SendAsync<CardResponse>(request, false, cancellationToken);
    }

    public Task<EmptyResponse> PassAsync(PassRequest request, CancellationToken cancellationToken = default)
    {
        return SendAsync<EmptyResponse>(request, false, cancellationToken);
    }

    protected override void OnNotification(DecksterNotification notification)
    {
        try
        {
            switch (notification)
            {
                case GameStartedNotification m:
                    GameStarted?.Invoke(m);
                    return;
                case PlayerDrewCardNotification m:
                    PlayerDrewCard?.Invoke(m);
                    return;
                case ItsYourTurnNotification m:
                    ItsYourTurn?.Invoke(m);
                    return;
                case ItsPlayersTurnNotification m:
                    ItsPlayersTurn?.Invoke(m);
                    return;
                case PlayerPassedNotification m:
                    PlayerPassed?.Invoke(m);
                    return;
                case PlayerPutCardNotification m:
                    PlayerPutCard?.Invoke(m);
                    return;
                case PlayerPutEightNotification m:
                    PlayerPutEight?.Invoke(m);
                    return;
                case GameEndedNotification m:
                    GameEnded?.Invoke(m);
                    return;
                case PlayerIsDoneNotification m:
                    PlayerIsDone?.Invoke(m);
                    return;
                case DiscardPileShuffledNotification m:
                    DiscardPileShuffled?.Invoke(m);
                    return;
                default:
                    return;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
}

public static class CrazyEightsClientConveniences
{
    /// <summary>
    /// does not throw exception on error
    /// </summary>
    public static Task<PlayerViewOfGame> PutCardAsync(this CrazyEightsClient self, Card card, CancellationToken cancellationToken = default)
    {
        var request = new PutCardRequest{ Card = card };
        return self.SendAsync<PlayerViewOfGame>(request, false, cancellationToken);
    }
    /// <summary>
    /// throws exception on error
    /// </summary>
    public static async Task<(List<Card> cards, Card topOfPile, Suit currentSuit, int stockPileCount, int discardPileCount, List<OtherCrazyEightsPlayer> otherPlayers)> PutCardOrThrowAsync(this CrazyEightsClient self, Card card, CancellationToken cancellationToken = default)
    {
        var request = new PutCardRequest{ Card = card };
        var response = await self.SendAsync<PlayerViewOfGame>(request, true, cancellationToken);
        return (response.Cards, response.TopOfPile, response.CurrentSuit, response.StockPileCount, response.DiscardPileCount, response.OtherPlayers);
    }
    /// <summary>
    /// does not throw exception on error
    /// </summary>
    public static Task<PlayerViewOfGame> PutEightAsync(this CrazyEightsClient self, Card card, Suit newSuit, CancellationToken cancellationToken = default)
    {
        var request = new PutEightRequest{ Card = card, NewSuit = newSuit };
        return self.SendAsync<PlayerViewOfGame>(request, false, cancellationToken);
    }
    /// <summary>
    /// throws exception on error
    /// </summary>
    public static async Task<(List<Card> cards, Card topOfPile, Suit currentSuit, int stockPileCount, int discardPileCount, List<OtherCrazyEightsPlayer> otherPlayers)> PutEightOrThrowAsync(this CrazyEightsClient self, Card card, Suit newSuit, CancellationToken cancellationToken = default)
    {
        var request = new PutEightRequest{ Card = card, NewSuit = newSuit };
        var response = await self.SendAsync<PlayerViewOfGame>(request, true, cancellationToken);
        return (response.Cards, response.TopOfPile, response.CurrentSuit, response.StockPileCount, response.DiscardPileCount, response.OtherPlayers);
    }
    /// <summary>
    /// does not throw exception on error
    /// </summary>
    public static Task<CardResponse> DrawCardAsync(this CrazyEightsClient self, CancellationToken cancellationToken = default)
    {
        var request = new DrawCardRequest{  };
        return self.SendAsync<CardResponse>(request, false, cancellationToken);
    }
    /// <summary>
    /// throws exception on error
    /// </summary>
    public static async Task<Card> DrawCardOrThrowAsync(this CrazyEightsClient self, CancellationToken cancellationToken = default)
    {
        var request = new DrawCardRequest{  };
        var response = await self.SendAsync<CardResponse>(request, true, cancellationToken);
        return response.Card;
    }
    /// <summary>
    /// does not throw exception on error
    /// </summary>
    public static Task<EmptyResponse> PassAsync(this CrazyEightsClient self, CancellationToken cancellationToken = default)
    {
        var request = new PassRequest{  };
        return self.SendAsync<EmptyResponse>(request, false, cancellationToken);
    }
    /// <summary>
    /// throws exception on error
    /// </summary>
    public static async Task PassOrThrowAsync(this CrazyEightsClient self, CancellationToken cancellationToken = default)
    {
        var request = new PassRequest{  };
        var response = await self.SendAsync<EmptyResponse>(request, true, cancellationToken);
    }
}

public static class CrazyEightsClientDecksterClientExtensions
{
    public static GameApi<CrazyEightsClient> CrazyEights(this DecksterClient client)
    {
        return new GameApi<CrazyEightsClient>(client.BaseUri.Append("crazyeights"), client.Token, c => new CrazyEightsClient(c));
    }
}
