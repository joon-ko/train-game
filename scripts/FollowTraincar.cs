using Godot;
using System.Collections.Generic;

public enum FollowType
{
    Middle,
    Tail
}

[GlobalClass]
public partial class FollowTraincar : Traincar
{
	[Signal] public delegate void FinishedPathEventHandler();

    [Export] public FollowType FollowType { get; set; }
    [Export] public Traincar Head { get; set; }
    [Export] public Vector2I InitialCoordinate { get; set; }
    [Export] public int Separation { get; set; }

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

    private Dictionary<Direction, Vector2I> deltaForDirection = new Dictionary<Direction, Vector2I>()
    {
        { Direction.PosX, new Vector2I(-16, -8) },
        { Direction.NegX, new Vector2I(16, 8) },
        { Direction.PosY, new Vector2I(16, -8) },
        { Direction.NegY, new Vector2I(-16, 8) }
    };

    public override void _Ready()
    {
        base._Ready();

        currentPathFollow.Position = tileMapLayer.MapToLocal(InitialCoordinate);
    }

    public void AcceptPath(PathInfo pathInfo)
    {
        PreviousDirection = Direction;
        Direction = pathInfo.Direction;

        // Set the orientation of the train
        var animationMap = IsMoving() ? movingAnimationMap : stillAnimationMap;
        currentSprite.Animation = animationMap[pathInfo.Direction];
        previousSprite.Animation = animationMap[PreviousDirection];

        previousPath.Curve = (Curve2D)currentPath.Curve.Duplicate();

        // Set the new path for the train
        currentPath.Curve.ClearPoints();
        currentPath.Curve.AddPoint(tileMapLayer.MapToLocal(pathInfo.StartCoordinate));
        currentPath.Curve.AddPoint(tileMapLayer.MapToLocal(pathInfo.EndCoordinate));
        currentPathFollow.Progress = 0f;
    }

    public override void _Process(double delta)
    {
        var inMovingAnimation = currentSprite.Animation.ToString().EndsWith("move");
        if (IsMoving() && !inMovingAnimation)
        {
            currentSprite.Animation = movingAnimationMap[Direction];
            previousSprite.Animation = movingAnimationMap[PreviousDirection];
        }
        else if (!IsMoving() && inMovingAnimation)
        {
            currentSprite.Animation = stillAnimationMap[Direction];
            previousSprite.Animation = stillAnimationMap[PreviousDirection];
        }

        else if (Head.currentPathFollow.Progress < Separation)
        {
            currentPath.Visible = false;
            previousPath.Visible = true;
            var remainingProgress = Separation - Head.currentPathFollow.Progress;
            var previousProgress = previousPath.Curve.GetBakedLength() - remainingProgress;
            previousPathFollow.Progress = previousProgress;
        }
        else
        {
            currentPath.Visible = true;
            previousPath.Visible = false;
            currentPathFollow.Progress = Head.currentPathFollow.Progress - Separation;
        }
    }

	public bool IsMoving()
	{
		return Speed >= 0.2f;
	}
}
