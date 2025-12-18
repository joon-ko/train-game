using Godot;

public partial class Root : Control
{
    private Train testTrain;

    private GridManager gridManager;
    private TileMapLayer groundLayer;
    private TileMapLayer envLayer;
    private TileMapLayer switchLayer;

    private RichTextLabel brakeInfoLabel;
    private RichTextLabel timeRemainingLabel;
    private RichTextLabel speedLabel;
    private RichTextLabel accuracyLabel;
    private RichTextLabel gameEndLabel;

    private Control trainPathVisualizer;

    private LevelState levelState;
    private CargoPanel cargoPanel;

    private bool scheduled = false;

    private AnimationManager animationManager;
    private SwitchManager switchManager;

    private PackedScene platformScene;
    private Node2D platforms;

    private float TimeRemaining = 120f;


    public override void _Ready()
    {
        animationManager = GetNode<AnimationManager>("/root/AnimationManager");
        switchManager = GetNode<SwitchManager>("/root/SwitchManager");

        testTrain = GetNode<Train>("Train");
        testTrain.Head.FinishedPath += _OnFinishedPath;

        gridManager = GetNode<GridManager>("GridManager");
        groundLayer = GetNode<TileMapLayer>("GridManager/Ground");
        envLayer = GetNode<TileMapLayer>("GridManager/Environment");
        switchLayer = GetNode<TileMapLayer>("SwitchArrowLayer/SwitchArrows");

        brakeInfoLabel = GetNode<RichTextLabel>("UILayer/UIContainer/BrakeInfoLabel");
        animationManager.AddBobAnimation(brakeInfoLabel);

        timeRemainingLabel = GetNode<RichTextLabel>("UILayer/UIContainer/VBoxContainer/TimeRemainingLabel");
        timeRemainingLabel.Text = _GetTimeRemainingText();
        animationManager.AddBobAnimation(timeRemainingLabel);

        speedLabel = GetNode<RichTextLabel>("UILayer/UIContainer/VBoxContainer/SpeedLabel");
        speedLabel.Text = _GetSpeedLabelText();
        animationManager.AddBobAnimation(speedLabel);

        gameEndLabel = GetNode<RichTextLabel>("GameOverScreen/ColorRect/CenterContainer/GameEndLabel");
        gameEndLabel.Text = "Game Over";

        trainPathVisualizer = GetNode<Control>("TrainPathVisualizer");

        levelState = GetNode<LevelState>("LevelState");
        cargoPanel = GetNode<CargoPanel>("UILayer/UIContainer/CargoPanel");
        cargoPanel.PurpleCargoRequired = levelState.PurpleCargoRequired;
        cargoPanel.PinkCargoRequired = levelState.PinkCargoRequired;

        platformScene = GD.Load<PackedScene>("res://scenes/Platform.tscn");
        platforms = GetNode<Node2D>("Platforms");

        foreach (Platform platform in platforms.GetChildren())
        {
            platform.Initialize();
        }
    }

    private void _AssignTrainPath()
    {
        var coordinate = groundLayer.LocalToMap(testTrain.Head.GetTrainPosition());
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
        return $"time remaining: {Mathf.RoundToInt(Mathf.Ceil(TimeRemaining))}";
    }

    private string _GetSpeedLabelText()
    {
        var formattedSpeed = Mathf.RoundToInt(testTrain.Head.Speed * 100f);
        return $"train speed: {formattedSpeed} km/h";
    }

    private void OnRestartButtonPressed()
    {
        GetTree().ReloadCurrentScene();
        // TODO: Fix null GetTree()
        GetTree().Paused = false;
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
        
        
        if (TimeRemaining <= 0) 
        {
            GetTree().Paused = true;
            GameOverScreen GameOver = GetTree().CurrentScene.GetNode<GameOverScreen>("GameOverScreen");
            AnimationPlayer GameOverFadeIn = GetTree().CurrentScene.GetNode<AnimationPlayer>("GameOverScreen/AnimationPlayer");
            GameOverFadeIn.Play("gameOver");
            GameOverFadeIn.Advance(0);
            GameOver.Show();

        } else if (levelState.QuotaMet())
        {
            gameEndLabel.Text = "You Won!";
            GetTree().Paused = true;
            GameOverScreen GameOver = GetTree().CurrentScene.GetNode<GameOverScreen>("GameOverScreen");
            AnimationPlayer GameOverFadeIn = GetTree().CurrentScene.GetNode<AnimationPlayer>("GameOverScreen/AnimationPlayer");
            GameOverFadeIn.Play("gameOver");
            GameOverFadeIn.Advance(0);
            GameOver.Show();
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
        }
    }
}