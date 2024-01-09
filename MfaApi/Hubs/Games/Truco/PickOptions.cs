namespace MfaApi.Hubs.Games.Truco;

public class PickOptions
{
    public int Quantity { get; private set; } 
    public DrawType Draw { get; private set; }

    public PickOptions() : this(1, DrawType.Top) { }

    public PickOptions(int quantity) : this(quantity, DrawType.Top) { }

    public PickOptions(DrawType draw) : this(1, draw) { }

    public PickOptions(int quantity, DrawType draw)
    {
        if(quantity <= 0)
        {
            quantity = 1;
        }

        this.Quantity = quantity;
        this.Draw = draw;
    }
}
