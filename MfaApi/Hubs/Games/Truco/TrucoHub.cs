using Microsoft.AspNetCore.SignalR;

namespace MfaApi.Hubs.Games.Truco;

public class TrucoHub : Hub<ITrucoClient>
{
    private readonly TrucoLobby _lobby;

    public TrucoHub(TrucoLobby lobby)
    {
        _lobby = lobby;
    }

    public async Task<string> JoinGame()
    {
        TrucoGame game = await _lobby.AddPlayerToGameAsync(this.Context);
        return "a";
    }

    //public async Task<string> JoinGame(string gameName)
    //{

    //}
    //public override async Task OnConnectedAsync()
    //{
    //    await this.Groups.AddToGroupAsync(Context.ConnectionId, "JoinGame");
    //    await this.Clients.Caller.WriteMessage("Connected.");
    //    //await this.Clients.GroupExcept("JoinGame", Context.ConnectionId).WriteMessage($"A new player joined game {Name}");
    //}

    //public override async Task OnDisconnectedAsync(Exception? exception)
    //{
    //    await this.Clients.Caller.WriteMessage("Disconnected.");
    //    await base.OnDisconnectedAsync(exception);
    //}
}
