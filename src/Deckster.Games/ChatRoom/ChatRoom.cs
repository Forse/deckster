using Deckster.Core.Games.ChatRoom;

namespace Deckster.Games.ChatRoom;

public class ChatRoom : GameObject
{
    public event NotifyAll<ChatNotification>? PlayerSaid; 
    
    protected override GameState GetState() => GameState.Running;
    
    public List<SendChatRequest> Transcript { get; init; } = [];

    public static ChatRoom Instantiate(ChatCreatedEvent e)
    {
        return new ChatRoom
        {
            Id = e.Id,
            Name = e.Name,
            StartedTime = e.StartedTime,
        };
    }
    
    public async Task<ChatResponse> ChatAsync(SendChatRequest request)
    {
        Transcript.Add(request);
        var response = new ChatResponse();
        await RespondAsync(request.PlayerId, response);
        await PlayerSaid.InvokeOrDefault(() => new ChatNotification
        {
            Sender = request.PlayerId.ToString(),
            Message = request.Message
        });
        
        return response;
    }

    public override Task StartAsync()
    {
        return Task.CompletedTask;
    }
}

