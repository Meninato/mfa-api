using Ardalis.SmartEnum;

namespace MfaApi.Hubs.Games.Truco;

public sealed class FaceType : SmartEnum<FaceType, string>
{
    public static readonly FaceType Up = new FaceType(nameof(Up), "up");
    public static readonly FaceType Down = new FaceType(nameof(Down), "down");

    private FaceType(string name, string value) : base(name, value)
    {
    }
}
