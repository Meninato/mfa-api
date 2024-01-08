namespace MfaApi.Hubs.Games.Truco;

public interface IDeckOfCards
{
    Card[] Cards { get; }
    Card[] GetCards();
    void Shuffle(int? times);
    Card[] Pick(PickOptions? options);
}
