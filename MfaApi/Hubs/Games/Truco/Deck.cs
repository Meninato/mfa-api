namespace MfaApi.Hubs.Games.Truco;

public class Deck : IDeckOfCards
{
    private Dictionary<DrawType, Func<int, Card[]>> _pickStrategies = new();
    private Card[] _pickedCards = new Card[0];
    private Card[] _cards;

    public DrawType? DrawStrategy { get; set; } 

    public Deck(Card[] cards)
    {
        _cards = cards;
        _pickStrategies.Add(DrawType.Top, PickFromTop);
        _pickStrategies.Add(DrawType.Bottom, PickFromBottom);
        _pickStrategies.Add(DrawType.Random, PickRandom);
    }

    public Card[] GetCards()
    {
        return _cards;
    }

    public void Shuffle(int times = 1)
    {
        if(times <= 0)
        {
            times = 1;
        }

        for (int k = 0; k < times; k++)
        {
            // In-place Fisher-Yates shuffle
            for (int i = 0; i < _cards.Length - 1; ++i)
            {
                int j = Random.Shared.Next(i, _cards.Length);
                (_cards[j], _cards[i]) = (_cards[i], _cards[j]);
            }
        }
    }

    public Card[] Pick(PickOptions options)
    {
        DrawType draw = this.DrawStrategy ?? options.Draw;
        int qty = options.Quantity;

        return _pickStrategies[draw](qty);
    }

    private Card[] PickFromTop(int quantity)
    {
        //Pick the cards
        Card[] picked = new Card[quantity];
        Array.Copy(_cards, _cards.Length - quantity, picked, 0, quantity);

        //Refresh the cards of the deck removing picked cards
        Card[] remainingCards = new Card[_cards.Length - quantity];
        Array.Copy(_cards, 0, remainingCards, 0, _cards.Length - quantity);

        //Refresh the picked cards of the deck adding the fresh ones
        Card[] burnedCards = new Card[_pickedCards.Length + quantity];
        Array.Copy(_pickedCards, burnedCards, _pickedCards.Length);
        Array.Copy(picked, 0, burnedCards, _pickedCards.Length, picked.Length);

        //Update references
        _cards = remainingCards;
        _pickedCards = burnedCards;

        //Using LINQ
        //Card[] p = _cards.TakeLast(quantity).ToArray();
        //Card[] r = _cards.Take(_cards.Length - quantity).ToArray();
        //Card[] b = _pickedCards.Concat(p).ToArray();

        return picked;
    }

    private Card[] PickFromBottom(int quantity)
    {
        return new Card[quantity];
    }

    private Card[] PickRandom(int quantity)
    {
        return new Card[quantity];
    }
}
