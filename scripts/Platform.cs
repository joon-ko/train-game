using Godot;

public enum PlatformType
{
    Pickup,
    Delivery
}

public enum CargoType
{
    Purple,
    Orange,
    Star,
    Pink
}

[GlobalClass]
public partial class Platform : Node2D
{
    [Export] public PlatformType PlatformType { get; set; } = PlatformType.Pickup;
    [Export] public CargoType CargoType { get; set; } = CargoType.Purple;
    [Export] public Vector2I Location { get; set; }
    [Export] public Vector2I TrainTargetLocation { get; set; }


}