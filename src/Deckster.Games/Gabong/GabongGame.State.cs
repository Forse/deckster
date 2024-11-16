using Deckster.Core.Collections;
using Deckster.Core.Games.Common;
using Deckster.Core.Games.Gabong;

namespace Deckster.Games.Gabong;

public partial class GabongGame
{
    public int CardsToDraw { get; set; }
    public int CardsDrawn { get; set; }
    public int GameDirection { get; set; } = 1;
    
    public Guid GabongMasterId { get; set; } = Guid.Empty;

    public bool IsBetweenRounds { get; set; } = true;

    /// <summary>
    /// All the (shuffled) cards in the game
    /// </summary>
    public List<Card> Deck { get; init; } = [];

    /// <summary>
    /// Where players draw cards from
    /// </summary>
    public List<Card> StockPile { get; set; } = new();

    /// <summary>
    /// Where players put cards
    /// </summary>
    public List<Card> DiscardPile { get; set; } = new();

    /// <summary>
    /// All the players
    /// </summary>
    public List<GabongPlayer> Players { get; init; } = [];

    public Suit? NewSuit { get; set; }
    public Card TopOfPile => DiscardPile.PeekOrDefault();
    private GabongPlay LastPlay { get; set; } = GabongPlay.RoundStarted;
    public Suit CurrentSuit => NewSuit ?? TopOfPile.Suit;

    public GabongPlayer CurrentPlayer => CalculateCurrentPlayer();

    private GabongPlayer PlayerIndexAdjustedBy(int delta)
    {
        return Players[(Players.Count + LastPlayMadeByPlayerIndex + (delta * GameDirection)) % Players.Count];
    }
    private int GetPlayerIndex(Guid lastGabongMadeBy)
    {
        return Players.FindIndex(p => p.Id == lastGabongMadeBy);
    }
  
    
    
    private void ShufflePileIfNecessary()
    {
        if (StockPile.Count > 3)
        {
            return;
        }

        if (DiscardPile.Count < 2)
        {
            return;
        }

        ShufflePile(14);
    }

    private void ShufflePile(int saveTopCards)
    {
        saveTopCards = Math.Min(saveTopCards, DiscardPile.Count);
        var saved = new List<Card>();
        for (int i = 0; i < saveTopCards; i++) //save the top 14 cards
        {
            saved.Push(DiscardPile.Pop());
        }

        var reshuffledCards = DiscardPile.KnuthShuffle(Seed);
        DiscardPile.Clear();
        for (int i = 0; i < saveTopCards; i++) //save the top 14 cards
        {
            DiscardPile.Push(saved.Pop());
        }

        StockPile.PushRange(reshuffledCards);
    }
}