using Godot;

public class PathInfo
{
    public Direction Direction;
    public Vector2I StartCoordinate;
    public Vector2I EndCoordinate;

    public PathInfo(Vector2I start, Vector2I end, Direction direction)
    {
        StartCoordinate = start;
        EndCoordinate = end;
        Direction = direction;
    }

    public void PrintInfo()
    {
        GD.Print("Path info:");
        GD.Print($"Direction: {Direction}");
        GD.Print($"Start: {StartCoordinate}");
        GD.Print($"End: {EndCoordinate}");
    }
}