using Godot;

public partial class TitleCard : Control
{
    private TextureRect santaHat;
    private RichTextLabel label;

    private Vector2I POSITION_OFFSET = new Vector2I(141, -34);

    public override void _Ready()
    {
        santaHat = GetNode<TextureRect>("SantaHat");
        label = GetNode<RichTextLabel>("RichTextLabel");
    }

    public override void _Process(double delta)
    {
        santaHat.GlobalPosition = label.GlobalPosition + POSITION_OFFSET;
    }
}
