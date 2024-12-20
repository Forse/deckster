using System.Diagnostics.CodeAnalysis;
using Deckster.Client.Games.CrazyEights;
using Deckster.Core.Games.Common;
using Deckster.CrazyEights.SampleClient;
using Deckster.Games.CrazyEights;
using Deckster.Server.Data;
using Deckster.Server.Games.Common.Fakes;

namespace Deckster.Server.Games.CrazyEights;

public class CrazyEightsGameHost : StandardGameHost<CrazyEightsGame>
{
    public override string GameType => "CrazyEights";

    private readonly List<CrazyEightsPoorAi> _bots = [];

    public CrazyEightsGameHost(IRepo repo, ILoggerFactory loggerFactory) : base(repo, loggerFactory, new CrazyEightsProjection(), 6)
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
        var bot = new CrazyEightsPoorAi(new CrazyEightsClient(channel));
        _bots.Add(bot);
        return TryAddPlayer(channel, out error);
    }
}