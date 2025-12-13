using Godot;

public partial class Root : Control
{
    private AnimationManager animationManager;
    private Train testTrain;
    private Grid grid;
    private RichTextLabel brakeInfoLabel;

    private bool scheduled;

    public override void _Ready()
    {
        animationManager = GetNode<AnimationManager>("/root/AnimationManager");

        testTrain = GetNode<Train>("Train");
        testTrain.FinishedPath += _OnFinishedPath;

        grid = GetNode<Grid>("GridLayers/Ground");

        brakeInfoLabel = GetNode<RichTextLabel>("UIContainer/BrakeInfoLabel");
        brakeInfoLabel.PivotOffset = brakeInfoLabel.Size / 2f;
        animationManager.AddSwayAnimation(brakeInfoLabel);
        animationManager.AddBlinkAnimation(brakeInfoLabel);
    }

    private void _AssignTrainPath()
    {
        var coordinate = grid.LocalToMap(testTrain.GetTrainPosition());
        if (grid.TrainPaths.ContainsKey(coordinate))
        {
            testTrain.AcceptPath(grid.TrainPaths[coordinate]);
            scheduled = true;
        }
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
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey)
        {
            var keyEvent = (InputEventKey)@event;
            if (keyEvent.Keycode == Key.Space && keyEvent.Pressed)
            {
                testTrain.ToggleBrake();
            }
            return;
        }
    }
}