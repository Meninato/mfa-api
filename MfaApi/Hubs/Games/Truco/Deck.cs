using System.Collections.Immutable;

namespace MfaApi.Hubs.Games.Truco;

public class Deck : IDeckOfCards
{
    public Card[] Cards { get; private set; }

    private Deck(Card[] cards)
    {
        this.Cards = cards;
    }

    public Card[] GetCards()
    {
        return Cards;
    }

    public void Shuffle(int? times)
    {
        // In-place Fisher-Yates shuffle
        for (int i = 0; i < this.Cards.Length - 1; ++i)
        {
            int j = Random.Shared.Next(i, this.Cards.Length);
            (this.Cards[j], this.Cards[i]) = (this.Cards[i], this.Cards[j]);
        }
    }

    public Card[] Pick(PickOptions? options)
    {
        throw new NotImplementedException();
    }
}
