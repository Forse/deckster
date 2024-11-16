using Deckster.Core.Protocol;

namespace Deckster.Core.Games.TexasHoldEm;

public class BetRequest : DecksterRequest
{
    public int Bet { get; set; }
}

public class FoldRequest : DecksterRequest;

public class CheckRequest : DecksterRequest;

public class CallRequest : DecksterRequest;