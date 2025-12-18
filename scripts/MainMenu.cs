using Godot;

public partial class MainMenu : Control
{
    private Train train;

    private GridManager gridManager;
    private TileMapLayer groundLayer;
    private TileMapLayer envLayer;
    // private TileMapLayer switchLayer;

    private Control trainPathVisualizer;

    private RichTextLabel brakeInfoLabel;

    private bool scheduled = false;

    private AnimationManager animationManager;
    private SwitchManager switchManager;

    private RichTextLabel titleCard;

    public override void _Ready()
    {
        animationManager = GetNode<AnimationManager>("/root/AnimationManager");
        switchManager = GetNode<SwitchManager>("/root/SwitchManager");

        train = GetNode<Train>("Train");
        train.Head.FinishedPath += _OnFinishedPath;

        gridManager = GetNode<GridManager>("GridManager");
        groundLayer = GetNode<TileMapLayer>("GridManager/Ground");
        envLayer = GetNode<TileMapLayer>("GridManager/Environment");
        // switchLayer = GetNode<TileMapLayer>("SwitchArrowLayer/SwitchArrows");

        trainPathVisualizer = GetNode<Control>("TrainPathVisualizer");

        titleCard = GetNode<RichTextLabel>("UILayer/MarginContainer/TitleCard/RichTextLabel");
        animationManager.AddBobAnimation(titleCard);

        brakeInfoLabel = GetNode<RichTextLabel>("UILayer/MarginContainer/BrakeInfoLabel");
        animationManager.AddBobAnimation(brakeInfoLabel);
    }

    private void _AssignTrainPath()
    {
        var coordinate = groundLayer.LocalToMap(train.Head.GetTrainPosition());

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

        train.Head.AcceptPath(path);
        train.Middle.AcceptPath(path);
        train.Tail.AcceptPath(path);
        scheduled = true;
    }

    private void _OnFinishedPath()
    {
        scheduled = false;
    }

    // private void RenderSwitchLayer()
    // {
    //     var switchCoord = switchManager.GetSwitchCoord(0);
    //     if (switchManager.GetSwitchOrientation(switchCoord) == SwitchOrientation.Straight)
    //     {
    //         switchLayer.SetCell(switchCoord, 0, TileManager.GetTileAtlasCoordinate(Tile.StraightArrow));
    //     }
    //     else
    //     {
    //         switchLayer.SetCell(switchCoord, 0, TileManager.GetTileAtlasCoordinate(Tile.BentArrow));
    //     }
    // }

    public override void _Process(double delta)
    {
        if (!scheduled)
        {
            _AssignTrainPath();
        }

        // RenderSwitchLayer();
    }


    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey)
        {
            var keyEvent = (InputEventKey)@event;
            if (keyEvent.Keycode == Key.Space && keyEvent.Pressed)
            {
                train.Head.ToggleBrake();
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