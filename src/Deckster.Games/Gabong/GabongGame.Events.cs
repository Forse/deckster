using Deckster.Core.Games.Gabong;

namespace Deckster.Games.Gabong;

public partial class GabongGame
{
    public event NotifyAll<GameStartedNotification>? GameStarted;
    public event NotifyAll<PlayerPutCardNotification>? PlayerPutCard;
    public event NotifyAll<PlayerDrewCardNotification>? PlayerDrewCard;
    public event NotifyAll<PlayerDrewPenaltyCardNotification>? PlayerDrewPenaltyCard;
    public event NotifyAll<GameEndedNotification>? GameEnded;
    public event NotifyPlayer<RoundStartedNotification>? RoundStarted;
    public event NotifyAll<RoundEndedNotification>? RoundEnded;
    public event NotifyAll<PlayerLostTheirTurnNotification>? PlayerLostTheirTurn;
    public event RequestSelf<PenalizePlayerForTakingTooLongRequest>? PlayerTookTooLong;
    public event RequestSelf<PenalizePlayerForTooManyCardsRequest>? PlayerHasTooManyCards;
}