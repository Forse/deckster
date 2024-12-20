using System.Net.WebSockets;
using Deckster.Core;
using Deckster.Games;
using Deckster.Games.CodeGeneration.Meta;
using Deckster.Server.Authentication;
using Deckster.Server.ContentNegotiation.Html;
using Deckster.Server.Data;
using Deckster.Server.Games;
using Deckster.Server.Games.Common.Fakes;
using Deckster.Server.Middleware;
using Microsoft.AspNetCore.Mvc;

namespace Deckster.Server.Controllers;

// Marker interface for discoverability
public interface IGameController;

public abstract class GameController<TGameHost, TGame> : Controller, IGameController
    where TGameHost : IGameHost
    where TGame : GameObject
{
    protected readonly GameHostRegistry HostRegistry;
    protected readonly IRepo Repo;

    protected static readonly string GameType = typeof(TGame).Name.Replace("Game", "");

    protected GameController(GameHostRegistry hostRegistry, IRepo repo)
    {
        HostRegistry = hostRegistry;
        Repo = repo;
    }

    [HttpGet("description")]
    public ViewResult GameDescription()
    {
        HttpContext.SetTitle(GameType);
        return View();
    }

    [HttpGet("metadata")]
    public GameMeta Meta()
    {
        var meta = GameMeta.TryGetFor(typeof(TGame), out var m) ? m : null;
        return meta;
    }
    
    [HttpGet("")]
    public GameOverviewVm Overview()
    {
        HttpContext.SetTitle(GameType);
        var games = HostRegistry.GetHosts<TGameHost>().Select(h => new GameVm
        {
            GameType = h.GameType,
            Name = h.Name,
            Players = h.GetPlayers()
        });
        return new GameOverviewVm
        {
            GameType = GameType,
            Games = games.ToList()
        };
    }
    
    [HttpGet("games")]
    public IEnumerable<GameVm> Games()
    {
        HttpContext.SetTitle($"{GameType} games");
        var games = HostRegistry.GetHosts<TGameHost>().Select(h => new GameVm
        {
            Name = h.Name,
            Players = h.GetPlayers()
        });
        return games;
    }
    
    [HttpGet("games/{name}")]
    [ProducesResponseType<GameVm>(200)]
    [ProducesResponseType<ResponseMessage>(404)]
    public object GameState(string name)
    {
        HttpContext.SetTitle($"{GameType} state");
        if (!HostRegistry.TryGet<TGameHost>(name, out var host))
        {
            return StatusCode(404, new ResponseMessage($"Game not found: '{name}'"));
        }
        
        var vm = new GameVm
        {
            Name = host.Name,
            GameType = host.GameType,
            State = host.State,
            Players = host.GetPlayers()
        };

        return vm;
    }
    
    [HttpDelete("games/{name}")]
    [ProducesResponseType<GameVm>(200)]
    [ProducesResponseType<ResponseMessage>(404)]
    public async Task<object> CancelGame(string name)
    {
        if (!HostRegistry.TryGet<TGameHost>(name, out var host))
        {
            return StatusCode(404, new ResponseMessage($"Game not found: '{name}'"));
        }

        await host.EndAsync();
        
        var vm = new GameVm
        {
            Name = host.Name,
            Players = host.GetPlayers()
        };

        return Request.AcceptsJson() ? vm : View(vm);
    }

    [HttpPost("games/{name}/bot")]
    public ResponseMessage AddBot(string name)
    {
        if (!HostRegistry.TryGet<TGameHost>(name, out var host))
        {
            Response.StatusCode = 404;
            return new ResponseMessage($"Game not found: '{name}'");
        }

        if (!host.TryAddBot(out var error))
        {
            Response.StatusCode = 400;
            return new ResponseMessage(error);
        }

        return new ResponseMessage("ok");
    }

    [HttpGet("previousgames")]
    public async Task<IEnumerable<TGame>> PreviousGames()
    {
        HttpContext.SetTitle(GameType);
        var games = await Repo.Query<TGame>().ToListAsync();

        return games;
    }
    
    [HttpGet("previousgames/{id:guid}")]
    public async Task<Historic<TGame>?> PreviousGame(Guid id)
    {
        var game = await Repo.GetGameAsync<TGame>(id);
        if (game == null)
        {
            Response.StatusCode = 404;
            return null;
        }
        

        return game;
    }
    
    [HttpGet("previousgames/{id:guid}/{version:long}")]
    public async Task<Historic<TGame>?> PreviousGames(Guid id, long version)
    {
        var game = await Repo.GetGameAsync<TGame>(id, version);
        if (game == null)
        {
            Response.StatusCode = 404;
            return null;
        }
        return game;
    }
    
    [HttpPost("create/{name}")]
    [RequireUser]
    [ProducesResponseType<GameInfo>(200)]
    [ProducesResponseType<ResponseMessage>(400)]
    public object Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return StatusCode(400, new ResponseMessage("Name is required"));
        }
        
        var host = HttpContext.RequestServices.GetRequiredService<TGameHost>();
        host.Name = name;
        HostRegistry.Add(host);
        return new GameInfo
        {
            Id = host.Name
        };
    }

    [HttpPost("create")]
    [RequireUser]
    [ProducesResponseType<GameInfo>(200)]
    [ProducesResponseType<ResponseMessage>(400)]
    public object Create() => Create(GameNames.Random());
    
    [HttpPost("games/{name}/start")]
    [RequireUser]
    [ProducesResponseType<GameInfo>(200)]
    [ProducesResponseType<ResponseMessage>(404)]
    public async Task<object> Start(string name)
    {
        if (!HostRegistry.TryGet<TGameHost>(name, out var host))
        {
            return StatusCode(404, new ResponseMessage($"Game not found: '{name}'"));
        }
        
        await host.StartAsync();
        return StatusCode(200, new ResponseMessage($"Game '{name}' started"));
    }
    
    [HttpGet("join/{gameName}")]
    [RequireUser]
    public async Task Join(string gameName)
    {
        //HttpContext.Response.Headers.Connection = "close";
        if (!HttpContext.WebSockets.IsWebSocketRequest)
        {
            HttpContext.Response.StatusCode = 400;
            await HttpContext.Response.WriteAsJsonAsync(new ResponseMessage("Not WS request"));
            return;
        }

        if (!HttpContext.TryGetUser(out var decksterUser))
        {
            HttpContext.Response.StatusCode = 401;
            await HttpContext.Response.WriteAsJsonAsync(new ResponseMessage("Unauthorized"));
            return;
        }
        using var actionSocket = await HttpContext.WebSockets.AcceptWebSocketAsync(WebSocketDefaults.AcceptContext);
        
        if (!await HostRegistry.StartJoinAsync<TGameHost>(decksterUser, actionSocket, gameName))
        {
            HttpContext.Response.StatusCode = 400;
            await HttpContext.Response.WriteAsJsonAsync(new ResponseMessage("Could not connect"));
            await actionSocket.CloseOutputAsync(WebSocketCloseStatus.ProtocolError, "Could not connect", default);
        }
    }

    [HttpGet("join/{connectionId:guid}/finish")]
    [RequireUser]
    public Task FinishJoin(Guid connectionId)
    {
        return FinishHandshake(connectionId, true);
    }
    
    private async Task FinishHandshake(Guid connectionId, bool joinAsPlayer)
    {
        //HttpContext.Response.Headers.Connection = "close";
        if (!HttpContext.WebSockets.IsWebSocketRequest)
        {
            HttpContext.Response.StatusCode = 400;
            await HttpContext.Response.WriteAsJsonAsync(new ResponseMessage("Not WS request"));
            return;
        }
        
        using var eventSocket = await HttpContext.WebSockets.AcceptWebSocketAsync(WebSocketDefaults.AcceptContext);

        if (!await HostRegistry.FinishJoinAsync(connectionId, eventSocket, joinAsPlayer))
        {
            if (!HttpContext.Response.HasStarted)
            {
                HttpContext.Response.StatusCode = 400;
                await HttpContext.Response.WriteAsJsonAsync(new ResponseMessage("Could not connect"));    
            }
        }
    }
    
    [HttpGet("spectate/{gameName}")]
    [RequireUser]
    public Task Spectate(string gameName)
    {
        return Join(gameName);//same start of handshake
    }
    
    [HttpGet("spectate/{connectionId:guid}/finish")]
    [RequireUser]
    public Task FinishJoinAsSpectator(Guid connectionId)
    {
        return FinishHandshake(connectionId, false);
    }
}

public static class WebSocketDefaults
{
    public static readonly WebSocketAcceptContext AcceptContext = new()
    {
        KeepAliveInterval = TimeSpan.FromSeconds(5)
    };
}