using System.Diagnostics.CodeAnalysis;
using Deckster.Bullshit.SampleClient;
using Deckster.Client.Games.Bullshit;
using Deckster.Core.Games.Common;
using Deckster.Games.Bullshit;
using Deckster.Server.Data;
using Deckster.Server.Games.Common.Fakes;

namespace Deckster.Server.Games.Bullshit;

public class BullshitGameHost : StandardGameHost<BullshitGame>
{
    public override string GameType => "Bullshit";
    private readonly List<BullshitPoorAi> _bots = new();
    
    public BullshitGameHost(IRepo repo, ILoggerFactory loggerFactory) : base(repo, loggerFactory, new BullshitProjection(), 6)
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
        var bot = new BullshitPoorAi(new BullshitClient(channel), LoggerFactory.CreateLogger(channel.Player.Name));
        _bots.Add(bot);
        return TryAddPlayer(channel, out error);
    }
}