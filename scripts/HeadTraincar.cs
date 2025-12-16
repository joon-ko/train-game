using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

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

	private GridManager gridManager;
	private bool checkedScore = false;
	public float Accuracy = 0f;

	public override void _Ready()
	{
		base._Ready();

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

	public override void _PhysicsProcess(double delta)
	{
		var inMovingAnimation = currentSprite.Animation.ToString().EndsWith("move");
		if (IsMoving() && !inMovingAnimation)
		{
			currentSprite.Animation = movingAnimationMap[Direction];
			checkedScore = false;
			Accuracy = 0;
		}
		else if (!IsMoving() && inMovingAnimation)
		{
			currentSprite.Animation = stillAnimationMap[Direction];
		}
		if (IsStopped() && !checkedScore) {
			Accuracy = CalculateAccuracy();
			checkedScore = true;
		}

		if (currentPathFollow.ProgressRatio < 1f)
		{
			currentPathFollow.Progress += Speed;
			return;
		}

		EmitSignal(SignalName.FinishedPath);
	}



	private float CalculateAccuracy() {
		var platforms = GetTree().GetNodesInGroup("Platforms");
		foreach (Platform platform in platforms.Cast<Platform>()) 
		{
			var targetProgress = platform.ProgressRatio * currentPath.Curve.GetBakedLength();
			if (currentPathInfo.Equals(platform.PathInfo)) 
			{
				var distance = Math.Abs(targetProgress - currentPathFollow.Progress);
				if (distance < 54)
				{
					GD.Print(distance);
					return (54-distance)/54*100;
				}
			} else if (previousPathInfo.Equals(platform.PathInfo))
			{	
				// from start of current path to the end of train
				var distance = currentPathFollow.Progress + Math.Abs(previousPathInfo.EndCoordinate.X - targetProgress);
				if (distance < 54)
				{
					GD.Print(distance);
					return (54-distance)/54*100;
				}
			}
		}
		return 0;
	}

	public bool IsMoving()
	{
		return Speed >= 0.2f;
	}

	public bool IsStopped()
	{
		return Speed == 0;
	}
}