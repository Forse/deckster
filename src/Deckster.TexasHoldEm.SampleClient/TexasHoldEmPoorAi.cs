using Deckster.Client.Games.TexasHoldEm;
using Deckster.Client.Logging;
using Deckster.Core.Games.TexasHoldEm;
using Microsoft.Extensions.Logging;

namespace Deckster.TexasHoldEm.SampleClient;

public class TexasHoldEmPoorAi
{
    private readonly ILogger _logger;
    private int _turn;
    private PlayerViewOfGame _view = new();
    private GameStartedNotification _game;
    private readonly TexasHoldEmClient _client;
    private readonly TaskCompletionSource _tcs = new();


    public TexasHoldEmPoorAi(TexasHoldEmClient client)
    {
        _client = client;
        _logger = Log.Factory.CreateLogger(client.PlayerData.Name);
        client.PlayerBetted += PlayerBetted;
        client.PlayerAllIn += PlayerAllIn;
        client.PlayerChecked += PlayerChecked;
        client.PlayerFolded += PlayerFolded;
        client.PlayerCalled += PlayerCalled;
        client.NewCardsRevealed += NewCardsRevealed;
        client.RoundStarted += RoundStarted;
        client.RoundEnded += RoundEnded;
        client.ItsYourTurn += ItsMyTurn;
        client.GameStarted += GameStarted;
        client.GameEnded += GameEnded;
    }

    private void GameStarted(GameStartedNotification notification)
    {
        _logger.LogInformation("Game started. GameId: {id}", notification.GameId);
        _game = notification;
        _view = notification.PlayerViewOfGame;
    }

    private void GameEnded(GameEndedNotification notification)
    {
        _logger.LogInformation($"Game ended. Players: [{string.Join(", ", notification.Players.Select(p => p.Name))}]");
        _tcs.SetResult();
    }

    public Task PlayAsync(CancellationToken cancellationToken)
    {
        cancellationToken.Register(_tcs.SetCanceled);
        return _tcs.Task;
    }

    private async void ItsMyTurn(ItsYourTurnNotification notification)
    {
        var turn = _turn++;
        try
        {
            var cards = notification.PlayerViewOfGame.Cards;
            var currentBiggestBet = notification.PlayerViewOfGame.OtherPlayers.Max(i => i.CurrentBet);

            _logger.LogDebug("It's my turn. Cards on board: {cardsOnBoard}. I have: {cards} ({turn}).",
                string.Join(", ", notification.PlayerViewOfGame.CardsOnTable),
                string.Join(", ", cards),
                turn);

            _logger.LogDebug("It's my turn. The bet is: {betSize}. I have betted: {myBet}. My stack size {stackSize} ({turn}).",
                currentBiggestBet,
                notification.PlayerViewOfGame.CurrentBet,
                notification.PlayerViewOfGame.StackSize,
                turn);

            _view = notification.PlayerViewOfGame;

            if (notification.PlayerViewOfGame.CurrentBet == currentBiggestBet)
            {
                await _client.PlayerCheckRequestAsync();
                _logger.LogDebug("I currently have the biggest bet, checking");
            }
            else
            {
                var betSize = currentBiggestBet - notification.PlayerViewOfGame.CurrentBet;
                await _client.PlayerBetRequestAsync(betSize);
                _logger.LogDebug("I match the current bet, betting {betSize}", betSize);
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Argh ({turn})", turn);
            throw;
        }
        _logger.LogDebug("Done ({turn})", turn);
    }

    private void PlayerBetted(PlayerBettedNotification notification)
    {
        _logger.LogTrace("{playerId} betted {bet}", notification.PlayerId, notification.BetSize);
    }

    private void PlayerAllIn(PlayerAllInNotification notification)
    {
        _logger.LogTrace("{playerId} went all in", notification.PlayerId);
    }

    private void PlayerChecked(PlayerCheckedNotification notification)
    {
        _logger.LogTrace("{playerId} checked", notification.PlayerId);
    }

    private void PlayerFolded(PlayerFoldedNotification notification)
    {
        _logger.LogTrace("{playerId} folded", notification.PlayerId);
    }

    private void PlayerCalled(PlayerCalledNotification notification)
    {
        _logger.LogTrace("{playerId} called the bet", notification.PlayerId);
    }

    private void PlayerIsOutOfGame(PlayerIsOutOfGameNotification notification)
    {
        _logger.LogTrace("{playerId} is out of the game", notification.PlayerId);
    }

    private void NewCardsRevealed(NewCardsRevealed notification)
    {
        _logger.LogTrace("New cards {cards} revealed ", string.Join(",", notification.Cards));
    }

    private void RoundStarted(NewRoundStartedNotification notification)
    {
        _logger.LogTrace("New round in {gameId} started", notification.GameId);
    }

    private void RoundEnded(RoundEndedNotification notification)
    {
        _logger.LogTrace("Round ended, Winner {playerId}. Winnings {winnings}", notification.WinnerId, notification.Winnings);
    }
}