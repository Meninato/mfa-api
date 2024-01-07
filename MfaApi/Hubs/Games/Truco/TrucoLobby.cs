using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace MfaApi.Hubs.Games.Truco;

public class TrucoLobby
{
    // The key into the per connection dictionary used to look up the current game;
    // Right now this is just flagging a current context of a browser.
    private static readonly object _gameKey = new();

    // FIFO queue of games waiting to be played.
    private readonly ConcurrentQueue<TrucoGame> _waitingGames = new();

    //private readonly ConcurrentDictionary = new();

    public async Task<TrucoGame> AddPlayerToGameAsync(HubCallerContext hubCallerContext)
    {
        // There's already a game associated with this player, just return it
        // TODO: this only works for the same tab, if you open a different tab a new ID gonna be generated or if you 
        // refresh the page too. Add a dictionary instead
        if (hubCallerContext.Items[_gameKey] is TrucoGame g)
        {
            return g;
        }
        else
        {
            hubCallerContext.Items[_gameKey] = new TrucoGame();
        }

        //// Try to add the player to this game. It'll return false if the game is full.
        //if(false)
        //{

        //}
        //else
        //{
        //    // Store the association of this player to this game
        //    hubCallerContext.Items[_gameKey] = game;
        //}

        return new TrucoGame();
    }
}
