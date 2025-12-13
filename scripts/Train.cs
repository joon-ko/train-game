using System.Collections.Generic;
using Godot;

[GlobalClass]
public partial class Train : Node2D
{
    [Signal] public delegate void FinishedPathEventHandler();

    [Export] public Vector2I InitialCoordinate;
    [Export] public float MaxSpeed { get; set; } = 0.3f;
    public float Speed;
    public Direction Direction;

    private TileMapLayer tileMapLayer;
    private PathFollow2D pathFollow;
    private Path2D path;
    private AnimatedSprite2D sprite;

    private Tween brakeTween;
    private Tween unbrakeTween;

    private bool braked = false;

    private Dictionary<Direction, string> movingAnimationMap = new Dictionary<Direction, string>()
    {
        { Direction.PosX, "pos_x_move" },
        { Direction.PosY, "pos_y_move" },
        { Direction.NegX, "neg_x_move" },
        { Direction.NegY, "neg_y_move" }
    };

    private Dictionary<Direction, string> stillAnimationMap = new Dictionary<Direction, string>()
    {
        { Direction.PosX, "pos_x_still" },
        { Direction.PosY, "pos_y_still" },
        { Direction.NegX, "neg_x_still" },
        { Direction.NegY, "neg_y_still" }
    };

    public override void _Ready()
    {
        tileMapLayer = GetTree().CurrentScene.GetNode<TileMapLayer>("GridLayers/Ground");
        path = GetNode<Path2D>("Path2D");
        pathFollow = GetNode<PathFollow2D>("Path2D/PathFollow2D");
        sprite = GetNode<AnimatedSprite2D>("Path2D/PathFollow2D/Sprite");

        pathFollow.Position = tileMapLayer.MapToLocal(InitialCoordinate);

        Speed = MaxSpeed;
    }

    public void AcceptPath(PathInfo pathInfo)
    {
        // Set the orientation of the train
        var animationMap = IsMoving() ? movingAnimationMap : stillAnimationMap;
        sprite.Animation = animationMap[pathInfo.Direction];
        Direction = pathInfo.Direction;

        // Set the new path for the train
        path.Curve.ClearPoints();
        path.Curve.AddPoint(tileMapLayer.MapToLocal(pathInfo.StartCoordinate));
        path.Curve.AddPoint(tileMapLayer.MapToLocal(pathInfo.EndCoordinate));
        pathFollow.Progress = 0f;
    }

    public void ToggleBrake()
    {
        if (braked)
        {
            _Unbrake();
            return;
        }
        _Brake();
    }

    private void _Brake()
    {
        if (unbrakeTween != null && unbrakeTween.IsValid())
        {
            unbrakeTween.Kill(); // rip
        }
        unbrakeTween = CreateTween();
        unbrakeTween.TweenProperty(this, "Speed", 0f, 2.5f)
            .SetEase(Tween.EaseType.Out)
            .SetTrans(Tween.TransitionType.Quad);
        braked = true;
    }

    private void _Unbrake()
    {
        if (brakeTween != null && brakeTween.IsValid())
        {
            brakeTween.Kill(); // rip
        }
        brakeTween = CreateTween();
        brakeTween.TweenProperty(this, "Speed", MaxSpeed, 2.5f)
            .SetEase(Tween.EaseType.Out)
            .SetTrans(Tween.TransitionType.Quad);
        braked = false;
    }

    public override void _Process(double delta)
    {
        var inMovingAnimation = sprite.Animation.ToString().EndsWith("move");
        if (IsMoving() && !inMovingAnimation)
        {
            sprite.Animation = movingAnimationMap[Direction];
        }
        else if (!IsMoving() && inMovingAnimation)
        {
            sprite.Animation = stillAnimationMap[Direction];
        }

        if (pathFollow.ProgressRatio < 1f)
        {
            pathFollow.Progress += Speed;
            return;
        }

        EmitSignal(SignalName.FinishedPath);
    }

    public bool IsMoving()
    {
        return Speed >= 0.2f;
    }

    public Vector2 GetTrainPosition()
    {
        return pathFollow.Position;
    }
}