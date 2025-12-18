using Godot;

[GlobalClass]
public partial class LevelState : Node
{
    [Export] public int PinkCargoRequired { get; set; } = 0;
    [Export] public int PurpleCargoRequired { get; set; } = 0;

    public int PinkCargoDelivered = 0;
    public int PurpleCargoDelivered = 0;

    public bool QuotaMet()
    {
        if (PinkCargoDelivered >= PinkCargoRequired && PurpleCargoDelivered >= PurpleCargoRequired)
        {
            return true;
        }
        return false;
    }
}