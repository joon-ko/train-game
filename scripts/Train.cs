using Godot;

[GlobalClass]
public partial class Train : Node2D
{
    [Export] public Vector2I StartCoordinate { get; set; }
    [Export] public Direction StartDirection { get; set; } = Direction.NegY;
    [Export] public float MaxSpeed { get; set; } = 1.5f;

    [Export] public HeadTraincar Head { get; set; }
    [Export] public FollowTraincar Middle { get; set; }
    [Export] public FollowTraincar Tail { get; set; }

    public CargoType CarriedCargo = CargoType.None;
    public int CargoCount = 0;

    private TileMapLayer groundLayer;

    private AudioStreamPlayer chuggingPlayer;
    private AudioStreamPlayer cargoPickupPlayer;
    private AudioStreamPlayer cargoDropoffPlayer;

    public override void _Ready()
    {
        chuggingPlayer = GetNode<AudioStreamPlayer>("ChuggingPlayer");
        cargoPickupPlayer = GetNode<AudioStreamPlayer>("CargoPickup");
        cargoDropoffPlayer = GetNode<AudioStreamPlayer>("CargoDropoff");
    }

    public void StartChugging()
    {
        chuggingPlayer.Play();
    }

    public void StopChugging()
    {
        chuggingPlayer.Stop();
    }

    public bool IsChugging()
    {
        return chuggingPlayer.Playing;
    }

    public void PlayCargoPickupSound()
    {
        cargoPickupPlayer.Play();
    }

    public void PlayCargoDropoffSound()
    {
        cargoDropoffPlayer.Play();
    }
}