using Godot;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
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

	private GridManager gridManager;
	private bool checkedScore = false;


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
		if (IsStopped() && !checkedScore) {
			var platforms = GetTree().GetNodesInGroup("Platforms");
			GD.Print("platforms: ", platforms);
			var targetLocations = new Vector2I[platforms.Count];
			var targetTileLocations = new Vector2[platforms.Count];
			for (int i = 0; i < platforms.Count; i++)
			{
				GD.Print("heyo");
				var platform = (Platform)platforms[i];
				GD.Print("plato", platform);
				targetLocations[i] = platform.TrainTargetLocation;
				targetTileLocations[i] = tileMapLayer.MapToLocal(targetLocations[i]);
				gridManager = GetTree().CurrentScene.GetNode<GridManager>("GridManager");
				var targetProgress = platform.ProgressRatio * currentPath.Curve.GetBakedLength();
				GD.Print("currentPath: ", currentPath, "platformPath", platform.PathInfo);
				if (currentPath.Equals(platform.PathInfo)) {
					GD.Print("byeo");
					// calc score
					// target's progress minus train's progress
					// get target's progress
					var trainProgress = currentPathFollow.Progress;
					var score = targetProgress - trainProgress;
					GD.Print("Score: ", score);
				} else if (previousPath.Equals(platform.PathInfo))
				{
					// from start of current path to the end of train
					var score = currentPathFollow.Progress + (previousPathInfo.EndCoordinate.X - targetProgress);
					GD.Print("Score: ", score);
				}
			}
			checkedScore = true;
			// how do i get the path based on the targetlocation?
			// take the y of the target location and look for paths that have the same y
			// then get the one with the same x or that falls between the correct xs
			// for each target check if it's on the same path as the train or the train's previous path
			// get train's current and previous path
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

	public bool IsStopped()
	{
		return Speed == 0;
	}
}