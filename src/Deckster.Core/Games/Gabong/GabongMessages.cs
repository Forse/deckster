using Deckster.Core.Games.Common;
using Deckster.Core.Protocol;

namespace Deckster.Core.Games.Gabong;

public abstract class GabongGameNotification: DecksterNotification;

public abstract class GabongPlayerNotification : GabongGameNotification
{
    public Guid PlayerId { get; init; }
    public bool IsFor(Guid playerId) => PlayerId == playerId;
}

public class PlayerPutCardNotification : GabongPlayerNotification
{
    public Card Card { get; init; }
    
    public Suit? NewSuit { get; init; }
}

public class PlayerDrewCardNotification : GabongPlayerNotification
{
}
public class PlayerDrewPenaltyCardNotification : GabongPlayerNotification
{
    public PenaltyReason PenaltyReason { get; init; }
    
}

public class GameStartedNotification : GabongGameNotification
{
    public Guid GameId { get; init; }
}

public class GameEndedNotification : GabongGameNotification
{
    public List<PlayerData> Players { get; init; }
}

public class RoundStartedNotification : GabongGameNotification
{
    public PlayerViewOfGame PlayerViewOfGame { get; init; }
    public Guid StartingPlayerId { get; set; }
}

public class RoundEndedNotification : GabongGameNotification
{
    public List<PlayerData> Players { get; init; }
}
public class PlayerLostTheirTurnNotification : GabongPlayerNotification
{
    public PlayerLostTurnReason LostTurnReason { get; set; }
}

public class PenalizePlayerForTakingTooLongRequest : DecksterRequest;

public class PenalizePlayerForTooManyCardsRequest : DecksterRequest;

public enum PlayerLostTurnReason
{
    Passed,
    WrongPlay,
    TookTooLong,
    FinishedDrawingCardDebt
}

public enum PenaltyReason
{
    PlayOutOfTurn,
    TookTooLong,
    WrongPlay,
    WrongGabong,
    WrongBonga,
    UnpaidDebt,
    PassWithoutDrawing
}

public enum GabongPlay
{
    CardPlayed,
    TurnLost,
    RoundStarted
}
