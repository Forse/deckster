using Deckster.Core.Games.Common;
using Deckster.Core.Protocol;

namespace Deckster.Core.Games.CrazyEights;

public class PlayerPutCardNotification : DecksterNotification
{
    public Guid PlayerId { get; set; }
    public Card Card { get; set; }
}

public class PlayerPutEightNotification : DecksterNotification
{
    public Guid PlayerId { get; set; }
    public Card Card { get; set; }
    public Suit NewSuit { get; set; }
}

public class PlayerDrewCardNotification : DecksterNotification
{
    public Guid PlayerId { get; set; }
}

public class PlayerPassedNotification : DecksterNotification
{
    public Guid PlayerId { get; set; }
}

public class ItsYourTurnNotification : DecksterNotification
{
    public PlayerViewOfGame PlayerViewOfGame { get; init; } = new();
}

public class ItsPlayersTurnNotification : DecksterNotification
{
    public Guid PlayerId { get; init; }
}

public class GameStartedNotification : DecksterNotification
{
    public Guid GameId { get; init; }
    public PlayerViewOfGame PlayerViewOfGame { get; init; } = new();
}

public class GameEndedNotification : DecksterNotification
{
    public List<PlayerData> Players { get; init; }
    public Guid LoserId { get; set; }
    public string LoserName { get; set; }
}

public class PlayerIsDoneNotification : DecksterNotification
{
    public Guid PlayerId { get; init; }
}

public class DiscardPileShuffledNotification : DecksterNotification;