using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace MfaApi.Hubs.Games.Truco;

public class TrucoLobby
{
    //Injected
    private readonly IServiceProvider _serviceProvider;

    // FIFO queue of games waiting to be played.
    private readonly ConcurrentQueue<TrucoGame> _waitingGames = new();

    // The set of active games
    private readonly ConcurrentDictionary<string, TrucoGame> _activeGames = new();

    public TrucoLobby(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<TrucoGame> AddPlayerToGameAsync(HubCallerContext hubCallerContext)
    {
        while (true)
        {
            // Try to get a waiting game from the queue (the longest waiting game is served first FIFO)
            if (_waitingGames.TryPeek(out var game))
            {
                // Try to add the player to this game | if full will return false
                if (!await game.AddPlayerAsync(hubCallerContext.ConnectionId))
                {
                    // Game is full

                    // We're unable to use this waiting game, so make it an active game.
                    if (_activeGames.TryAdd(game.Name, game))
                    {
                        // Remove the game when it completes
                        game.Completed.UnsafeRegister(_ =>
                        {
                            _activeGames.TryRemove(game.Name, out var _);
                        },
                        null);

                        // Remove it from the list of waiting games after we've made it active
                        _waitingGames.TryDequeue(out _);
                    }

                    continue;
                }
                else
                {
                    // A player was added into the game room

                    // When the player disconnects, remove him from the game
                    hubCallerContext.ConnectionAborted.Register(() =>
                    {
                        //TODO: probably remove and end the game checking WaitingForPlayers

                        // We can't wait here (since this is synchronous), so fire and forget
                        _ = game.RemovePlayerAsync(hubCallerContext.ConnectionId);
                    });

                    //// When the game ends, remove the game from the player (he can join another game)
                    //game.Completed.Register(() => hubCallerContext.Items.Remove(_gameKey));
                }

                return game;
            }

            // If there are no games available create a new one requesting our transient object
            var newGame = _serviceProvider.GetRequiredService<TrucoGame>();

            _waitingGames.Enqueue(newGame);
        }
    }
}
