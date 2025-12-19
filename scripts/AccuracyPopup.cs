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
    private Tween maxSpeedTween;

    public override void _Ready()
    {
        label = GetNode<RichTextLabel>("RichTextLabel");
        label.Text = textForGrade[Grade];

        tween = CreateTween();
        tween.TweenProperty(this, "position:y", -20f, 1f)
            .AsRelative()
            .SetEase(Tween.EaseType.OutIn)
            .SetTrans(Tween.TransitionType.Linear);
        tween.Finished += _OnFinished;
    }

    private void _OnFinished()
    {
        if (Grade != AccuracyGrade.Perfect)
        {
            QueueFree();
            return;
        }
        maxSpeedTween = CreateTween();
        maxSpeedTween.TweenProperty(this, "position:y", -10f, 1f)
            .AsRelative()
            .SetEase(Tween.EaseType.OutIn)
            .SetTrans(Tween.TransitionType.Linear);
        maxSpeedTween.Finished += _OnMaxSpeedTweenFinished;
        label.Text = "+20 kph max speed!";
    }

    private void _OnMaxSpeedTweenFinished()
    {
        QueueFree();
    }
}