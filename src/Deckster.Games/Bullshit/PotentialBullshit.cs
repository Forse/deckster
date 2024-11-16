using Deckster.Core.Games.Common;

namespace Deckster.Games.Bullshit;

public class PotentialBullshit
{
    public Guid PlayerId { get; init; }
    public Card ClaimedToBeCard { get; init; }
}