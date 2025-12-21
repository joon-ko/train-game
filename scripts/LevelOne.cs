using Godot;
using System;

public partial class LevelOne : Control
{
    private Train train;

    private GridManager gridManager;
    private TileMapLayer groundLayer;
    private TileMapLayer envLayer;
    private TileMapLayer switchLayer;

    private RichTextLabel timeRemainingLabel;
    private RichTextLabel speedLabel;
    private RichTextLabel accuracyLabel;

    private Control trainPathVisualizer;

    private LevelState levelState;
    private CargoPanel cargoPanel;

    private bool madeInitialAssignment = false;
    private bool scheduled = false;

    private AnimationManager animationManager;
    private SwitchManager switchManager;

    private Node2D platforms;

    private AudioStreamPlayer choochooPlayerOne;
    private AudioStreamPlayer choochooPlayerTwo;
    private AudioStreamPlayer winPlayer;
    private AudioStreamPlayer losePlayer;
    private AudioStreamPlayer bgmPlayer;

    private PanelContainer gameOverPanel;
    private PanelContainer gameWinPanel;

    private const float MAX_TIME_REMAINING = 123;
    private float TimeRemaining = MAX_TIME_REMAINING;

    private bool levelOver = false;

    private PackedScene mainMenuScene;

    public override void _Ready()
    {
        animationManager = GetNode<AnimationManager>("/root/AnimationManager");
        switchManager = GetNode<SwitchManager>("/root/SwitchManager");

        gridManager = GetNode<GridManager>("GridManager");
        groundLayer = GetNode<TileMapLayer>("GridManager/Ground");
        envLayer = GetNode<TileMapLayer>("GridManager/Environment");
        switchLayer = GetNode<TileMapLayer>("SwitchArrowLayer/SwitchArrows");

        train = GetNode<Train>("Train");
        train.Head.FinishedPath += _OnFinishedPath;

        ClearFollowTraincarPositions();

        timeRemainingLabel = GetNode<RichTextLabel>("UILayer/UIContainer/VBoxContainer/TimeRemainingLabel");
        timeRemainingLabel.Text = _GetTimeRemainingText();

        speedLabel = GetNode<RichTextLabel>("UILayer/UIContainer/VBoxContainer/SpeedLabel");
        speedLabel.Text = _GetSpeedLabelText();

        trainPathVisualizer = GetNode<Control>("TrainPathVisualizer");

        levelState = GetNode<LevelState>("LevelState");
        cargoPanel = GetNode<CargoPanel>("UILayer/UIContainer/CargoPanel");
        cargoPanel.PurpleCargoRequired = levelState.PurpleCargoRequired;

        platforms = GetNode<Node2D>("Platforms");

        foreach (Platform platform in platforms.GetChildren())
        {
            platform.Initialize();
        }

        choochooPlayerOne = GetNode<AudioStreamPlayer>("Sounds/Choochoo1");
        choochooPlayerTwo = GetNode<AudioStreamPlayer>("Sounds/Choochoo2");
        winPlayer = GetNode<AudioStreamPlayer>("Sounds/Win");
        losePlayer = GetNode<AudioStreamPlayer>("Sounds/Lose");
        bgmPlayer = GetNode<AudioStreamPlayer>("Sounds/BGM");

        gameOverPanel = GetNode<PanelContainer>("UILayer/UIContainer/GameOverPanel");
        gameWinPanel = GetNode<PanelContainer>("UILayer/UIContainer/GameWinPanel");

        mainMenuScene = GD.Load<PackedScene>("res://scenes/MainMenu.tscn");
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

    private void _OnFinishedPath()
    {
        scheduled = false;
    }

    private string _GetTimeRemainingText()
    {
        return $"time remaining: {Mathf.Max(0, Mathf.RoundToInt(Mathf.Ceil(TimeRemaining)))}";
    }

    private string _GetSpeedLabelText()
    {
        var formattedSpeed = Mathf.RoundToInt(train.Head.Speed * 100f);
        return $"train speed: {formattedSpeed} km/h";
    }

    private void RenderSwitchLayer()
    {
        var switchCoord = switchManager.GetSwitchCoord(0);
        if (switchManager.GetSwitchOrientation(switchCoord) == SwitchOrientation.Straight)
        {
            switchLayer.SetCell(switchCoord, 0, TileManager.GetTileAtlasCoordinate(Tile.StraightArrowNegY));
        }
        else
        {
            switchLayer.SetCell(switchCoord, 0, TileManager.GetTileAtlasCoordinate(Tile.BentArrowNegYToNegX));
        }
    }

    public override void _Process(double delta)
    {
        if (!scheduled)
        {
            _AssignTrainPath();
        }

        RenderSwitchLayer();

        if (TimeRemaining <= 0 && !levelOver)
        {
            bgmPlayer.Stop();
            GetTree().CreateTimer(7f).Timeout += () => bgmPlayer.Play();

            losePlayer.Play();
            levelOver = true;
            gameOverPanel.Visible = true;
        }
        else if (levelState.QuotaMet() && !levelOver)
        {
            bgmPlayer.Stop();
            GetTree().CreateTimer(8f).Timeout += () => bgmPlayer.Play();

            winPlayer.Play();
            levelOver = true;
            gameWinPanel.Visible = true;
        }
        else
        {
            TimeRemaining -= (float)delta;
            timeRemainingLabel.Text = _GetTimeRemainingText();
        }

        speedLabel.Text = _GetSpeedLabelText();

        cargoPanel.PurpleCargoDelivered = levelState.PurpleCargoDelivered;
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

    public void RestartLevel()
    {
        GD.Print("Restarting level");
        ClearFollowTraincarPositions();

        cargoPanel.PurpleCargoRequired = levelState.PurpleCargoRequired;
        levelState.PurpleCargoDelivered = 0;
        madeInitialAssignment = false;
        scheduled = false;
        gameOverPanel.Visible = false;
        gameWinPanel.Visible = false;
        train.CarriedCargo = CargoType.None;
        train.CargoCount = 0;
        TimeRemaining = MAX_TIME_REMAINING;
        levelOver = false;

        // If the train is stopped, start the train up again so the player doesn't have to manually start it up
        if (train.Head.IsBraked())
        {
            train.Head.Unbrake();
        }
    }

    public void ReturnToMainMenu()
    {
        GD.Print("Returning to main menu");
        ClearFollowTraincarPositions();
        GetTree().ChangeSceneToPacked(mainMenuScene);
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
                return;
            }
            if (keyEvent.Keycode == Key.R && keyEvent.Pressed)
            {
                RestartLevel();
                return;
            }
            if (keyEvent.Keycode == Key.Q && keyEvent.Pressed)
            {
                ReturnToMainMenu();
                return;
            }
        }
    }
}
