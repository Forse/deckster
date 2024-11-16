using Deckster.Core.Games.Common;

namespace Deckster.Games.Bullshit;

public class BullshitPlayer
{
    public Guid Id { get; init; }
    public string Name { get; init; } = "";
    public List<Card> Cards { get; init; } = [];
    public bool IsStillPlaying() => Cards.Any();
    
    public static readonly BullshitPlayer Null = new()
    {
        Id = Guid.Empty,
        Name = "Ing. Kognito"
    };

    public bool HasCard(Card card)
    {
        return Cards.Contains(card);
    }
}