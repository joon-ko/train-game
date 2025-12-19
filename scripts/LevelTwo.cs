using System;
using Godot;

public partial class LevelTwo : Control
{
    private Train testTrain;

    private GridManager gridManager;
    private TileMapLayer groundLayer;
    private TileMapLayer envLayer;
    private TileMapLayer switchLayer;

    private RichTextLabel timeRemainingLabel;
    private RichTextLabel speedLabel;
    private RichTextLabel accuracyLabel;
    private RichTextLabel gameEndLabel;

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

    private PanelContainer gameOverPanel;
    private PanelContainer gameWinPanel;

    private const float MAX_TIME_REMAINING = 15;
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

        testTrain = GetNode<Train>("Train");
        testTrain.Head.FinishedPath += _OnFinishedPath;

        timeRemainingLabel = GetNode<RichTextLabel>("UILayer/UIContainer/VBoxContainer/TimeRemainingLabel");
        timeRemainingLabel.Text = _GetTimeRemainingText();

        speedLabel = GetNode<RichTextLabel>("UILayer/UIContainer/VBoxContainer/SpeedLabel");
        speedLabel.Text = _GetSpeedLabelText();

        gameEndLabel = GetNode<RichTextLabel>("GameOverScreen/ColorRect/CenterContainer/GameEndLabel");
        gameEndLabel.Text = "Game Over";

        trainPathVisualizer = GetNode<Control>("TrainPathVisualizer");

        levelState = GetNode<LevelState>("LevelState");
        cargoPanel = GetNode<CargoPanel>("UILayer/UIContainer/CargoPanel");
        cargoPanel.PurpleCargoRequired = levelState.PurpleCargoRequired;
        cargoPanel.PinkCargoRequired = levelState.PinkCargoRequired;

        platforms = GetNode<Node2D>("Platforms");

        foreach (Platform platform in platforms.GetChildren())
        {
            platform.Initialize();
        }

        choochooPlayerOne = GetNode<AudioStreamPlayer>("Sounds/Choochoo1");
        choochooPlayerTwo = GetNode<AudioStreamPlayer>("Sounds/Choochoo2");

        gameOverPanel = GetNode<PanelContainer>("UILayer/UIContainer/GameOverPanel");
        gameWinPanel = GetNode<PanelContainer>("UILayer/UIContainer/GameWinPanel");

        mainMenuScene = GD.Load<PackedScene>("res://scenes/MainMenu.tscn");
    }

    private void _AssignTrainPath()
    {
        var coordinate = madeInitialAssignment ? groundLayer.LocalToMap(testTrain.Head.GetTrainPosition()) : testTrain.StartCoordinate;
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

        testTrain.Head.AcceptPath(path);
        testTrain.Middle.AcceptPath(path);
        testTrain.Tail.AcceptPath(path);
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
        var formattedSpeed = Mathf.RoundToInt(testTrain.Head.Speed * 100f);
        return $"train speed: {formattedSpeed} km/h";
    }

    private void RenderSwitchLayer()
    {
        var switchCoord = switchManager.GetSwitchCoord(0);
        if (switchManager.GetSwitchOrientation(switchCoord) == SwitchOrientation.Straight)
        {
            switchLayer.SetCell(switchCoord, 0, TileManager.GetTileAtlasCoordinate(Tile.StraightArrow));
        }
        else
        {
            switchLayer.SetCell(switchCoord, 0, TileManager.GetTileAtlasCoordinate(Tile.BentArrow));
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
            levelOver = true;
            gameOverPanel.Visible = true;
        }
        else if (levelState.QuotaMet() && !levelOver)
        {
            levelOver = true;
            gameWinPanel.Visible = true;
        }
        else
        {
            TimeRemaining -= (float)delta;
            timeRemainingLabel.Text = _GetTimeRemainingText();
        }

        speedLabel.Text = _GetSpeedLabelText();

        cargoPanel.PinkCargoDelivered = levelState.PinkCargoDelivered;
        cargoPanel.PurpleCargoDelivered = levelState.PurpleCargoDelivered;
    }

    public void RestartLevel()
    {
        GD.Print("Restarting level");
        cargoPanel.PurpleCargoRequired = levelState.PurpleCargoRequired;
        cargoPanel.PinkCargoRequired = levelState.PinkCargoRequired;
        madeInitialAssignment = false;
        scheduled = false;
        gameOverPanel.Visible = false;
        testTrain.CarriedCargo = CargoType.None;
        testTrain.CargoCount = 0;
        TimeRemaining = MAX_TIME_REMAINING;
        levelOver = false;
    }

    public void ReturnToMainMenu()
    {
        GD.Print("Returning to main menu");
        GetTree().ChangeSceneToPacked(mainMenuScene);
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