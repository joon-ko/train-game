using Godot;
using System.Collections.Generic;
using System.Threading;

[GlobalClass]
public partial class HeadTraincar : Traincar
{
	[Signal] public delegate void FinishedPathEventHandler();

	[Export] public Vector2I InitialCoordinate;
	[Export] public float MaxSpeed { get; set; } = 0.3f;

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
		base._Ready();

		// timer = GetNode<Godot.Timer>("Timer");
		currentPathFollow.Position = tileMapLayer.MapToLocal(InitialCoordinate);
		Speed = MaxSpeed;
	}

	public void AcceptPath(PathInfo pathInfo)
	{
		previousPathInfo = currentPathInfo;
		currentPathInfo = pathInfo;

		// Set the orientation of the train
		var animationMap = IsMoving() ? movingAnimationMap : stillAnimationMap;
		currentSprite.Animation = animationMap[pathInfo.Direction];

		PreviousDirection = Direction;
		Direction = pathInfo.Direction;

		previousPath.Curve = (Curve2D)currentPath.Curve.Duplicate();

		// Set the new path for the train
		currentPath.Curve.ClearPoints();
		currentPath.Curve.AddPoint(tileMapLayer.MapToLocal(pathInfo.StartCoordinate));
		currentPath.Curve.AddPoint(tileMapLayer.MapToLocal(pathInfo.EndCoordinate));
		currentPathFollow.Progress = 0f;
	}

	public void ToggleBrake()
	{

		if (braked)
		{
			_Unbrake();
			return;
		}
		// if (!cooldown)
		// {
		// 	_Brake();
		// 	cooldown = true;
		// 	timer.Start();
		// }
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
		var inMovingAnimation = currentSprite.Animation.ToString().EndsWith("move");
		if (IsMoving() && !inMovingAnimation)
		{
			currentSprite.Animation = movingAnimationMap[Direction];
		}
		else if (!IsMoving() && inMovingAnimation)
		{
			currentSprite.Animation = stillAnimationMap[Direction];
		}

		if (currentPathFollow.ProgressRatio < 1f)
		{
			currentPathFollow.Progress += Speed;
			return;
		}

		EmitSignal(SignalName.FinishedPath);
	}

	private void _OnTimerTimeout()
	{
		// cooldown = false;
	}

	public bool IsMoving()
	{
		return Speed >= 0.2f;
	}
}
