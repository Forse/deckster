using Deckster.Core.Games.Common;
using Deckster.Core.Protocol;

namespace Deckster.Core.Games.TexasHoldEm;

public class PlayerViewOfGame : DecksterResponse
{
    public List<Card> Cards { get; init; }
    public List<Card> CardsOnTable { get; init; }
    public int StackSize { get; init; }
    public int CurrentBet { get; init; }
    public int PotSize { get; init; }
    public int BigBlind { get; init; }
    public int NumberOfRoundsUntilBigBlindIncreases { get; init; }
    public List<OtherPokerPlayers> OtherPlayers { get; init; }
    public Guid NextRoundStartingPlayerId { get; init; }

    public PlayerViewOfGame()
    {
    }

    public PlayerViewOfGame(string error)
    {
        Error = error;
    }
}