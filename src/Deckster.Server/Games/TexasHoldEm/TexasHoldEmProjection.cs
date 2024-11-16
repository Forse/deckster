using Deckster.Core.Games.TexasHoldEm;
using Deckster.Games;
using Deckster.Games.TexasHoldEm;

namespace Deckster.Server.Games.TexasHoldEm;

public class TexasHoldEmProjection : GameProjection<TexasHoldEmGame>
{
    public TexasHoldEmGame Create(TexasHoldEmCreatedEvent created)
    {
        var game = TexasHoldEmGame.Instantiate(created);
        return game;
    }
    
    public override (TexasHoldEmGame game, object startEvent) Create(IGameHost host)
    {
        var startEvent = new TexasHoldEmCreatedEvent
        {
            Id = Guid.NewGuid(),
            Name = host.Name,
            Players = host.GetPlayers(),
            Deck = Decks.Standard().KnuthShuffle(new Random().Next(0, int.MaxValue))
        };
        var game = Create(startEvent);
        return (game, startEvent);
    }
    
    public Task Apply(BetRequest @event, TexasHoldEmGame game) => game.PlayerBetRequest(@event);
    public Task Apply(CallRequest @event, TexasHoldEmGame game) => game.PlayerCallRequest(@event);
    public Task Apply(CheckRequest @event, TexasHoldEmGame game) => game.PlayerCheckRequest(@event);
    public Task Apply(FoldRequest @event, TexasHoldEmGame game) => game.PlayerFoldRequest(@event);
}