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

    public PathInfo PathInfo;

    private Color COLOR_ONE = Colors.BlueViolet;
    private Color COLOR_TWO = Colors.PaleVioletRed;
    public Color Color = Colors.BlueViolet;

    public float HeightOffset = 0f;
    private const float MAX_HEIGHT_OFFSET = 3f;

    private TileMapLayer groundLayer;

	private GridManager gridManager;

    private Tween colorTween;
    private Tween heightTween;

    private AnimatedSprite2D back;
    private AnimatedSprite2D middle;
    private AnimatedSprite2D front;

    private Dictionary<CargoType, int> backFrameForCargoType = new Dictionary<CargoType, int>()
    {
        { CargoType.Purple, 0 },
        { CargoType.None, 3 },
        { CargoType.Pink, 6 },
    };

    private Dictionary<CargoType, int> middleFrameForCargoType = new Dictionary<CargoType, int>()
    {
        { CargoType.Purple, 1 },
        { CargoType.None, 4 },
        { CargoType.Pink, 7 },
    };

    private Dictionary<CargoType, int> frontFrameForCargoType = new Dictionary<CargoType, int>()
    {
        { CargoType.Purple, 2 },
        { CargoType.None, 5 },
        { CargoType.Pink, 8 },
    };

    public float ProgressRatio;


    public override void _Ready()
    {
        groundLayer = GetTree().CurrentScene.GetNode<TileMapLayer>("GridManager/Ground");

        back = GetNode<AnimatedSprite2D>("Back");
        middle = GetNode<AnimatedSprite2D>("Middle");
        front = GetNode<AnimatedSprite2D>("Front");

        back.Frame = backFrameForCargoType[CargoType];
        middle.Frame = middleFrameForCargoType[CargoType];
        front.Frame = frontFrameForCargoType[CargoType];

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

    }

    public void Initialize()
    {
        PathInfo = GetPathFromTarget(TrainTargetLocation);
        ProgressRatio = (float)(TrainTargetLocation.X - PathInfo.StartCoordinate.X) / (PathInfo.EndCoordinate.X - PathInfo.StartCoordinate.X);
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
			}	
		}
		return null;
	}
}