using Godot;

public partial class AnimationManager : Node
{
    public void AddSwayAnimation(Node node)
    {
        var tween = node.CreateTween();
        tween.SetLoops();
        tween.TweenProperty(node, "rotation", Mathf.DegToRad(2), 1.4)
            .SetEase(Tween.EaseType.InOut)
            .SetTrans(Tween.TransitionType.Sine);
        tween.TweenProperty(node, "rotation", Mathf.DegToRad(-2), 1.4)
            .SetEase(Tween.EaseType.InOut)
            .SetTrans(Tween.TransitionType.Sine);
    }

    public void AddBlinkAnimation(Node node)
    {
        var tween = node.CreateTween();
        tween.SetLoops();
        tween.TweenProperty(node, "modulate:a", 0.0, 1.2)
            .SetEase(Tween.EaseType.InOut)
            .SetTrans(Tween.TransitionType.Sine);
        tween.TweenProperty(node, "modulate:a", 1.0, 1.2)
            .SetEase(Tween.EaseType.InOut)
            .SetTrans(Tween.TransitionType.Sine);
        tween.TweenInterval(1.0);
    }
}