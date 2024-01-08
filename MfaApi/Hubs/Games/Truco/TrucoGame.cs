using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Threading.Channels;

namespace MfaApi.Hubs.Games.Truco;

public class TrucoGame
{
    //Injected
    private readonly TrucoSettings _settings;
    private readonly IHubContext<TrucoHub, ITrucoClient> _hubContext;

    // Number of open player slots in this game
    private readonly Channel<int> _playerSlots;
    private readonly TimeSpan _playTimeout;

    // Player state keyed by connection id
    private readonly ConcurrentDictionary<string, TrucoPlayer> _players = new();

    public ITrucoClient Group { get; }

    // Notification when the game is completed
    //private readonly CancellationTokenSource _completedCts = new();

    public string Name { get; private set; }

    public bool WaitingForPlayers { get; private set; } = true;

    public TrucoGame(IOptions<TrucoSettings> settings, 
        IHubContext<TrucoHub, ITrucoClient> hubContext)
    {
        _settings = settings.Value;
        _hubContext = hubContext;
        _playerSlots = Channel.CreateBounded<int>(_settings.MaxPlayersPerGame);
        this.Name = RandomNameGenerator.GenerateRandomName();
        this.Group = _hubContext.Clients.Group(this.Name);

        // Give the client some buffer
        _playTimeout = TimeSpan.FromSeconds(_settings.TimePerPlayInSeconds + 5);

        // Fill the slots for this game
        for (int i = 0; i < _settings.MaxPlayersPerGame; i++)
        {
            _playerSlots.Writer.TryWrite(0);
        }
    }

    public async Task<bool> AddPlayerAsync(string connectionId)
    {
        bool result = false;

        // Try to grab a player slot
        if (_playerSlots.Reader.TryRead(out _))
        {
            // We succeeded so set up this player
            _players.TryAdd(connectionId, new TrucoPlayer()
            {
                Proxy = _hubContext.Clients.Client(connectionId)
            });

            //Save into game name group (you can call room id)
            await _hubContext.Groups.AddToGroupAsync(connectionId, this.Name);

            //Send a message to everyone in this group except the joined one
            await _hubContext.Clients.GroupExcept(this.Name, connectionId).WriteMessage($"A new player joined game {Name}");

            // If we don't have any more slots, it means we're full, lets start the game.
            if (!_playerSlots.Reader.TryPeek(out _))
            {
                // Complete the channel so players can no longer join the game
                _playerSlots.Writer.TryComplete();

                // Check to see any slots were given back from players that might have dropped from the game while waiting on
                // the game to start. We check this after TryComplete since it means no new players can join.
                if (!_playerSlots.Reader.TryPeek(out _))
                {
                    this.WaitingForPlayers = false;

                    // We're clear, start the game
                    _ = Task.Run(PlayGame);
                }

                // More players can join, let's wait
            }

            if (this.WaitingForPlayers)
            {
                await Group.WriteMessage($"Waiting for {_playerSlots.Reader.Count} player(s) to join.");
            }

            //TODO: return kind of status to determine if the game is full or not to avoid loop

            //A player was added to the game
            result = true;
        }

        return result;
    }

    public async Task RemovePlayerAsync(string connectionId)
    {
        // This should never be false, since we only remove players from games they are associated with
        if (_players.TryRemove(connectionId, out _))
        {
            // If the game hasn't started (the channel was not completed for e.g.), we can give this slot back to the game.
            _playerSlots.Writer.TryWrite(0);

            await this.Group.WriteMessage($"A player has left the game");
        }
    }

    private async Task PlayGame()
    {

    }

}
