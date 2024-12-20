﻿using Deckster.Client.Logging;
using Deckster.Client;
using Deckster.Client.Games.TexasHoldEm;
using Microsoft.Extensions.Logging;

namespace Deckster.TexasHoldEm.SampleClient;

internal class Program
{
    public static async Task<int> Main(string[] argz)
    {
        try
        {
            var logger = Log.Factory.CreateLogger("TexasHoldEm");
            const string gameName = "my-texas-game";
            using var cts = new CancellationTokenSource();
            Console.CancelKeyPress += (s, e) =>
            {
                if (cts.IsCancellationRequested)
                {
                    return;
                }

                cts.Cancel();
            };

            var deckster = await DecksterClient.LogInOrRegisterAsync("http://localhost:13992", "Kamuf Larsen", "hest");
            var client = deckster.TexasHoldEm();
            logger.LogInformation("Creating game {name}", gameName);
            var info = await client.CreateAsync(gameName, cts.Token);
            logger.LogInformation("Adding bot");

            for (var ii = 0; ii < 3; ii++)
            {
                await client.AddBotAsync(gameName, cts.Token);
            }

            logger.LogInformation("Joining game {name}", gameName);
            await using var game = await client.JoinAsync(gameName, cts.Token);

            logger.LogInformation("Using ai");
            var ai = new TexasHoldEmPoorAi(game);
            logger.LogInformation("Starting game");
            await client.StartGameAsync(gameName, cts.Token);
            logger.LogInformation("Playing game");
            await ai.PlayAsync(cts.Token);

            return 0;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return 1;
        }
    }
}

