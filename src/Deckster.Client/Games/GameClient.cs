using Deckster.Client.Logging;
using Deckster.Core.Communication;
using Deckster.Core.Games.Common;
using Deckster.Core.Games.Gabong;
using Deckster.Core.Protocol;
using Deckster.Core.Serialization;
using Microsoft.Extensions.Logging;

namespace Deckster.Client.Games;

public interface IGameClient : IDisposable, IAsyncDisposable;

public abstract class GameClient : IGameClient
{
    protected readonly ILogger Logger;
    protected readonly IClientChannel Channel;
    public event Action<string>? Disconnected;
    
    public PlayerData PlayerData => Channel.Player;

    protected GameClient(IClientChannel channel)
    {
        Channel = channel;
        
        channel.OnDisconnected += reason => Disconnected?.Invoke(reason);
        channel.StartReadNotifications<DecksterNotification>(OnNotification, DecksterJson.Options);
        Logger = Log.Factory.CreateLogger($"{GetType().Name} {PlayerData.Name}");
    }

    protected abstract void OnNotification(DecksterNotification notification);

    public async Task<TResponse> SendAsync<TResponse>(DecksterRequest request, bool throwOnError, CancellationToken cancellationToken = default)
    {
        //Logger.LogTrace($"Sending {request.Pretty()}");
        var response = await Channel.SendAsync<DecksterResponse>(request, DecksterJson.Options, cancellationToken);
        //Logger.LogTrace($"Got response {response.Pretty()}");
        
        if (response is {HasError: true})
        {
            if (throwOnError)
            {
                throw new OhNoesException(response.Error);
            }
        }
        return response switch
        {
            null => throw new Exception("OMG RESPAWNS IZ NULLZ"),
            TResponse expected => expected,
            _ => throw new Exception($"Unknown result '{response.GetType().Name}'")
        };
    }

    public async Task DisconnectAsync()
    {
        await Channel.DisconnectAsync();
    }

    public void Dispose()
    {
        Channel.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await Channel.DisposeAsync();
    }
}

public class OhNoesException : Exception
{
    public OhNoesException(string? message) : base(message)
    {
        
    }
}