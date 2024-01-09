namespace MfaApi.Hubs.Games.Truco;

public class Card
{
    public SuitType Suit { get; private set; }
    public RankType Rank { get; private set; } 
    public FaceType Face { get; set; }
    public int Value { get; set; }

    public Card(SuitType suit, RankType rank) : this(suit, rank, FaceType.Up, 0) { }

    public Card(SuitType suit, RankType rank, FaceType face) : this(suit, rank, face, 0) { }

    public Card(SuitType suit, RankType rank, FaceType face, int value)
    {
        this.Suit = suit;
        this.Rank = rank;
        this.Value = value;
        this.Face = face;
    }
}
