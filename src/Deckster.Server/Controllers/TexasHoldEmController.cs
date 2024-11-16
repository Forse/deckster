using Deckster.Games.TexasHoldEm;
using Deckster.Server.Data;
using Deckster.Server.Games;
using Deckster.Server.Games.TexasHoldEm;
using Microsoft.AspNetCore.Mvc;

namespace Deckster.Server.Controllers;

[Route("texasholdem")]
public class TexasHoldEmController(GameHostRegistry hostRegistry, IRepo repo)
    : GameController<TexasHoldEmGameHost, TexasHoldEmGame>(hostRegistry, repo);