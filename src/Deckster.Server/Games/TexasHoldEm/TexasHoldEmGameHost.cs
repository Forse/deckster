using System.Diagnostics.CodeAnalysis;
using Deckster.Client.Games.TexasHoldEm;
using Deckster.Core.Games.Common;
using Deckster.Games.TexasHoldEm;
using Deckster.Server.Data;
using Deckster.Server.Games.Common.Fakes;
using Deckster.TexasHoldEm.SampleClient;

namespace Deckster.Server.Games.TexasHoldEm;

public class TexasHoldEmGameHost : StandardGameHost<TexasHoldEmGame>
{
    public override string GameType => "TexasHoldEm";

    private readonly List<TexasHoldEmPoorAi> _bots = [];

    public TexasHoldEmGameHost(IRepo repo, ILoggerFactory loggerFactory) : base(repo, loggerFactory, new TexasHoldEmProjection(), 4)
    {
    }

    public override bool TryAddBot([MaybeNullWhen(true)] out string error)
    {
        var channel = new InMemoryChannel
        {
            Player = new PlayerData
            {
                Id = Guid.NewGuid(),
                Name = TestUserNames.Random()
            }
        };
        var bot = new TexasHoldEmPoorAi(new TexasHoldEmClient(channel));
        _bots.Add(bot);
        return TryAddPlayer(channel, out error);
    }
}