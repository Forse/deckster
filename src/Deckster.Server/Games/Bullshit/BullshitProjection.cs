using Deckster.Core.Games.Bullshit;
using Deckster.Games;
using Deckster.Games.Bullshit;

namespace Deckster.Server.Games.Bullshit;

public class BullshitProjection : GameProjection<BullshitGame>
{
    public BullshitGame Create(BullshitCreatedEvent e)
    {
        var game = new BullshitGame
        {
            Id = e.Id,
            Name = e.Name,
            Seed = e.InitialSeed,
            StartedTime = e.StartedTime,
            Deck = e.Deck,
            Players = e.Players.Select(p => new BullshitPlayer
            {
                Id = p.Id,
                Name = p.Name
            }).ToList()
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
            InitialSeed = new Random().Next(0, int.MaxValue),
            Players = host.GetPlayers(),
            Deck = Decks.Standard().KnuthShuffle(new Random().Next(0, int.MaxValue))
        };
        var game = Create(e);
        return (game, e);
    }

    public Task Apply(PutCardRequest @event, BullshitGame game) => game.PutCard(@event);
    public Task Apply(DrawCardRequest @event, BullshitGame game) => game.DrawCard(@event);
    public Task Apply(PassRequest @event, BullshitGame game) => game.Pass(@event);
    public Task Apply(BullshitRequest @event, BullshitGame game) => game.CallBullshit(@event);
}