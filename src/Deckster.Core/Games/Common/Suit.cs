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
            Suit.Clubs => "C",
            Suit.Diamonds => "D",
            Suit.Hearts => "H",
            Suit.Spades => "S",
            _ => throw new ArgumentOutOfRangeException(nameof(suit), suit, null)
        };
    }
}