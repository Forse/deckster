using Deckster.Games.Bullshit;
using Deckster.Server.Data;
using Deckster.Server.Games;
using Deckster.Server.Games.Bullshit;
using Microsoft.AspNetCore.Mvc;

namespace Deckster.Server.Controllers;

[Route("bullshit")]
public class BullshitController(GameHostRegistry hostRegistry, IRepo repo)
    : GameController<BullshitGameHost, BullshitGame>(hostRegistry, repo);