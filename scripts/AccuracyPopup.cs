using System.Collections.Generic;
using Godot;

[GlobalClass]
public partial class AccuracyPopup : Control
{
    public AccuracyGrade Grade;

    private RichTextLabel label;

    private Dictionary<AccuracyGrade, string> textForGrade = new Dictionary<AccuracyGrade, string>()
    {
        { AccuracyGrade.Perfect, "[wave amp=20 freq=6][rainbow]PERFECT![/rainbow][/wave]" },
        { AccuracyGrade.Good, "[wave amp=20 freq=6][color=GREEN]GOOD[/color][/wave]" },
        { AccuracyGrade.OK, "[color=ORANGE]OK[/color]" }
    };

    private Tween tween;

    public override void _Ready()
    {
        label = GetNode<RichTextLabel>("RichTextLabel");
        label.Text = textForGrade[Grade];

        tween = CreateTween();
        tween.TweenProperty(this, "position:y", -24f, 1.2f)
            .AsRelative()
            .SetEase(Tween.EaseType.OutIn)
            .SetTrans(Tween.TransitionType.Linear);

        tween.Finished += _OnFinished;
    }

    private void _OnFinished()
    {
        QueueFree();
    }
}