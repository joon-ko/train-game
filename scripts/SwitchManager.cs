using Godot;
using System.Collections.Generic;

public partial class SwitchManager : Node
{
    private Dictionary<int, Vector2I> switchLocations = new Dictionary<int, Vector2I>();
    private Dictionary<Vector2I, SwitchOrientation> switchStates = new Dictionary<Vector2I, SwitchOrientation>();

    public void AddSwitch(Vector2I coord)
    {
        if (!switchStates.ContainsKey(coord))
        {
            switchLocations.Add(switchLocations.Count, coord);
            switchStates.Add(coord, SwitchOrientation.Straight);
        }
    }

    public void ToggleSwitch(int switchId)
    {
        var switchCoord = switchLocations[switchId];
        var newOrientation = switchStates[switchCoord] == SwitchOrientation.Straight
            ? SwitchOrientation.Bent
            : SwitchOrientation.Straight;

        switchStates[switchCoord] = newOrientation;
    }

    public Vector2I GetSwitchCoord(int index)
    {
        return switchLocations[index];
    }

    public SwitchOrientation GetSwitchOrientation(Vector2I coord)
    {
        return switchStates[coord];
    }

    public void ClearSwitches()
    {
        switchLocations.Clear();
        switchStates.Clear();
    }

    public int GetSwitchCount()
    {
        return switchLocations.Count;
    }
}