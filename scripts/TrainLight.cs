using System.Collections.Generic;
using Godot;

[GlobalClass]
public partial class TrainLight : PointLight2D
{
    [Export] public HeadTraincar Head { get; set; }

    private Dictionary<Direction, Vector2I> scaleForDirection = new Dictionary<Direction, Vector2I>()
    {
        { Direction.NegY, new Vector2I(1, 1) },
        { Direction.PosY, new Vector2I(-1, -1) },
        { Direction.NegX, new Vector2I(-1, 1) },
        { Direction.PosX, new Vector2I(1, -1) }
    };

    private Dictionary<Direction, Vector2I> offsetForDirection = new Dictionary<Direction, Vector2I>()
    {
        { Direction.NegY, new Vector2I(16, -8) },
        { Direction.PosY, new Vector2I(-16, 8) },
        { Direction.NegX, new Vector2I(-16, -8) },
        { Direction.PosX, new Vector2I(16, 8) }
    };

    public override void _Process(double delta)
    {
        Position = Head.GetTrainPosition() + (offsetForDirection[Head.Direction] / 2);
        Scale = scaleForDirection[Head.Direction];
    }
}