using Ardalis.SmartEnum;

namespace MfaApi.Hubs.Games.Truco;

public sealed class SuitType : SmartEnum<SuitType, string>
{
    public static readonly SuitType Diamonds = new SuitType(nameof(Diamonds), "D");
    public static readonly SuitType Spades = new SuitType(nameof(Spades), "S");
    public static readonly SuitType Hearts = new SuitType(nameof(Hearts), "H");
    public static readonly SuitType Clubs = new SuitType(nameof(Clubs), "C");

    private SuitType(string name, string value) : base(name, value)
    {
    }
}
