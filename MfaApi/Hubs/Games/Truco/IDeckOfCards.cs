namespace MfaApi.Hubs.Games.Truco;

public interface IDeckOfCards
{
    Card[] GetCards();
    void Shuffle(int times = 1);
    Card[] Pick(PickOptions options);
    DrawType? DrawStrategy { get; set; }
}
