using Godot;

[GlobalClass]
public partial class Train : Node2D
{
    [Export] public Direction InitialDirection { get; set; } = Direction.NegY;

    [Export] public HeadTraincar Head { get; set; }
    [Export] public FollowTraincar Middle { get; set; }
    [Export] public FollowTraincar Tail { get; set; }

    public CargoType CarriedCargo = CargoType.Orange;

    private TileMapLayer groundLayer;
}