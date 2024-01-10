namespace MfaApi.Hubs.Games.Truco;

public class Deck : IDeckOfCards
{
    private Dictionary<DrawType, Func<int, IEnumerable<Card>>> _pickStrategies = new();
    private IEnumerable<Card> _pickedCards = Enumerable.Empty<Card>();
    private IEnumerable<Card> _cards;

    public DrawType? DrawStrategy { get; set; } 

    public Deck(IEnumerable<Card> cards)
    {
        _cards = cards;
        _pickStrategies.Add(DrawType.Top, PickFromTop);
        _pickStrategies.Add(DrawType.Bottom, PickFromBottom);
        _pickStrategies.Add(DrawType.Random, PickRandom);
    }

    public Card[] GetCards()
    {
        return _cards.ToArray();
    }

    public void Shuffle(int times = 1)
    {
        if(times <= 0)
        {
            times = 1;
        }

        var shuffled = _cards.ToArray();
        for (int k = 0; k < times; k++)
        {
            for (int i = 0; i < shuffled.Length - 1; ++i)
            {
                int j = Random.Shared.Next(i, shuffled.Length);
                (shuffled[j], shuffled[i]) = (shuffled[i], shuffled[j]);
            }
        }

        _cards = shuffled;
    }

    public Card[] Pick(PickOptions options)
    {
        DrawType draw = this.DrawStrategy ?? options.Draw;
        int qty = options.Quantity;

        return _pickStrategies[draw](qty).ToArray();
    }

    private IEnumerable<Card> PickFromTop(int quantity)
    {
        IEnumerable<Card> picked = _cards.TakeLast(quantity);
        _cards = _cards.Take(_cards.Count() - quantity);
        _pickedCards = _pickedCards.Concat(picked);

        return picked;
    }

    private IEnumerable<Card> PickFromBottom(int quantity)
    {
        IEnumerable<Card> picked = _cards.Take(quantity);
        _cards = _cards.Skip(quantity).Take(_cards.Count() - quantity);
        _pickedCards = _pickedCards.Concat(picked);

        return picked;
    }

    private IEnumerable<Card> PickRandom(int quantity)
    {
        IEnumerable<Card> picked = Enumerable.Empty<Card>();
        HashSet<int> indexes = new HashSet<int>();
        var cards = _cards.ToArray();

        while(indexes.Count < quantity)
        {
            int index = Random.Shared.Next(cards.Length);
            indexes.Add(index);
        }

        foreach (int index in indexes)
        {
            picked.Append(cards[index]);
        }

        _cards = _cards.Where(card => picked.Contains(card) == false);
        _pickedCards = _pickedCards.Concat(picked);

        return picked;
    }
}
