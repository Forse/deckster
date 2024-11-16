using Deckster.Core.Games.Common;

namespace Deckster.Games.Bullshit;

public class BullshitCreatedEvent : GameCreatedEvent
{
    public List<PlayerData> Players { get; init; } = [];
    public List<Card> Deck { get; init; } = [];
}