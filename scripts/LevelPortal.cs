using Godot;

[GlobalClass]
public partial class LevelPortal : Node2D
{
    [Export] public Vector2I PortalCoordinate { get; set; }
    [Export] public Level Level { get; set; }
    [Export] public Train Train { get; set; }

    private PackedScene levelTwoScene;

    private bool activated = false;

    private TileMapLayer groundLayer;

    public override void _Ready()
    {
        groundLayer = GetTree().CurrentScene.GetNode<TileMapLayer>("GridManager/Ground");

        levelTwoScene = GD.Load<PackedScene>("res://scenes/LevelTwo.tscn");
    }

    public override void _Process(double delta)
    {
        var trainCoord = groundLayer.LocalToMap(Train.Head.GetTrainPosition());
        if (trainCoord == PortalCoordinate && !activated)
        {
            activated = true;
            GetTree().ChangeSceneToPacked(levelTwoScene);
        }
    }
}