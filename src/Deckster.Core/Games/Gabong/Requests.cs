using Deckster.Core.Games.Common;
using Deckster.Core.Protocol;

namespace Deckster.Core.Games.Gabong;

public abstract class GabongRequest : DecksterRequest;
public class PutCardRequest : GabongRequest
{
    public Card Card { get; set; }
    public Suit? NewSuit { get; set; }
}

public class DrawCardRequest : GabongRequest;
public class PassRequest : GabongRequest;

public class PlayGabongRequest : GabongRequest;

public class PlayBongaRequest : GabongRequest;