using Deckster.Core.Games.Common;

namespace Deckster.Bullshit.SampleClient;

public class BullshitState
{
    public List<Card> Cards { get; set; }
    public Card? ClaimedToBeTopOfPile => DiscardPile.Any() ? DiscardPile.Last() : null;
    
    public Dictionary<Guid, OtherPlayer> OtherPlayers { get; set; }
    public int StockPileCount { get; set; }
    public List<Card> DiscardPile { get; set; } = [];

    public bool TryGetCard(out Card card)
    {
        foreach (var c in Cards)
        {
            if (CanPut(c))
            {
                card = c;
                return true;
            }
        }

        card = default;
        return false;
    }

    private bool CanPut(Card card)
    {
        if (!ClaimedToBeTopOfPile.HasValue)
        {
            return true;
        }

        var top = ClaimedToBeTopOfPile.Value;
        return top.Suit == card.Suit || top.Rank == card.Rank;
    }
}