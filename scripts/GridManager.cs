using Godot;
using System.Collections.Generic;

[GlobalClass]
public partial class GridManager : Node
{
	[Export] public int Width { get; set; } = 10;
	[Export] public int Height { get; set; } = 10;

	public Dictionary<Vector2I, List<PathInfo>> TrainPaths = [];

	// Hardcoded vertex locations
	private Vector2I START = new Vector2I(26, 9);
	private Vector2I FIRST_CORNER = new Vector2I(26, 0);
	private Vector2I SWITCH = new Vector2I(23, 0);
	private Vector2I FAR_RIGHT_CORNER = new Vector2I(23, -5);
	private Vector2I FAR_CORNER_ONE = new Vector2I(16, -5);
	private Vector2I FAR_CORNER_TWO = new Vector2I(16, -2);
	private Vector2I FAR_CORNER_THREE = new Vector2I(13, -2);
	private Vector2I LEFT_CORNER = new Vector2I(13, 6);
	private Vector2I MIDDLE_CORNER_ONE = new Vector2I(18, 0);
	private Vector2I MIDDLE_CORNER_TWO = new Vector2I(18, 3);
	private Vector2I FAR_LEFT_JUNCTION = new Vector2I(13, 3);
	private Vector2I BOTTOM_CORNER = new Vector2I(26, 6);

	private SwitchManager switchManager;

	public override void _Ready()
	{
		switchManager = GetNode<SwitchManager>("/root/SwitchManager");
		GenerateTrainPaths();
	}

	private void GenerateTrainPaths()
	{
		// Still hardcoding all paths for test level for now
		// TODO: Algorithm to read tile map layers and generate train paths automatically

		// Head to the switch and choose the bent or straight path...
		TrainPaths.Add(START, [new PathInfo(START, FIRST_CORNER, Direction.NegY)]);
		TrainPaths.Add(FIRST_CORNER, [new PathInfo(FIRST_CORNER, SWITCH, Direction.NegX)]);
		switchManager.AddSwitch(SWITCH);

		// Bent switch path
		TrainPaths.Add(SWITCH, [new PathInfo(SWITCH, FAR_RIGHT_CORNER, Direction.NegY, SwitchOrientation.Bent)]);
		TrainPaths.Add(FAR_RIGHT_CORNER, [new PathInfo(FAR_RIGHT_CORNER, FAR_CORNER_ONE, Direction.NegX)]);
		TrainPaths.Add(FAR_CORNER_ONE, [new PathInfo(FAR_CORNER_ONE, FAR_CORNER_TWO, Direction.PosY)]);
		TrainPaths.Add(FAR_CORNER_TWO, [new PathInfo(FAR_CORNER_TWO, FAR_CORNER_THREE, Direction.NegX)]);
		TrainPaths.Add(FAR_CORNER_THREE, [new PathInfo(FAR_CORNER_THREE, LEFT_CORNER, Direction.PosY)]);

		// Straight switch path
		var switchNode = TrainPaths[SWITCH];
		switchNode.Add(new PathInfo(SWITCH, MIDDLE_CORNER_ONE, Direction.NegX, SwitchOrientation.Straight));
		TrainPaths.Add(MIDDLE_CORNER_ONE, [new PathInfo(MIDDLE_CORNER_ONE, MIDDLE_CORNER_TWO, Direction.PosY)]);
		TrainPaths.Add(MIDDLE_CORNER_TWO, [new PathInfo(MIDDLE_CORNER_TWO, FAR_LEFT_JUNCTION, Direction.NegX)]);
		TrainPaths.Add(FAR_LEFT_JUNCTION, [new PathInfo(FAR_LEFT_JUNCTION, LEFT_CORNER, Direction.PosY)]);

		// Unified again to get back to start!
		TrainPaths.Add(LEFT_CORNER, [new PathInfo(LEFT_CORNER, BOTTOM_CORNER, Direction.PosX)]);
		TrainPaths.Add(BOTTOM_CORNER, [new PathInfo(BOTTOM_CORNER, FIRST_CORNER, Direction.NegY)]);
	}
}
