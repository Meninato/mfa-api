namespace MfaApi.Hubs.Games.Truco;

public interface ITrucoClient
{
    Task WriteMessage(string message);
}
