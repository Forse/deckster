using Deckster.Core.Games.Common;

namespace Deckster.Games.TexasHoldEm;

public class TexasHoldEmHandRating
{
    private readonly List<List<int>> _possibleStraights =
    [
        [1, 2, 3, 4, 5],
        [2, 3, 4, 5, 6],
        [3, 4, 5, 6, 7],
        [4, 5, 6, 7, 8],
        [5, 6, 7, 8, 9],
        [6, 7, 8, 9, 10],
        [7, 8, 9, 10, 11],
        [8, 9, 10, 11, 12],
        [9, 10, 11, 12, 13],
        [10, 11, 12, 13, 14]
    ];

    public int HandRating(List<Card> cards)
    {
        if (HasStraightFlush(cards))
        {
            return 15;
        }

        if (HasFourEquals(cards))
        {
            return 14;
        }

        if (HasFullHouse(cards))
        {
            return 13;
        }

        if (HasFlush(cards))
        {
            return 12;
        }

        if (HasStraight(cards))
        {
            return 11;
        }

        if (HasThreeEquals(cards))
        {
            return 10;
        }

        if (HasTwoPairs(cards))
        {
            return 9;
        }

        if (HasSinglePair(cards))
        {
            return 8;
        }

        return 1;
    }

    public bool HasStraightFlush(List<Card> cards)
    {
        return HasStraight(cards) && HasFlush(cards);
    }

    public bool HasFullHouse(List<Card> cards)
    {
        return (HasSinglePair(cards) || HasTwoPairs(cards)) && HasThreeEquals(cards);
    }

    public bool HasStraight(List<Card> cards)
    {
        var orderedCards = cards.Select(i => i.Rank).Distinct().Order().ToList();
        for (int i = 0; i + 4 < orderedCards.Count; i++)
        {
            var sequence = orderedCards.Skip(i).Take(5).ToList();
            foreach (var possibleStraight in _possibleStraights)
            {
                if (possibleStraight.SequenceEqual(sequence))
                {
                    return true;
                }
            }
        }
        return false;
    }

    public bool HasFlush(List<Card> cards)
    {
        var suits = cards.GroupBy(i => i.Suit).ToList();
        return suits.Any(i => i.Count() >= 5);
    }

    public bool HasFourEquals(List<Card> cards)
    {
        var groups = cards.GroupBy(i => i.Rank).ToList();
        var pairs = groups.Where(g => g.Count() == 4);
        return pairs.Count() == 1;
    }

    public bool HasThreeEquals(List<Card> cards)
    {
        var groups = cards.GroupBy(i => i.Rank).ToList();
        var threeEquals = groups.Where(g => g.Count() == 3);
        return threeEquals.Any();
    }

    public bool HasTwoPairs(List<Card> cards)
    {
        var groups = cards.GroupBy(i => i.Rank).ToList();
        var pairs = groups.Where(g => g.Count() == 2);
        return pairs.Count() >= 2;
    }

    public bool HasSinglePair(List<Card> cards)
    {
        var groups = cards.GroupBy(i => i.Rank).ToList();
        var pairs = groups.Where(g => g.Count() == 2);
        return pairs.Count() == 1;
    }

    public Card HighCard(List<Card> cards)
    {
        return cards.MaxBy(i => i.Rank);
    }
}