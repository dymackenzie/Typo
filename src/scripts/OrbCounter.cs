using Godot;
using System;

public partial class OrbCounter : Control
{

    Globals Globals;
    Label label;
    Color color;

    public override void _Ready() {
        Globals = GetNode<Globals>("/root/Globals");
        label = GetNode<Label>("Label");
        label.Text = Globals.GetExperience().ToString();
        color = label.Modulate;
        Globals.ExperienceChanged += OnExperienceChanged;
    }

    public void OnExperienceChanged() {
        Tween tween = CreateTween().SetTrans(Tween.TransitionType.Linear).SetEase(Tween.EaseType.Out);
        label.Text = Globals.GetExperience().ToString();
        // animate color change
        tween.TweenProperty(label, "modulate", Colors.White, 0.1);
        tween.TweenProperty(label, "modulate", color, 0.1);
    }

}
