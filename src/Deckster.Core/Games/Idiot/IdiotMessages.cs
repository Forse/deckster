using Deckster.Core.Games.Common;
using Deckster.Core.Protocol;

namespace Deckster.Core.Games.Idiot;

public class PlayerViewOfGame
{
    public List<Card> CardsOnHand { get; init; } = [];
    public List<Card> CardsFacingUp { get; init; } = [];
    public int StockPileCount { get; init; }
    public List<OtherIdiotPlayer> OtherPlayers { get; init; } = [];
    public int CardsFacingDownCount { get; init; }
}

public class OtherIdiotPlayer
{
    public Guid Id { get; init; }
    public string Name { get; init; }
    public int CardsOnHandCount { get; init; }
    public List<Card> CardsFacingUp { get; init; } = [];
    public int CardsFacingDownCount { get; init; }
}

public class IamReadyRequest : DecksterRequest;

public class SwapCardsRequest : DecksterRequest
{
    public Card CardOnHand { get; init; }
    public Card CardFacingUp { get; init; }
}

public class PutCardsFromHandRequest : DecksterRequest
{
    public Card[] Cards { get; init; }
}

public class PutCardsFacingUpRequest : DecksterRequest
{
    public Card[] Cards { get; init; }
}

public class PutCardFacingDownRequest : DecksterRequest
{
    public int Index { get; init; }
}

public class DrawCardsRequest : DecksterRequest
{
    public int NumberOfCards { get; init; }
}

public class PullInDiscardPileRequest : DecksterRequest;

public class PutChanceCardRequest : DecksterRequest;

public class SwapCardsResponse : DecksterResponse
{
    public Card CardNowOnHand { get; init; }
    public Card CardNowFacingUp { get; init; }
}

public class PullInResponse : DecksterResponse
{
    public Card[] Cards { get; init; }
}

public class DrawCardsResponse : DecksterResponse
{
    public Card[] Cards { get; init; }
}

public class PutBlindCardResponse : DecksterResponse
{
    public Card AttemptedCard { get; init; }
    public Card[] PullInCards { get; init; }
}


public class PlayerSwappedCardsNotification : DecksterNotification
{
    public Guid PlayerId { get; init; }
    public Card CardNowOnHand { get; init; }
    public Card CardNowFacingUp { get; init; }
}

public enum PutCardFrom
{
    Hand,
    FacingUp,
    FacingDown
}

public class PlayerPutCardsNotification : DecksterNotification
{
    public Guid PlayerId { get; init; }
    public Card[] Cards { get; init; }
    public PutCardFrom From { get; init; }
}

public class PlayerIsReadyNotification : DecksterNotification
{
    public Guid PlayerId { get; init; }
}

public class PlayerIsDoneNotification : DecksterNotification
{
    public Guid PlayerId { get; init; }
}

public class DiscardPileFlushedNotification : DecksterNotification
{
    public Guid PlayerId { get; init; }
}

public class ItsYourTurnNotification : DecksterNotification;

public class PlayerDrewCardsNotification : DecksterNotification
{
    public Guid PlayerId { get; init; }
    public int NumberOfCards { get; init; }
}

public class PlayerAttemptedPuttingCardNotification : DecksterNotification
{
    public Guid PlayerId { get; init; }
    public Card Card { get; init; }
}

public class PlayerPulledInDiscardPileNotification : DecksterNotification
{
    public Guid PlayerId { get; init; }
}

public class GameStartedNotification : DecksterNotification;

public class GameEndedNotification : DecksterNotification
{
    public Guid LoserId { get; set; }
}

public class ItsTimeToSwapCardsNotification : DecksterNotification
{
    public PlayerViewOfGame PlayerViewOfGame { get; init; } 
}
