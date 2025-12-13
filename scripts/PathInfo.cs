using Godot;

public class PathInfo
{
    public Direction Direction;
    public Vector2I StartCoordinate;
    public Vector2I EndCoordinate;    public bool IsSwitch;
    public SwitchOrientation SwitchOrientation;

    public PathInfo(
        Vector2I start,
        Vector2I end,
        Direction direction,
        SwitchOrientation switchOrientation = SwitchOrientation.Straight)
    {
        StartCoordinate = start;
        EndCoordinate = end;
        Direction = direction;
        SwitchOrientation = switchOrientation;
    }

    public void PrintInfo()
    {
        GD.Print("\nPath info:");
        GD.Print($"- direction: {Direction}");
        GD.Print($"- start: {StartCoordinate}");
        GD.Print($"- end: {EndCoordinate}");
        GD.Print($"- switch orientation: {SwitchOrientation}");
    }
}