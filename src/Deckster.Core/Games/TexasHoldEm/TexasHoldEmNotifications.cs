using Deckster.Core.Games.Common;
using Deckster.Core.Protocol;

namespace Deckster.Core.Games.TexasHoldEm;

public class PlayerBettedNotification : DecksterNotification
{
    public Guid PlayerId { get; set; }
    public int BetSize { get; set; }
}

public class PlayerAllInNotification : DecksterNotification
{
    public Guid PlayerId { get; set; }
}

public class PlayerCheckedNotification : DecksterNotification
{
    public Guid PlayerId { get; set; }
}

public class PlayerFoldedNotification : DecksterNotification
{
    public Guid PlayerId { get; set; }
}

public class PlayerCalledNotification : DecksterNotification
{
    public Guid PlayerId { get; set; }
}

public class PlayerIsOutOfGameNotification : DecksterNotification
{
    public Guid PlayerId { get; set; }
}

public class ItsYourTurnNotification : DecksterNotification
{
    public PlayerViewOfGame PlayerViewOfGame { get; init; } = new();
}

public class NewRoundStartedNotification : DecksterNotification
{
    public Guid GameId { get; init; }
    public PlayerViewOfGame PlayerViewOfGame { get; init; } = new();
}

public class NewCardsRevealed : DecksterNotification
{
    public List<Card> Cards { get; init; }
}

public class RoundEndedNotification : DecksterNotification
{
    public Guid WinnerId { get; init; }
    public int Winnings { get; init; }
};

public class GameStartedNotification : DecksterNotification
{
    public Guid GameId { get; init; }
    public PlayerViewOfGame PlayerViewOfGame { get; init; } = new();
}


public class GameEndedNotification : DecksterNotification
{
    public List<PlayerData> Players { get; init; } = [];
}