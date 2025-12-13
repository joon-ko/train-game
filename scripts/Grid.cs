using System.Collections.Generic;
using Godot;

[GlobalClass]
public partial class Grid : TileMapLayer
{
	[Export] public int Width { get; set; } = 10;
	[Export] public int Height { get; set; } = 10;

	public Dictionary<Vector2I, PathInfo> TrainPaths = [];

	private Vector2I BOTTOM_CORNER = new Vector2I(26, 7);
	private Vector2I TOP_CORNER = new Vector2I(13, -6);
	private Vector2I LEFT_CORNER = new Vector2I(13, 7);
	private Vector2I RIGHT_CORNER = new Vector2I(26, -6);

	private int sourceId;

	public override void _Ready()
	{
		sourceId = TileSet.GetSourceId(0);

		// Hardcoding paths for now
		TrainPaths.Add(BOTTOM_CORNER, new PathInfo(BOTTOM_CORNER, RIGHT_CORNER, Direction.NegY));
		TrainPaths.Add(RIGHT_CORNER, new PathInfo(RIGHT_CORNER, TOP_CORNER, Direction.NegX));
		TrainPaths.Add(TOP_CORNER, new PathInfo(TOP_CORNER, LEFT_CORNER, Direction.PosY));
		TrainPaths.Add(LEFT_CORNER, new PathInfo(LEFT_CORNER, BOTTOM_CORNER, Direction.PosX));
	}
}
