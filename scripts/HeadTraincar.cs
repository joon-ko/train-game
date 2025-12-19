using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

[GlobalClass]
public partial class HeadTraincar : Traincar
{
	[Signal] public delegate void FinishedPathEventHandler();

	public Vector2I StartCoordinate;
	public float MaxSpeed { get; set; } = 0.3f;

	private PackedScene accuracyPopupScene;

	private Tween brakeTween;
	private Tween unbrakeTween;
	private LevelState levelState;

	private CanvasLayer uiLayer;

	private Label cargoCountLabel;

	private bool braked = false;

	private float MAX_SCORE_DISTANCE = 54f;

	private GridManager gridManager;

	private bool checkedForAction = false;

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
		StartCoordinate = train.StartCoordinate;
		MaxSpeed = train.MaxSpeed;

		accuracyPopupScene = GD.Load<PackedScene>("res://scenes/AccuracyPopup.tscn");

		currentPathFollow.Position = tileMapLayer.MapToLocal(StartCoordinate);
		Speed = MaxSpeed;

		cargoCountLabel = train.GetNode<Label>("CanvasLayer/CargoCount");
		levelState = GetTree().CurrentScene.GetNode<LevelState>("LevelState");

		uiLayer = GetTree().CurrentScene.GetNode<CanvasLayer>("UILayer");
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

		GetTree().CreateTimer(0.5f).Timeout += _ReenableActionCheck;
	}

	private void _ReenableActionCheck()
	{
		checkedForAction = false;
	}

	public override void _Process(double delta)
	{
		UpdateCargoCountLabel();
	}

	private void UpdateCargoCountLabel()
	{
		cargoCountLabel.Position = train.Head.GetTrainPosition() + new Vector2(-12, -38);
		cargoCountLabel.Visible = train.CargoCount != 0;
		cargoCountLabel.Text = $"{train.CargoCount}";
	}

	public override void _PhysicsProcess(double delta)
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

		if (IsStopped() && !checkedForAction)
		{
			CheckForPlatformAction();
			checkedForAction = true;
		}

		if (currentPathFollow.ProgressRatio < 1f)
		{
			currentPathFollow.Progress += Speed;
			return;
		}

		EmitSignal(SignalName.FinishedPath);
	}

	private AccuracyGrade CalculateGradeForAccuracy(float accuracy)
	{
		if (accuracy > 95f)
		{
			return AccuracyGrade.Perfect;
		}
		if (accuracy > 70f)
		{
			return AccuracyGrade.Good;
		}
		if (accuracy > 20f)
		{
			return AccuracyGrade.OK;
		}
		return AccuracyGrade.Miss;

	}

	private void HandleDistanceCheckForPlatform(Platform platform, float distance)
	{
		if (distance < MAX_SCORE_DISTANCE)
		{
			var accuracy = (MAX_SCORE_DISTANCE - distance) / MAX_SCORE_DISTANCE * 100f;
			var grade = CalculateGradeForAccuracy(accuracy);
			GD.Print($"Raw accuracy: {accuracy}");

			// If the accuracy grade is a miss, just do nothing.
			if (grade == AccuracyGrade.Miss)
			{
				return;
			}

			// If the platform is a pickup platform:
			if (platform.PlatformType == PlatformType.Pickup)
			{
				// The train should not have existing cargo. If it does, do nothing.
				if (train.CargoCount > 0 || train.CarriedCargo != CargoType.None)
				{
					return;
				}

				// Award cargo to the train.
				train.CarriedCargo = platform.CargoType;
				train.CargoCount = CargoUtils.GetAwardedCargoForGrade(grade);

				// Instantiate a popup of the accuracy grade.
				var accuracyPopup = accuracyPopupScene.Instantiate<AccuracyPopup>();
				accuracyPopup.Grade = grade;
				accuracyPopup.Position = GetTrainPosition() + new Vector2(-36, -12);
				uiLayer.AddChild(accuracyPopup);

				return;
			}

			// If the platform is a delivery platform:
			if (platform.PlatformType == PlatformType.Delivery)
			{
				// The train should have existing cargo. If it does not, do nothing.
				if (train.CargoCount == 0 || train.CarriedCargo == CargoType.None)
				{
					return;
				}

				// Deliver the cargo, adding it to the total score.
				var deliveredCargoCount = CargoUtils.GetDeliveredCargoForGrade(grade, train.CargoCount);
				if (train.CarriedCargo == CargoType.Purple)
				{
					levelState.PurpleCargoDelivered += deliveredCargoCount;
				}
				else if (train.CarriedCargo == CargoType.Pink)
				{
					levelState.PinkCargoDelivered += deliveredCargoCount;
				}
				train.CarriedCargo = CargoType.None;
				train.CargoCount = 0;

				// Instantiate a popup of the accuracy grade.
				// Instantiate a popup of the accuracy grade.
				var accuracyPopup = accuracyPopupScene.Instantiate<AccuracyPopup>();
				accuracyPopup.Grade = grade;
				accuracyPopup.GlobalPosition = GetTrainPosition() + new Vector2(0, 32);
				uiLayer.AddChild(accuracyPopup);

				return;
			}
		}
	}

	private void CheckForPlatformAction()
	{
		var platforms = GetTree().GetNodesInGroup("Platforms");
		foreach (Platform platform in platforms.Cast<Platform>())
		{
			var targetProgress = platform.ProgressRatio * currentPath.Curve.GetBakedLength();
		if (currentPathInfo.Equals(platform.PathInfo))
			{

				var distance = Math.Abs(targetProgress - currentPathFollow.Progress);
				HandleDistanceCheckForPlatform(platform, distance);
			}
			else if (previousPathInfo.Equals(platform.PathInfo))
			{
				// from start of current path to the end of train
				var distance = currentPathFollow.Progress + Math.Abs(previousPathInfo.EndCoordinate.X - targetProgress);
				HandleDistanceCheckForPlatform(platform, distance);
			}
		}
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