namespace Deckster.Core.Games.TexasHoldEm;

public class OtherPokerPlayers
{
    public Guid PlayerId { get; init; }
    public string Name { get; init; }
    public int StackSize { get; init; }
    public int CurrentBet { get; init; }
    public bool HasChecked { get; init; }
    public bool IsAllIn { get; init; }
    public bool HasFolded { get; init; }
}