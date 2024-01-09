using Ardalis.SmartEnum;

namespace MfaApi.Hubs.Games.Truco;

public sealed class DrawType : SmartEnum<DrawType, string>
{
    public static readonly DrawType Top = new DrawType(nameof(Top), "top");
    public static readonly DrawType Bottom = new DrawType(nameof(Bottom), "bottom");
    public static readonly DrawType Random = new DrawType(nameof(Random), "random");

    private DrawType(string name, string value) : base(name, value)
    {
    }
}
