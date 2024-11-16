using Deckster.Client.Games.ChatRoom;
using Deckster.Core.Games.ChatRoom;

namespace Deckster.ChatRoom.SampleClient;

public class TronderBot
{
    private readonly ChatRoomClient _client;
    private readonly TaskCompletionSource _tcs = new();

    private static string[] Phrases = ReadEmbedded("phrases.txt");

    public TronderBot(ChatRoomClient client)
    {
        _client = client;
        client.PlayerSaid += PlayerSaid;
        client.Disconnected += Disconnected;
    }

    private void Disconnected(string reason)
    {
        
    }

    private async void PlayerSaid(ChatNotification n)
    {
        if (new Random().Next(0, 100) > 50)
        {
            await _client.ChatAsync(RandomPhrase());
        }
    }

    private static string RandomPhrase()
    {
        var index = new Random().Next(0, Phrases.Length);
        return Phrases[index];
    }

    private static string[] ReadEmbedded(string filename)
    {
        var assembly = typeof(TronderBot).Assembly; 
        var names = assembly.GetManifestResourceNames();
        var name = names.FirstOrDefault(n => n.EndsWith(filename));
        if (name == null)
        {
            throw new Exception($"OMG CANT HAZ EMBDEDED {filename}");
        }

        using var stream = assembly.GetManifestResourceStream(name);
        if (stream == null)
        {
            throw new Exception($"OMG CANT HAZ EMBDEDED {filename}");
        }

        using var reader = new StreamReader(stream);
        return reader.ReadToEnd().Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
    }
}