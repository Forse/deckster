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
    public enum DisplayFormat
    {
        Saklig,
        Windows
    }
    
    public static string Display(this Suit suit)
    {
        var format = OperatingSystem.IsWindows() ? DisplayFormat.Windows : DisplayFormat.Saklig;
        
        return format switch
        {
            DisplayFormat.Saklig => suit switch
            {
                Suit.Clubs => "♣",
                Suit.Hearts => "♥",
                Suit.Spades => "♠",
                Suit.Diamonds => "♦",
                _ => throw new ArgumentOutOfRangeException(nameof(suit), suit, null)
            },
            _ => suit switch
            {
                Suit.Clubs => "c",
                Suit.Hearts => "h",
                Suit.Spades => "s",
                Suit.Diamonds => "d",
                _ => throw new ArgumentOutOfRangeException(nameof(suit), suit, null)
            }
        };
    }
}