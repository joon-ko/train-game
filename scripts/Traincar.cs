using Godot;

public partial class Traincar : Node2D
{
    public float Speed;
    public Direction Direction;
    public Direction PreviousDirection;

    public Path2D currentPath;
    public PathFollow2D currentPathFollow;
    public AnimatedSprite2D currentSprite;

    public Path2D previousPath;
    public PathFollow2D previousPathFollow;
    public AnimatedSprite2D previousSprite;

    public Train train;
    public TileMapLayer tileMapLayer;

    public override void _Ready()
    {
        currentPath = GetNode<Path2D>("CurrentPath");
        currentPathFollow = GetNode<PathFollow2D>("CurrentPath/PathFollow2D");
        currentSprite = GetNode<AnimatedSprite2D>("CurrentPath/PathFollow2D/Sprite");

        previousPath = GetNode<Path2D>("PreviousPath");
        previousPathFollow = GetNode<PathFollow2D>("PreviousPath/PathFollow2D");
        previousSprite = GetNode<AnimatedSprite2D>("PreviousPath/PathFollow2D/Sprite");

        train = GetParent<Train>();

        tileMapLayer = GetTree().CurrentScene.GetNode<TileMapLayer>("GridManager/Ground");
    }

	public Vector2 GetTrainPosition()
	{
		return currentPathFollow.Position;
	}
}