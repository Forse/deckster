using Deckster.Core.Games.Common;
using Deckster.Core.Protocol;

namespace Deckster.Core.Games.Bullshit;

public class OtherBullshitPlayer
{
    public Guid PlayerId { get; init; }
    public string Name { get; init; } = "";
    public int NumberOfCards { get; init; }
}

public class PlayerViewOfGame
{
    public List<Card> Cards { get; init; }
    public Card? ClaimedTopOfPile { get; init; }
    public int StockPileCount { get; init; }
    public int DiscardPileCount { get; init; }
    public List<OtherBullshitPlayer> OtherPlayers { get; init; }
    
    public PlayerViewOfGame()
    {
        
    }
}

public class PutCardRequest : DecksterRequest
{
    public Card ActualCard { get; init; }
    public Card ClaimedToBeCard { get; init; }
}

public class PlayerPutCardNotification : DecksterNotification
{
    public Guid PlayerId { get; set; }
    public Card ClaimedToBeCard { get; set; }
}

public class BullshitRequest : DecksterRequest
{
    
}

public class BullshitResponse : DecksterResponse
{
    public Card[] PunishmentCards { get; init; } = [];
}

public class DrawCardRequest : DecksterRequest;

public class PassRequest : DecksterRequest;

public class CardResponse : DecksterResponse
{
    public Card Card { get; init; }
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
    
}

public class FalseBullshitCallNotification : DecksterNotification
{
    public Guid PlayerId { get; init; }
    public int PunishmentCardCount { get; init; }
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


public class BullshitBroadcastNotification : DecksterNotification
{
    public Guid PlayerId { get; init; }
    public Card ClaimedToBeCard { get; init; }
    public Card ActualCard { get; init; }
}

public class BullshitPlayerNotification : DecksterNotification
{
    public Guid CalledByPlayerId { get; init; }
    public Card Card { get; init; }
    public Card[] PunishmentCards { get; init; }
}