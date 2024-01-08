using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace MfaApi.Hubs.Games.Truco;

public class TrucoLobby
{
    // FIFO queue of games waiting to be played.
    private readonly ConcurrentQueue<TrucoGame> _waitingGames = new();

    // The set of active games
    private readonly ConcurrentDictionary<string, TrucoGame> _activeGames = new();

    private readonly IServiceProvider _serviceProvider;

    public TrucoLobby(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<TrucoGame> AddPlayerToGameAsync(HubCallerContext hubCallerContext)
    {
        // Try to get a waiting game from the queue (the longest waiting game is served first FIFO)
        if (_waitingGames.TryPeek(out var game))
        {
            // Try to add the player to this game. It'll return false if the game is full.
            if (!await game.AddPlayerAsync(hubCallerContext.ConnectionId))
            {
                // Game is full
            }
            else
            {
                //// A player was added into the game room

                //// When the player disconnects, remove him from the game
                //hubCallerContext.ConnectionAborted.Register(() =>
                //{
                //    // We can't wait here (since this is synchronous), so fire and forget
                //    _ = game.RemovePlayerAsync(hubCallerContext.ConnectionId);
                //});

                //// When the game ends, remove the game from the player (he can join another game)
                //game.Completed.Register(() => hubCallerContext.Items.Remove(_gameKey));
            }
        }

        // If there are no games available create a new one requesting our transient object
        var newGame = _serviceProvider.GetRequiredService<TrucoGame>();

        _waitingGames.Enqueue(newGame);

        return newGame;
    }
}
