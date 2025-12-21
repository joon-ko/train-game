using System.Collections.Generic;
using Godot;

public enum PlatformType
{
    Pickup,
    Delivery
}

public enum CargoType
{
    Purple,
    None,
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
    [Export] public int MaxCargoAwarded { get; set; } = 20;
    [Export] public Train Train;

    public PathInfo PathInfo;

    private Color COLOR_ONE = Colors.BlueViolet;
    private Color COLOR_TWO = Colors.PaleVioletRed;
    public Color Color = Colors.BlueViolet;

    public float HeightOffset = 0f;
    private const float MAX_HEIGHT_OFFSET = 3f;

    private GridManager gridManager;
    private TileMapLayer groundLayer;
    private Node2D presents;

    private Tween colorTween;
    private Tween heightTween;

    private Sprite2D back;
    private Sprite2D middle;
    private Sprite2D front;

    public float ProgressRatio;

    public override void _Ready()
    {
        groundLayer = GetTree().CurrentScene.GetNode<TileMapLayer>("GridManager/Ground");

        back = GetNode<Sprite2D>("Back");
        middle = GetNode<Sprite2D>("Middle");
        front = GetNode<Sprite2D>("Front");

        colorTween = CreateTween();
        colorTween.SetLoops();
        colorTween.SetTrans(Tween.TransitionType.Sine);
        colorTween.SetEase(Tween.EaseType.InOut);
        colorTween.TweenProperty(this, "Color", COLOR_TWO, 2.0);
        colorTween.TweenProperty(this, "Color", COLOR_ONE, 2.0);

        heightTween = CreateTween();
        heightTween.SetLoops();
        heightTween.SetTrans(Tween.TransitionType.Sine);
        heightTween.SetEase(Tween.EaseType.InOut);
        heightTween.TweenProperty(this, "HeightOffset", MAX_HEIGHT_OFFSET, 1.5);
        heightTween.TweenProperty(this, "HeightOffset", 0f, 1.5);

        gridManager = GetTree().CurrentScene.GetNode<GridManager>("GridManager");

        if (CargoType != CargoType.None)
        {
            presents = GetNode<Node2D>("Presents");
        }
    }

    public void Initialize()
    {
        PathInfo = GetPathFromTarget(TrainTargetLocation);
        if (TrainTargetLocation.Y == PathInfo.StartCoordinate.Y)
        {
            ProgressRatio = (float)(TrainTargetLocation.X - PathInfo.StartCoordinate.X) / (PathInfo.EndCoordinate.X - PathInfo.StartCoordinate.X);
        } else if (TrainTargetLocation.X == PathInfo.StartCoordinate.X)
        {
            ProgressRatio = (float)(TrainTargetLocation.Y - PathInfo.StartCoordinate.Y) / (PathInfo.EndCoordinate.Y - PathInfo.StartCoordinate.Y);
        }
    }

    public override void _Process(double delta)
    {
        if (CargoType != CargoType.None)
        {
            // Don't show presents if the train picked them up
            presents.Visible = Train.CarriedCargo != CargoType;
        }
    }

    // public override void _PhysicsProcess(double delta)
    // {
    //     QueueRedraw();
    // }

    // public override void _Draw()
    // {
    //     var targetPosition = groundLayer.MapToLocal(TrainTargetLocation);
    //     var platformPosition = groundLayer.MapToLocal(Location);
    //     var delta = targetPosition - platformPosition;
    //     DrawCircle(delta + new Vector2(0, -1 * HeightOffset), 4, Color);
    // }

    private PathInfo GetPathFromTarget(Vector2I target)
    {
        foreach (var paths in gridManager.TrainPaths.Values)
        {
            foreach (var path in paths)
            {
                if (path.EndCoordinate.Y == target.Y)
                {
                    if ((path.EndCoordinate.X < target.X && path.StartCoordinate.X > target.X)
                        || (path.EndCoordinate.X > target.X && path.StartCoordinate.X < target.X))
                    {
                        return path;
                    }
                }
                if (path.EndCoordinate.X == target.X)
                {
                    if ((path.EndCoordinate.Y < target.Y && path.StartCoordinate.Y > target.Y)
                        || (path.EndCoordinate.Y > target.Y && path.StartCoordinate.Y < target.Y))
                    {
                        return path;
                    }

                }
            }
        }
        return null;
    }
}