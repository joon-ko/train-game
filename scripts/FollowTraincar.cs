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

	private Dictionary<Direction, string> animationMap = new Dictionary<Direction, string>()
	{
		{ Direction.PosX, "x" },
		{ Direction.PosY, "y" },
		{ Direction.NegX, "x" },
		{ Direction.NegY, "y" }
	};

    private Dictionary<CargoType, int> animFrameForCargoType = new Dictionary<CargoType, int>()
    {
        { CargoType.Purple, 0 },
        { CargoType.None, 1 },
        { CargoType.Star, 2 },
        { CargoType.Pink, 3 },
    };

    public override void _Ready()
    {
        base._Ready();

        currentPathFollow.Position = tileMapLayer.MapToLocal(InitialCoordinate);

        currentSprite.Frame = animFrameForCargoType[train.CarriedCargo];
        previousSprite.Frame = animFrameForCargoType[train.CarriedCargo];
    }

    public void AcceptPath(PathInfo pathInfo)
    {
        PreviousDirection = Direction;
        Direction = pathInfo.Direction;

        // Set the orientation of the train
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
        currentSprite.Animation = animationMap[Direction];
        previousSprite.Animation = animationMap[PreviousDirection];

        currentSprite.Frame = animFrameForCargoType[train.CarriedCargo];
        previousSprite.Frame = animFrameForCargoType[train.CarriedCargo];

        if (Head.currentPathFollow.Progress < Separation)
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
