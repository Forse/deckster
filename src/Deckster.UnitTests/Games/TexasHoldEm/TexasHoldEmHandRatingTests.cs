using Deckster.Core.Games.Common;
using Deckster.Games.TexasHoldEm;
using NUnit.Framework;

namespace Deckster.UnitTests.Games.TexasHoldEm;

public class TexasHoldEmHandRatingTests
{
    [Test]
    public void HasSinglePair_WithSinglePair_ShouldReturnTrue()
    {
        // arrange
        var cards = new List<Card>
        {
            new Card(1, Suit.Clubs),
            new Card(1, Suit.Diamonds),
            new Card(2, Suit.Diamonds),
            new Card(3, Suit.Diamonds),
            new Card(4, Suit.Diamonds),
        };

        // act
        var result = new TexasHoldEmHandRating().HasSinglePair(cards);

        // assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void HasSinglePair_WithTwoPairs_ShouldReturnFalse()
    {
        // arrange
        var cards = new List<Card>
        {
            new Card(1, Suit.Clubs),
            new Card(1, Suit.Diamonds),
            new Card(2, Suit.Diamonds),
            new Card(2, Suit.Clubs),
            new Card(4, Suit.Diamonds),
        };

        // act
        var result = new TexasHoldEmHandRating().HasSinglePair(cards);

        // assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void HasSinglePair_WithoutPair_ShouldReturnFalse()
    {
        // arrange
        var cards = new List<Card>
        {
            new Card(1, Suit.Diamonds),
            new Card(2, Suit.Diamonds),
            new Card(3, Suit.Diamonds),
            new Card(4, Suit.Diamonds),
            new Card(5, Suit.Diamonds),
        };

        // act
        var result = new TexasHoldEmHandRating().HasSinglePair(cards);

        // assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void HasTwoPairs_WithTwoPair_ShouldReturnTrue()
    {
        // arrange
        var cards = new List<Card>
        {
            new Card(1, Suit.Clubs),
            new Card(1, Suit.Diamonds),
            new Card(2, Suit.Diamonds),
            new Card(2, Suit.Clubs),
            new Card(4, Suit.Diamonds),
        };

        // act
        var result = new TexasHoldEmHandRating().HasTwoPairs(cards);

        // assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void HasTwoPairs_WithSinglePair_ShouldReturnFalse()
    {
        // arrange
        var cards = new List<Card>
        {
            new Card(1, Suit.Diamonds),
            new Card(1, Suit.Clubs),
            new Card(3, Suit.Diamonds),
            new Card(4, Suit.Diamonds),
            new Card(5, Suit.Diamonds),
        };

        // act
        var result = new TexasHoldEmHandRating().HasTwoPairs(cards);

        // assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void HasFlush_WithAllOneSuite_ShouldReturnTrue()
    {
        // arrange
        var cards = new List<Card>
        {
            new Card(1, Suit.Diamonds),
            new Card(2, Suit.Diamonds),
            new Card(3, Suit.Diamonds),
            new Card(4, Suit.Diamonds),
            new Card(5, Suit.Diamonds),
        };

        // act
        var result = new TexasHoldEmHandRating().HasFlush(cards);

        // assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void HasFlush_WithOneOffSuite_ShouldReturnFalse()
    {
        // arrange
        var cards = new List<Card>
        {
            new Card(1, Suit.Diamonds),
            new Card(1, Suit.Clubs),
            new Card(3, Suit.Diamonds),
            new Card(4, Suit.Diamonds),
            new Card(5, Suit.Diamonds),
        };

        // act
        var result = new TexasHoldEmHandRating().HasFlush(cards);

        // assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void HasStraight_WithAllInStraight_ShouldReturnTrue()
    {
        // arrange
        var cards = new List<Card>
        {
            new Card(1, Suit.Diamonds),
            new Card(2, Suit.Clubs),
            new Card(3, Suit.Diamonds),
            new Card(4, Suit.Diamonds),
            new Card(5, Suit.Diamonds),
        };

        // act
        var result = new TexasHoldEmHandRating().HasStraight(cards);

        // assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void HasStraight_WithOneOutOfOrder_ShouldReturnFalse()
    {
        // arrange
        var cards = new List<Card>
        {
            new Card(1, Suit.Diamonds),
            new Card(2, Suit.Clubs),
            new Card(3, Suit.Diamonds),
            new Card(4, Suit.Diamonds),
            new Card(6, Suit.Diamonds),
        };

        // act
        var result = new TexasHoldEmHandRating().HasStraight(cards);

        // assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void HasStraightFlush_WithStraightFlush_ShouldReturnTrue()
    {
        // arrange
        var cards = new List<Card>
        {
            new Card(1, Suit.Diamonds),
            new Card(2, Suit.Diamonds),
            new Card(3, Suit.Diamonds),
            new Card(4, Suit.Diamonds),
            new Card(5, Suit.Diamonds),
        };

        // act
        var result = new TexasHoldEmHandRating().HasStraightFlush(cards);

        // assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void HasStraightFlush_WithStraightNotFlush_ShouldReturnFalse()
    {
        // arrange
        var cards = new List<Card>
        {
            new Card(1, Suit.Diamonds),
            new Card(2, Suit.Clubs),
            new Card(3, Suit.Diamonds),
            new Card(4, Suit.Diamonds),
            new Card(5, Suit.Diamonds),
        };

        // act
        var result = new TexasHoldEmHandRating().HasStraightFlush(cards);

        // assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void HasStraightFlush_WithFlushNotStraight_ShouldReturnFalse()
    {
        // arrange
        var cards = new List<Card>
        {
            new Card(1, Suit.Diamonds),
            new Card(2, Suit.Diamonds),
            new Card(3, Suit.Diamonds),
            new Card(4, Suit.Diamonds),
            new Card(6, Suit.Diamonds),
        };

        // act
        var result = new TexasHoldEmHandRating().HasStraightFlush(cards);

        // assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void HasFullHouse_WithOnePairAndThreeOfAKind_ShouldReturnTrue()
    {
        // arrange
        var cards = new List<Card>
        {
            new Card(1, Suit.Diamonds),
            new Card(1, Suit.Spades),
            new Card(2, Suit.Diamonds),
            new Card(2, Suit.Spades),
            new Card(2, Suit.Clubs),
            new Card(3, Suit.Clubs),
            new Card(4, Suit.Clubs),
        };

        // act
        var result = new TexasHoldEmHandRating().HasFullHouse(cards);

        // assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void HasFullHouse_WithTwoPairsAndThreeOfAKind_ShouldReturnTrue()
    {
        // arrange
        var cards = new List<Card>
        {
            new Card(1, Suit.Diamonds),
            new Card(1, Suit.Spades),
            new Card(2, Suit.Diamonds),
            new Card(2, Suit.Spades),
            new Card(2, Suit.Clubs),
            new Card(3, Suit.Clubs),
            new Card(3, Suit.Diamonds),
        };

        // act
        var result = new TexasHoldEmHandRating().HasFullHouse(cards);

        // assert
        Assert.That(result, Is.True);
    }
}