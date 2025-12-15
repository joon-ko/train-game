using Godot;

public partial class TrainPathVisualizer : Control
{
    private GridManager gridManager;
    private TileMapLayer groundLayer;

    private const int CIRCLE_RADIUS = 8;
    private const int CIRCLE_WIDTH = 8;
    private Color START_COLOR = Colors.ForestGreen;
    private Color END_COLOR = Colors.OrangeRed;

    public override void _Ready()
    {
        gridManager = GetTree().CurrentScene.GetNode<GridManager>("GridManager");
        groundLayer = GetTree().CurrentScene.GetNode<TileMapLayer>("GridManager/Ground");
    }

    public override void _PhysicsProcess(double delta)
    {
        QueueRedraw();
    }

    public override void _Draw()
    {
        foreach (var paths in gridManager.TrainPaths.Values)
        {
            foreach (var path in paths)
            {
                var startPosition = groundLayer.MapToLocal(path.StartCoordinate);
                var endPosition = groundLayer.MapToLocal(path.EndCoordinate);
                DrawStartCircle(startPosition);
                DrawEndCircle(endPosition);
                DrawLine(startPosition, endPosition, Colors.White, 4f);
            }
        }
    }

    public void DrawStartCircle(Vector2 p)
    {
        DrawArc(p, CIRCLE_RADIUS, Mathf.Pi / 2, (Mathf.Pi / 2) + Mathf.Pi, 32, START_COLOR, CIRCLE_WIDTH);
    }

    public void DrawEndCircle(Vector2 p)
    {
        DrawArc(p, CIRCLE_RADIUS, -Mathf.Pi / 2, (-Mathf.Pi / 2) + Mathf.Pi, 32, END_COLOR, CIRCLE_WIDTH);
    }
}