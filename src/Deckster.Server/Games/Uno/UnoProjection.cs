using Deckster.Core.Games.Uno;
using Deckster.Games;
using Deckster.Games.Uno;

namespace Deckster.Server.Games.Uno;

public class UnoProjection : GameProjection<UnoGame>
{
    public UnoGame Create(UnoGameCreatedEvent created)
    {
        return UnoGame.Instantiate(created);
    }
    
    public override (UnoGame game, object startEvent) Create(IGameHost host)
    {
        var startEvent = new UnoGameCreatedEvent
        {
            Id = Guid.NewGuid(),
            Name = host.Name,
            Players = host.GetPlayers(),
            Deck = UnoDeck.Standard.KnuthShuffle(new Random().Next(0, int.MaxValue)),
        };

        var game = Create(startEvent);
        return (game, startEvent);
    }

    public Task Apply(PutCardRequest @event, UnoGame game) => game.PutCard(@event);
    public Task Apply(PutWildRequest @event, UnoGame game) => game.PutWild(@event);
    public Task Apply(DrawCardRequest @event, UnoGame game) => game.DrawCard(@event);
    public Task Apply(PassRequest @event, UnoGame game) => game.Pass(@event);
}