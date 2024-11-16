using Deckster.Core.Games.Common;

namespace Deckster.Bullshit.SampleClient;

public class OtherPlayer
{
    public Guid Id { get; init; }
    public string Name { get; init; }
    public List<Card> KnownCards { get; init; } = [];
    public int CardCount { get; set; }

    public override string ToString() => $"{Name} ({Id})";
}