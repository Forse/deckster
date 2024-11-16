using System.Diagnostics.CodeAnalysis;
using Deckster.Core.Games.Bullshit;
using Deckster.Games.Bullshit;
using Deckster.Server.Controllers;
using Deckster.Server.Data;

namespace Deckster.Server.Games.Bullshit;

public class BullshitGameHost : StandardGameHost<BullshitGame>
{
    public override string GameType => "Bullshit";
    
    public BullshitGameHost(IRepo repo, ILoggerFactory loggerFactory) : base(repo, loggerFactory, new BullshitProjection(), 6)
    {
    }
    
    public override bool TryAddBot([MaybeNullWhen(true)] out string error)
    {
        error = "Bots not supported";
        return false;
    }
}

public class BullshitProjection : GameProjection<BullshitGame>
{
    public BullshitGame Create(BullshitCreatedEvent e)
    {
        var game = new BullshitGame
        {
            Id = e.Id,
            Name = e.Name,
            Seed = e.InitialSeed,
            StartedTime = e.StartedTime
        };
        game.Deal();
        
        return game;
    }
    
    public override (BullshitGame game, object startEvent) Create(IGameHost host)
    {
        var e = new BullshitCreatedEvent
        {
            Id = Guid.NewGuid(),
            Name = host.Name,
            StartedTime = DateTimeOffset.UtcNow,
            InitialSeed = new Random().Next(0, int.MaxValue)
        };
        var game = Create(e);
        return (game, e);
    }

    Task Apply(PutCardRequest @event, BullshitGame game) => game.PutCard(@event);
    Task Apply(DrawCardRequest @event, BullshitGame game) => game.DrawCard(@event);
    Task Pass(PassRequest @event, BullshitGame game) => game.Pass(@event);
    Task CallBullshit(BullshitRequest @event, BullshitGame game) => game.CallBullshit(@event);
}