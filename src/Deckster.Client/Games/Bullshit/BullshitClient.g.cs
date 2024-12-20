using Deckster.Core.Games.Bullshit;
using Deckster.Core.Games.Common;
using System.Diagnostics;
using Deckster.Core.Communication;
using Deckster.Core.Protocol;
using Deckster.Core.Extensions;

namespace Deckster.Client.Games.Bullshit;

/**
 * Autogenerated by really, really eager small hamsters.
*/

[DebuggerDisplay("BullshitClient {PlayerData}")]
public class BullshitClient(IClientChannel channel) : GameClient(channel)
{
    public event Action<GameStartedNotification>? GameStarted;
    public event Action<PlayerDrewCardNotification>? PlayerDrewCard;
    public event Action<ItsYourTurnNotification>? ItsYourTurn;
    public event Action<ItsPlayersTurnNotification>? ItsPlayersTurn;
    public event Action<PlayerPassedNotification>? PlayerPassed;
    public event Action<PlayerPutCardNotification>? PlayerPutCard;
    public event Action<GameEndedNotification>? GameEnded;
    public event Action<PlayerIsDoneNotification>? PlayerIsDone;
    public event Action<DiscardPileShuffledNotification>? DiscardPileShuffled;
    public event Action<BullshitBroadcastNotification>? PlayersBullshitHasBeenCalled;
    public event Action<BullshitPlayerNotification>? YourBullshitHasBeenCalled;
    public event Action<FalseBullshitCallNotification>? PlayerAccusedFalseBullshit;

    public Task<EmptyResponse> PutCardAsync(PutCardRequest request, CancellationToken cancellationToken = default)
    {
        return SendAsync<EmptyResponse>(request, false, cancellationToken);
    }

    public Task<CardResponse> DrawCardAsync(DrawCardRequest request, CancellationToken cancellationToken = default)
    {
        return SendAsync<CardResponse>(request, false, cancellationToken);
    }

    public Task<EmptyResponse> PassAsync(PassRequest request, CancellationToken cancellationToken = default)
    {
        return SendAsync<EmptyResponse>(request, false, cancellationToken);
    }

    public Task<BullshitResponse> CallBullshitAsync(BullshitRequest request, CancellationToken cancellationToken = default)
    {
        return SendAsync<BullshitResponse>(request, false, cancellationToken);
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
                case GameEndedNotification m:
                    GameEnded?.Invoke(m);
                    return;
                case PlayerIsDoneNotification m:
                    PlayerIsDone?.Invoke(m);
                    return;
                case DiscardPileShuffledNotification m:
                    DiscardPileShuffled?.Invoke(m);
                    return;
                case BullshitBroadcastNotification m:
                    PlayersBullshitHasBeenCalled?.Invoke(m);
                    return;
                case BullshitPlayerNotification m:
                    YourBullshitHasBeenCalled?.Invoke(m);
                    return;
                case FalseBullshitCallNotification m:
                    PlayerAccusedFalseBullshit?.Invoke(m);
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

public static class BullshitClientConveniences
{
    /// <summary>
    /// does not throw exception on error
    /// </summary>
    public static Task<EmptyResponse> PutCardAsync(this BullshitClient self, Card actualCard, Card claimedToBeCard, CancellationToken cancellationToken = default)
    {
        var request = new PutCardRequest{ ActualCard = actualCard, ClaimedToBeCard = claimedToBeCard };
        return self.SendAsync<EmptyResponse>(request, false, cancellationToken);
    }
    /// <summary>
    /// throws exception on error
    /// </summary>
    public static async Task PutCardOrThrowAsync(this BullshitClient self, Card actualCard, Card claimedToBeCard, CancellationToken cancellationToken = default)
    {
        var request = new PutCardRequest{ ActualCard = actualCard, ClaimedToBeCard = claimedToBeCard };
        var response = await self.SendAsync<EmptyResponse>(request, true, cancellationToken);
    }
    /// <summary>
    /// does not throw exception on error
    /// </summary>
    public static Task<CardResponse> DrawCardAsync(this BullshitClient self, CancellationToken cancellationToken = default)
    {
        var request = new DrawCardRequest{  };
        return self.SendAsync<CardResponse>(request, false, cancellationToken);
    }
    /// <summary>
    /// throws exception on error
    /// </summary>
    public static async Task<Card> DrawCardOrThrowAsync(this BullshitClient self, CancellationToken cancellationToken = default)
    {
        var request = new DrawCardRequest{  };
        var response = await self.SendAsync<CardResponse>(request, true, cancellationToken);
        return response.Card;
    }
    /// <summary>
    /// does not throw exception on error
    /// </summary>
    public static Task<EmptyResponse> PassAsync(this BullshitClient self, CancellationToken cancellationToken = default)
    {
        var request = new PassRequest{  };
        return self.SendAsync<EmptyResponse>(request, false, cancellationToken);
    }
    /// <summary>
    /// throws exception on error
    /// </summary>
    public static async Task PassOrThrowAsync(this BullshitClient self, CancellationToken cancellationToken = default)
    {
        var request = new PassRequest{  };
        var response = await self.SendAsync<EmptyResponse>(request, true, cancellationToken);
    }
    /// <summary>
    /// does not throw exception on error
    /// </summary>
    public static Task<BullshitResponse> CallBullshitAsync(this BullshitClient self, CancellationToken cancellationToken = default)
    {
        var request = new BullshitRequest{  };
        return self.SendAsync<BullshitResponse>(request, false, cancellationToken);
    }
    /// <summary>
    /// throws exception on error
    /// </summary>
    public static async Task<Card[]> CallBullshitOrThrowAsync(this BullshitClient self, CancellationToken cancellationToken = default)
    {
        var request = new BullshitRequest{  };
        var response = await self.SendAsync<BullshitResponse>(request, true, cancellationToken);
        return response.PunishmentCards;
    }
}

public static class BullshitClientDecksterClientExtensions
{
    public static GameApi<BullshitClient> Bullshit(this DecksterClient client)
    {
        return new GameApi<BullshitClient>(client.BaseUri.Append("bullshit"), client.Token, c => new BullshitClient(c));
    }
}
