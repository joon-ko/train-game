using Godot;

[GlobalClass]
public partial class CargoPanel : Control
{
    public int PurpleCargoRequired = 100;
    public int PinkCargoRequired = 100;

    public int PurpleCargoDelivered = 0;
    public int PinkCargoDelivered = 0;

    private Label purpleCargoLabel;
    private Label pinkCargoLabel;

    private const string CONTAINER = "PanelContainer/MarginContainer/HBoxContainer";

    public override void _Ready()
    {
        purpleCargoLabel = GetNode<Label>(CONTAINER + "/PurpleCargoContainer/HBoxContainer/Label");
        pinkCargoLabel = GetNode<Label>(CONTAINER + "/PinkCargoContainer/HBoxContainer/Label");

        UpdateLabels();
    }

    public override void _Process(double delta)
    {
        UpdateLabels();
    }

    private void UpdateLabels()
    {
        purpleCargoLabel.Text = $"{PurpleCargoDelivered}/{PurpleCargoRequired}";
        pinkCargoLabel.Text = $"{PinkCargoDelivered}/{PinkCargoRequired}";
    }
}