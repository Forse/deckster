﻿using Deckster.Client;
using Deckster.Client.Games.ChatRoom;
using Deckster.Core.Games.ChatRoom;
using Deckster.Core.Serialization;

namespace Deckster.ChatRoom.SampleClient;

class Program
{
    public static async Task<int> Main(string[] argz)
    {
        try
        {
            using var cts = new CancellationTokenSource();
            var deckster = await DecksterClient.LogInOrRegisterAsync("http://localhost:13992", "Kamuf Larsen", "hest");
            await using var chatRoom = await deckster.ChatRoom().CreateAndJoinAsync("my-room", cts.Token);
            
            Console.WriteLine("Connected");
            chatRoom.PlayerSaid += m => Console.WriteLine($"Got message {m.Pretty()}");
            
            Console.CancelKeyPress += (s, e) =>
            {
                chatRoom.Dispose();
                cts.Cancel();
            };
          
            chatRoom.Disconnected += s =>
            {
                Console.WriteLine($"Disconnected: '{s}'");
                cts.Cancel();
            };
            
            while (!cts.IsCancellationRequested)
            {
                Console.WriteLine("Write message:");
                var message = await Console.In.ReadLineAsync(cts.Token);
                
                
                switch (message)
                {
                    case "quit":
                        await chatRoom.DisposeAsync();
                        return 0;
                    default:
                        Console.WriteLine($"Sending '{message}'");
                        var response = await chatRoom.ChatAsync(new SendChatRequest
                        {
                            Message = message
                        }, cts.Token);
                
                        Console.WriteLine("Response:");
                        Console.WriteLine(response.Pretty());
                        break;
                }
            }
            
            return 0;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return 1;
        }
    }
}