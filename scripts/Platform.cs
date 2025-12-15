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

    public PathInfo PathInfo;

    private Color COLOR_ONE = Colors.BlueViolet;
    private Color COLOR_TWO = Colors.PaleVioletRed;
    public Color Color = Colors.BlueViolet;

    public float HeightOffset = 0f;
    private const float MAX_HEIGHT_OFFSET = 3f;

    private TileMapLayer groundLayer;

    private Tween colorTween;
    private Tween heightTween;

    private AnimatedSprite2D back;
    private AnimatedSprite2D middle;
    private AnimatedSprite2D front;

    private Dictionary<CargoType, int> backFrameForCargoType = new Dictionary<CargoType, int>()
    {
        { CargoType.Purple, 0 },
        { CargoType.Orange, 3 },
        { CargoType.Pink, 6 },
    };

    private Dictionary<CargoType, int> middleFrameForCargoType = new Dictionary<CargoType, int>()
    {
        { CargoType.Purple, 1 },
        { CargoType.Orange, 4 },
        { CargoType.Pink, 7 },
    };

    private Dictionary<CargoType, int> frontFrameForCargoType = new Dictionary<CargoType, int>()
    {
        { CargoType.Purple, 2 },
        { CargoType.Orange, 5 },
        { CargoType.Pink, 8 },
    };

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
    }

    public override void _PhysicsProcess(double delta)
    {
        QueueRedraw();
    }

    public override void _Draw()
    {
        var targetPosition = groundLayer.MapToLocal(TrainTargetLocation);
        var platformPosition = groundLayer.MapToLocal(Location);
        var delta = targetPosition - platformPosition;
        DrawCircle(delta + new Vector2(0, -1 * HeightOffset), 4, Color);
    }
}