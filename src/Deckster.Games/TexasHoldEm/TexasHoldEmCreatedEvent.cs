using Deckster.Core.Games.Common;

namespace Deckster.Games.TexasHoldEm;

public class TexasHoldEmCreatedEvent : GameCreatedEvent
{
    public List<PlayerData> Players { get; init; } = [];
    public List<Card> Deck { get; init; } = [];
    public int StackSize { get; init; } = 100;
    public int NumberOfRounds { get; init; } = 100;
    public int BigBlindSize { get; init; } = 2;
}
