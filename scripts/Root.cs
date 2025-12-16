using Godot;

public partial class Root : Control
{
    private Train testTrain;
    private GridManager gridManager;
    private TileMapLayer grid;
    private TileMapLayer gridEnv;
    private RichTextLabel brakeInfoLabel;
    private RichTextLabel cargoProgressLabel;
    private RichTextLabel timeRemainingLabel;
    private RichTextLabel speedLabel;
    private Control trainPathVisualizer;

    private bool scheduled = false;

    private AnimationManager animationManager;
    private SwitchManager switchManager;

    private PackedScene platformScene;
    private Node2D platforms;

    private int CargoDelivered = 0;
    private int TargetCargoDelivered = 100;
    private float TimeRemaining = 120f;

    public override void _Ready()
    {
        animationManager = GetNode<AnimationManager>("/root/AnimationManager");
        switchManager = GetNode<SwitchManager>("/root/SwitchManager");

        testTrain = GetNode<Train>("Train");
        testTrain.Head.FinishedPath += _OnFinishedPath;

        gridManager = GetNode<GridManager>("GridManager");
        grid = GetNode<TileMapLayer>("GridManager/Ground");
        gridEnv = GetNode<TileMapLayer>("GridManager/Environment");

        brakeInfoLabel = GetNode<RichTextLabel>("UIContainer/BrakeInfoLabel");
        brakeInfoLabel.PivotOffset = brakeInfoLabel.Size / 2f;
        animationManager.AddSwayAnimation(brakeInfoLabel);
        animationManager.AddBlinkAnimation(brakeInfoLabel);

        cargoProgressLabel = GetNode<RichTextLabel>("UIContainer/VBoxContainer/CargoProgressLabel");
        cargoProgressLabel.Text = _GetCargoProgressText();

        timeRemainingLabel = GetNode<RichTextLabel>("UIContainer/VBoxContainer/TimeRemainingLabel");
        timeRemainingLabel.Text = _GetTimeRemainingText();

        speedLabel = GetNode<RichTextLabel>("UIContainer/VBoxContainer/SpeedLabel");
        speedLabel.Text = _GetSpeedLabelText();

        trainPathVisualizer = GetNode<Control>("TrainPathVisualizer");

        platformScene = GD.Load<PackedScene>("res://scenes/Platform.tscn");
        platforms = GetNode<Node2D>("Platforms");
        
        foreach (Platform platform in platforms.GetChildren())
        {
            platform.Initialize();
        }
        // PathInfo = GetPathFromTarget(TrainTargetLocation);

        // ProgressRatio = (TrainTargetLocation.X - PathInfo.StartCoordinate.X) / (PathInfo.EndCoordinate.X - PathInfo.StartCoordinate.X);

    }

    private void _AssignTrainPath()
    {
        var coordinate = grid.LocalToMap(testTrain.Head.GetTrainPosition());
        if (!gridManager.TrainPaths.ContainsKey(coordinate))
        {
            return;
        }

        PathInfo path;
        var paths = gridManager.TrainPaths[coordinate];
        if (paths.Count > 1)
        {
            // We're at a switch: check the orientation of this switch to determine the right path
            var orientation = switchManager.GetSwitchOrientation(coordinate);
            path = paths.Find((path) => path.SwitchOrientation == orientation);
        }
        else
        {
            path = paths[0];
        }

        testTrain.Head.AcceptPath(path);
        testTrain.Middle.AcceptPath(path);
        testTrain.Tail.AcceptPath(path);
        scheduled = true;
    }

    private void _OnFinishedPath()
    {
        scheduled = false;
    }

    private string _GetCargoProgressText()
    {
        return $"cargo delivered: {CargoDelivered}/{TargetCargoDelivered}";
    }

    private string _GetTimeRemainingText()
    {
        return $"time remaining: {Mathf.RoundToInt(Mathf.Ceil(TimeRemaining))}";
    }

    private string _GetSpeedLabelText()
    {
        var formattedSpeed = Mathf.RoundToInt(testTrain.Head.Speed * 100f);
        return $"train speed: {formattedSpeed} km/h";
    }

    public override void _Process(double delta)
    {
        if (!scheduled)
        {
            _AssignTrainPath();
        }

        var switchCoord = switchManager.GetSwitchCoord(0);
        if (switchManager.GetSwitchOrientation(switchCoord) == SwitchOrientation.Straight)
        {
            gridEnv.SetCell(switchCoord, 0, TileManager.GetTileAtlasCoordinate(Tile.StraightArrow));
        }
        else
        {
            gridEnv.SetCell(switchCoord, 0, TileManager.GetTileAtlasCoordinate(Tile.BentArrow));
        }

        cargoProgressLabel.Text = _GetCargoProgressText();

        TimeRemaining -= (float)delta;
        timeRemainingLabel.Text = _GetTimeRemainingText();

        speedLabel.Text = _GetSpeedLabelText();
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey)
        {
            var keyEvent = (InputEventKey)@event;
            if (keyEvent.Keycode == Key.Space && keyEvent.Pressed)
            {
                testTrain.Head.ToggleBrake();
                return;
            }
            if (keyEvent.Keycode == Key.Shift && keyEvent.Pressed)
            {
                switchManager.ToggleSwitch(0);
                return;
            }
            if (keyEvent.Keycode == Key.Tab && keyEvent.Pressed)
            {
                trainPathVisualizer.Visible = !trainPathVisualizer.Visible;
            }
            if (keyEvent.Keycode == Key.C && keyEvent.Pressed)
            {
                CargoDelivered += 1;
            }
        }
    }
}