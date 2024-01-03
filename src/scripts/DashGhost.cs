using Godot;
using System;

public partial class DashGhost : Sprite2D
{

	public override void _Ready() {
		Tween tween = CreateTween().SetTrans(Tween.TransitionType.Quart).SetEase(Tween.EaseType.Out);
		// fades out sprite
		tween.TweenProperty(this, "modulate:a", 0, 0.5);
		// destroyes sprite
		tween.TweenCallback(Callable.From(() => { QueueFree(); }));
	}

}
