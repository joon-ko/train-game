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
    private Control trainPathVisualizer;

    private bool scheduled = false;

    private AnimationManager animationManager;
    private SwitchManager switchManager;

    private PackedScene platformScene;
    private Node2D platforms;

    private int CargoDelivered = 0;
    private int TargetCargoDelivered = 100;
    private float TimeRemaining = 120f;

    // Hardcoded min/max tilemap coordinate bounds for now to scan for platforms """efficiently"""
    private const int MAP_MIN_X = 11;
	private const int MAP_MIN_Y = -8;
	private const int MAP_MAX_X = 28;
	private const int MAP_MAX_Y = 9;

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

        trainPathVisualizer = GetNode<Control>("TrainPathVisualizer");

        platformScene = GD.Load<PackedScene>("res://scenes/Platform.tscn");
        platforms = GetNode<Node2D>("Platforms");

        grid.SetCell(new Vector2I(11, 9), 0, new Vector2I(0, 6));
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