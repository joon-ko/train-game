using Godot;

public partial class Root : Control
{
    private Train testTrain;
    private GridManager gridManager;
    private TileMapLayer grid;
    private TileMapLayer gridEnv;
    private RichTextLabel brakeInfoLabel;
    private Control trainPathVisualizer;

    private bool scheduled;

    private AnimationManager animationManager;
    private SwitchManager switchManager;

    public override void _Ready()
    {
        animationManager = GetNode<AnimationManager>("/root/AnimationManager");
        switchManager = GetNode<SwitchManager>("/root/SwitchManager");

        testTrain = GetNode<Train>("Train");
        testTrain.FinishedPath += _OnFinishedPath;

        gridManager = GetNode<GridManager>("GridManager");
        grid = GetNode<TileMapLayer>("GridManager/Ground");
        gridEnv = GetNode<TileMapLayer>("GridManager/Environment");

        brakeInfoLabel = GetNode<RichTextLabel>("UIContainer/BrakeInfoLabel");
        brakeInfoLabel.PivotOffset = brakeInfoLabel.Size / 2f;
        animationManager.AddSwayAnimation(brakeInfoLabel);
        animationManager.AddBlinkAnimation(brakeInfoLabel);

        trainPathVisualizer = GetNode<Control>("TrainPathVisualizer");

        grid.SetCell(new Vector2I(11,9), 0, new Vector2I(0,6));
    }

    private void _AssignTrainPath()
    {
        var coordinate = grid.LocalToMap(testTrain.GetTrainPosition());
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

        testTrain.AcceptPath(path);
        scheduled = true;
    }

    private void _OnFinishedPath()
    {
        scheduled = false;
    }

    public override void _Process(double delta)
    {
        if (!scheduled)
        {
            _AssignTrainPath();
        }

        // hard coded switch coords for now
        if (switchManager.GetSwitchOrientation(new Vector2I(23, 0)) == SwitchOrientation.Straight) 
        {
            gridEnv.SetCell(new Vector2I(23,0), 0, new Vector2I(3,8));
        }
        else
        {
            gridEnv.SetCell(new Vector2I(23,0), 0, new Vector2I(3,9));
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey)
        {
            var keyEvent = (InputEventKey)@event;
            if (keyEvent.Keycode == Key.Space && keyEvent.Pressed)
            {
                testTrain.ToggleBrake();
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
        }
    }
}