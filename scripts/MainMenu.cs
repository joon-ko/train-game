using System;
using Godot;

public partial class MainMenu : Control
{
    private Train train;

    private GridManager gridManager;
    private TileMapLayer groundLayer;
    private TileMapLayer envLayer;
    private TileMapLayer switchLayer;

    private Control trainPathVisualizer;

    private RichTextLabel brakeInfoLabel;
    private RichTextLabel switchInfoLabel;

    private bool madeInitialAssignment = false;
    private bool scheduled = false;

    private AnimationManager animationManager;
    private SwitchManager switchManager;

    private RichTextLabel titleCard;

    private AudioStreamPlayer choochooPlayerOne;
    private AudioStreamPlayer choochooPlayerTwo;

    public override void _Ready()
    {
        animationManager = GetNode<AnimationManager>("/root/AnimationManager");
        switchManager = GetNode<SwitchManager>("/root/SwitchManager");

        train = GetNode<Train>("Train");
        train.Head.FinishedPath += _OnFinishedPath;

        gridManager = GetNode<GridManager>("GridManager");
        groundLayer = GetNode<TileMapLayer>("GridManager/Ground");
        envLayer = GetNode<TileMapLayer>("GridManager/Environment");
        switchLayer = GetNode<TileMapLayer>("SwitchArrowLayer/SwitchArrows");

        trainPathVisualizer = GetNode<Control>("TrainPathVisualizer");

        titleCard = GetNode<RichTextLabel>("UILayer/MarginContainer/TitleCard/RichTextLabel");
        animationManager.AddBobAnimation(titleCard);

        brakeInfoLabel = GetNode<RichTextLabel>("UILayer/MarginContainer/VBoxContainer/BrakeInfoLabel");
        switchInfoLabel = GetNode<RichTextLabel>("UILayer/MarginContainer/VBoxContainer/SwitchInfoLabel");

        choochooPlayerOne = GetNode<AudioStreamPlayer>("Sounds/Choochoo1");
        choochooPlayerTwo = GetNode<AudioStreamPlayer>("Sounds/Choochoo2");

        ClearFollowTraincarPositions();
    }

    private void _AssignTrainPath()
    {
        var coordinate = madeInitialAssignment ? groundLayer.LocalToMap(train.Head.GetTrainPosition()) : train.StartCoordinate;
        if (!madeInitialAssignment)
        {
            madeInitialAssignment = true;
        }

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

    private void ClearFollowTraincarPositions()
    {
        // A series of hacks, but hacks are the essence of jams
        train.Middle.currentPath.Curve.ClearPoints();
        train.Middle.previousPath.Curve.ClearPoints();
        train.Tail.currentPath.Curve.ClearPoints();
        train.Tail.previousPath.Curve.ClearPoints();

        train.Middle.currentPathFollow.Position = new Vector2(-50, 50);
        train.Middle.previousPathFollow.Position = new Vector2(-50, 50);
        train.Tail.currentPathFollow.Position = new Vector2(-50, 50);
        train.Tail.previousPathFollow.Position = new Vector2(-50, 50);
    }


    private void _OnFinishedPath()
    {
        scheduled = false;
    }

    private void RenderSwitchLayer()
    {
        var switchOne = switchManager.GetSwitchCoord(0);
        if (switchManager.GetSwitchOrientation(switchOne) == SwitchOrientation.Straight)
        {
            switchLayer.SetCell(switchOne, 0, TileManager.GetTileAtlasCoordinate(Tile.StraightArrow));
        }
        else
        {
            switchLayer.SetCell(switchOne, 0, TileManager.GetTileAtlasCoordinate(Tile.BentArrow));
        }

        var switchTwo = switchManager.GetSwitchCoord(1);
        if (switchManager.GetSwitchOrientation(switchTwo) == SwitchOrientation.Straight)
        {
            switchLayer.SetCell(switchTwo, 0, TileManager.GetTileAtlasCoordinate(Tile.StraightArrow));
        }
        else
        {
            switchLayer.SetCell(switchTwo, 0, TileManager.GetTileAtlasCoordinate(Tile.BentArrow));
        }

        var switchThree = switchManager.GetSwitchCoord(2);
        if (switchManager.GetSwitchOrientation(switchThree) == SwitchOrientation.Straight)
        {
            switchLayer.SetCell(switchThree, 0, TileManager.GetTileAtlasCoordinate(Tile.StraightArrow));
        }
        else
        {
            switchLayer.SetCell(switchThree, 0, TileManager.GetTileAtlasCoordinate(Tile.BentArrow));
        }
    }

    public override void _Process(double delta)
    {
        if (!scheduled)
        {
            _AssignTrainPath();
        }

        RenderSwitchLayer();
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
            if (keyEvent.Keycode == Key.Z && keyEvent.Pressed)
            {
                switchManager.ToggleSwitch(0);
                return;
            }
            if (keyEvent.Keycode == Key.X && keyEvent.Pressed)
            {
                switchManager.ToggleSwitch(1);
                return;
            }
            if (keyEvent.Keycode == Key.C && keyEvent.Pressed)
            {
                switchManager.ToggleSwitch(2);
                return;
            }
            if (keyEvent.Keycode == Key.Tab && keyEvent.Pressed)
            {
                trainPathVisualizer.Visible = !trainPathVisualizer.Visible;
                return;
            }
            if (keyEvent.Keycode == Key.Shift && keyEvent.Pressed)
            {
                // Sound the train horn, choo choo!
                var randomBit = Random.Shared.Next(2);
                var player = randomBit == 0 ? choochooPlayerOne : choochooPlayerTwo;
                player.Play();
            }
        }
    }
}