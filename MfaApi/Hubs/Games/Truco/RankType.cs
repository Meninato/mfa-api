using Ardalis.SmartEnum;

namespace MfaApi.Hubs.Games.Truco;

public sealed class RankType : SmartEnum<RankType, string>
{
    public static readonly RankType Two = new RankType(nameof(Two), "2");
    public static readonly RankType Three = new RankType(nameof(Three), "3");
    public static readonly RankType Four = new RankType(nameof(Four), "4");
    public static readonly RankType Five = new RankType(nameof(Five), "5");
    public static readonly RankType Six = new RankType(nameof(Six), "6");
    public static readonly RankType Seven = new RankType(nameof(Seven), "7");
    public static readonly RankType Eight = new RankType(nameof(Eight), "8");
    public static readonly RankType Nine = new RankType(nameof(Nine), "9");
    public static readonly RankType Ten = new RankType(nameof(Ten), "10");
    public static readonly RankType Jack = new RankType(nameof(Jack), "J");
    public static readonly RankType Queen = new RankType(nameof(Queen), "Q");
    public static readonly RankType King = new RankType(nameof(King), "K");
    public static readonly RankType Ace = new RankType(nameof(Ace), "A");

    private RankType(string name, string value) : base(name, value)
    {
    }
}
