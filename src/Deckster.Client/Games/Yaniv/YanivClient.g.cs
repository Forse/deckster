using Deckster.Core.Games.Yaniv;
using Deckster.Core.Games.Common;
using System.Diagnostics;
using Deckster.Core.Communication;
using Deckster.Core.Protocol;
using Deckster.Core.Extensions;

namespace Deckster.Client.Games.Yaniv;

/**
 * Autogenerated by really, really eager small hamsters.
*/

[DebuggerDisplay("YanivClient {PlayerData}")]
public class YanivClient(IClientChannel channel) : GameClient(channel)
{
    public event Action<PlayerPutCardsNotification>? PlayerPutCards;
    public event Action<ItsYourTurnNotification>? ItsYourTurn;
    public event Action<RoundStartedNotification>? RoundStarted;
    public event Action<RoundEndedNotification>? RoundEnded;
    public event Action<GameEndedNotification>? GameEnded;
    public event Action<DiscardPileShuffledNotification>? DiscardPileShuffled;

    public Task<EmptyResponse> CallYanivAsync(CallYanivRequest request, CancellationToken cancellationToken = default)
    {
        return SendAsync<EmptyResponse>(request, false, cancellationToken);
    }

    public Task<PutCardsResponse> PutCardsAsync(PutCardsRequest request, CancellationToken cancellationToken = default)
    {
        return SendAsync<PutCardsResponse>(request, false, cancellationToken);
    }

    protected override void OnNotification(DecksterNotification notification)
    {
        try
        {
            switch (notification)
            {
                case PlayerPutCardsNotification m:
                    PlayerPutCards?.Invoke(m);
                    return;
                case ItsYourTurnNotification m:
                    ItsYourTurn?.Invoke(m);
                    return;
                case RoundStartedNotification m:
                    RoundStarted?.Invoke(m);
                    return;
                case RoundEndedNotification m:
                    RoundEnded?.Invoke(m);
                    return;
                case GameEndedNotification m:
                    GameEnded?.Invoke(m);
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

public static class YanivClientConveniences
{
    /// <summary>
    /// does not throw exception on error
    /// </summary>
    public static Task<EmptyResponse> CallYanivAsync(this YanivClient self, CancellationToken cancellationToken = default)
    {
        var request = new CallYanivRequest{  };
        return self.SendAsync<EmptyResponse>(request, false, cancellationToken);
    }
    /// <summary>
    /// throws exception on error
    /// </summary>
    public static async Task CallYanivOrThrowAsync(this YanivClient self, CancellationToken cancellationToken = default)
    {
        var request = new CallYanivRequest{  };
        var response = await self.SendAsync<EmptyResponse>(request, true, cancellationToken);
    }
    /// <summary>
    /// does not throw exception on error
    /// </summary>
    public static Task<PutCardsResponse> PutCardsAsync(this YanivClient self, Card[] cards, DrawCardFrom drawCardFrom, CancellationToken cancellationToken = default)
    {
        var request = new PutCardsRequest{ Cards = cards, DrawCardFrom = drawCardFrom };
        return self.SendAsync<PutCardsResponse>(request, false, cancellationToken);
    }
    /// <summary>
    /// throws exception on error
    /// </summary>
    public static async Task<Card> PutCardsOrThrowAsync(this YanivClient self, Card[] cards, DrawCardFrom drawCardFrom, CancellationToken cancellationToken = default)
    {
        var request = new PutCardsRequest{ Cards = cards, DrawCardFrom = drawCardFrom };
        var response = await self.SendAsync<PutCardsResponse>(request, true, cancellationToken);
        return response.DrawnCard;
    }
}

public static class YanivClientDecksterClientExtensions
{
    public static GameApi<YanivClient> Yaniv(this DecksterClient client)
    {
        return new GameApi<YanivClient>(client.BaseUri.Append("yaniv"), client.Token, c => new YanivClient(c));
    }
}
