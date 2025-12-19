using Godot;
using System.Collections.Generic;

public class SearchNode
{
	public Vector2I Start;
	public Vector2I Location;
	public Direction Direction;
	public SwitchOrientation? Orientation;

	public SearchNode(Vector2I start, Vector2I location, Direction direction, SwitchOrientation? orientation = null)
	{
		Start = start;
		Location = location;
		Direction = direction;
		Orientation = orientation;
	}
}

[GlobalClass]
public partial class GridManager : Node
{
	public Dictionary<Vector2I, List<PathInfo>> TrainPaths = [];

	[Export] Vector2I StartLocation { get; set; }
	[Export] Direction StartDirection { get; set; }
	[Export] Node2D LevelPortals { get; set; }

	private SwitchManager switchManager;
	private TileMapLayer groundLayer;
	private TileMapLayer environmentLayer;

	private List<Vector2I> levelPortalCoords = new List<Vector2I>();

	private Dictionary<Direction, Vector2I> directionDeltas = new Dictionary<Direction, Vector2I>()
	{
		{ Direction.PosX, new Vector2I(1, 0) },
		{ Direction.PosY, new Vector2I(0, 1) },
		{ Direction.NegX, new Vector2I(-1, 0) },
		{ Direction.NegY, new Vector2I(0, -1) }
	};

	public override void _Ready()
	{
		switchManager = GetNode<SwitchManager>("/root/SwitchManager");
		groundLayer = GetNode<TileMapLayer>("Ground");
		environmentLayer = GetNode<TileMapLayer>("Environment");

		SetLevelPortalCoords();
		GenerateTrainPaths(StartLocation, StartDirection);
	}

	public void SetLevelPortalCoords()
	{
		foreach (LevelPortal portal in LevelPortals.GetChildren())
		{
			levelPortalCoords.Add(portal.PortalCoordinate);
		}
	}

	private void GenerateTrainPaths(Vector2I startLocation, Direction direction)
	{
		var levelPortalCount = 0;

		// Clear train paths and switch states and start a path from the start location.
		TrainPaths.Clear();
		switchManager.ClearSwitches();

		// Initialize the search queue.
		var queue = new Queue<SearchNode>();
		queue.Enqueue(new SearchNode(startLocation, startLocation, direction));

		// On each search step, walk to neighboring tile.
		// We will walk in a straight line until we reach a corner or a switch.
		while (queue.Count > 0)
		{
			var node = queue.Dequeue();
			var newLocation = node.Location + directionDeltas[node.Direction];

			var atlasCoord = groundLayer.GetCellAtlasCoords(newLocation);
			if (atlasCoord == new Vector2I(-1, -1))
			{
				continue;
			}

			var tile = TileManager.GetTileForAtlasCoord(atlasCoord);
			var isCorner = TileManager.IsCornerForDirection(tile, node.Direction);
			var isSwitch = TileManager.IsSwitchForDirection(tile, node.Direction);
			var isLevelPortal = levelPortalCoords.Contains(newLocation);

			if (isLevelPortal)
			{
				if (DoesTrainPathExist(node.Start, newLocation))
				{
					continue;
				}
				AddPath(new PathInfo(node.Start, newLocation, node.Direction, node.Orientation));
				levelPortalCount++;
				continue;
			}
			else if (isCorner)
			{
				// If we reach a corner:
				// The corner can either be a pure corner or a corner that merges with another lane.
				// In either case, end the current path and do nothing if this path already exists in the paths list.
				if (DoesTrainPathExist(node.Start, newLocation))
				{
					continue;
				}

				// If the path is new, add it to the paths list, and start a new path from the current location.
				// Update the direction based on the corner and continue walking on the new path.
				var newDirection = TileManager.GetNewDirectionForCorner(tile, node.Direction);
				AddPath(new PathInfo(node.Start, newLocation, node.Direction, node.Orientation));
				queue.Enqueue(new SearchNode(newLocation, newLocation, newDirection));
				continue;
			}
			else if (isSwitch)
			{
				// If we reach a switch:
				// End the current path and do nothing if this path already exists in the paths list.
				if (DoesTrainPathExist(node.Start, newLocation))
				{
					continue;
				}

				// If the path is new, add it to the paths list, and start two new paths from the current location.
				// Update the direction of the paths and the switch orientation leading to this path based on the switch.
				// Through either DFS or BFS, both paths will eventually be processed by the search queue.
				AddPath(new PathInfo(node.Start, newLocation, node.Direction, node.Orientation));

				var bentDirection = TileManager.GetBentDirectionForSwitch(tile, node.Direction);
				queue.Enqueue(new SearchNode(newLocation, newLocation, bentDirection, SwitchOrientation.Bent));

				var straightDirection = node.Direction;
				queue.Enqueue(new SearchNode(newLocation, newLocation, straightDirection, SwitchOrientation.Straight));

				// Add the switch to the switch manager.
				switchManager.AddSwitch(newLocation);
				continue;
			}

			// If we haven't hit a corner or switch, continue straight.
			queue.Enqueue(new SearchNode(node.Start, newLocation, node.Direction, node.Orientation));
		}

		// The train paths are finished generating when the search queue gets exhausted. Search branches exhaust themselves
		// naturally upon encountering duplicate paths.
		GD.Print("Finished processing train paths.");
		GD.Print($"Total path segments: {TrainPaths.Count}. Total switches: {switchManager.GetSwitchCount()}. Total level portals: {levelPortalCount}");
	}

	/// <summary>
	/// Determines whether a train path already exists at the given start and end coordinates.
	/// </summary>
	private bool DoesTrainPathExist(Vector2I start, Vector2I end)
	{
		if (!TrainPaths.ContainsKey(start))
		{
			return false;
		}

		var pathsAtStart = TrainPaths[start];
		
		return pathsAtStart.Exists((path) => path.EndCoordinate == end);
	}

	private void AddPath(PathInfo path)
	{
		List<PathInfo> paths;
		if (!TrainPaths.ContainsKey(path.StartCoordinate))
		{
			paths = new List<PathInfo>();
			TrainPaths[path.StartCoordinate] = paths;
		}
		else
		{
			paths = TrainPaths[path.StartCoordinate];
		}
		paths.Add(path);
	}
}
