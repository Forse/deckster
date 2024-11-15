namespace Deckster.Core.Games.Common;

public enum Suit
{
    Clubs,
    Diamonds,
    Hearts,
    Spades
}

public static class SuitExtensions
{
    public static string Display(this Suit suit)
    {
        return suit switch
        {
            Suit.Clubs => "c",
            Suit.Hearts => "h",
            Suit.Spades => "s",
            Suit.Diamonds => "d",
            _ => throw new ArgumentOutOfRangeException(nameof(suit), suit, null)
        };
    }
}