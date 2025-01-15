using Godot;
using System;

public partial class BlastArea : Area2D
{
	
	[Export] public bool enabled = false;
	[Export] public float blastRadius = 25.0f;
	[Export] public Color searchColor;
	[Export] public Color killColor;

	public float slowdownRate;
	public Globals Globals;
	public Player player = null;
	public Timer timer;
	public Vector2 position;
	public CollisionShape2D shape2D;

	public override void _Ready() {
		Globals = GetNode<Globals>("/root/Globals");
		shape2D = GetNode<CollisionShape2D>("CollisionShape2D");
		timer = GetNode<Timer>("Timer");
		shape2D.Shape.Set("radius", blastRadius);
		// make fade in on instance
		Tween tween = CreateTween().SetTrans(Tween.TransitionType.Quint).SetEase(Tween.EaseType.Out);
		tween.TweenProperty(this, "modulate:a", 0.75, timer.WaitTime);
		timer.Start();
	}

	public void MakeVisible() {
		if (player != null && player.shield.IsStopped()) {
			player.OnDamage();
		}
		enabled = true;
		// finish animation
		Tween tween = CreateTween().SetTrans(Tween.TransitionType.Quint).SetEase(Tween.EaseType.Out);
		tween.TweenProperty(this, "modulate:a", 0, 1);
		tween.TweenCallback(Callable.From(() => {QueueFree();}));
	}

	public override void _Draw() {
		if (enabled)
        	DrawCircle(position, blastRadius, killColor);
		else {
			DrawArc(Vector2.Zero, blastRadius, 0, (float) Math.Tau * (float) (timer.TimeLeft / timer.WaitTime), 50, killColor, 1, false);
			DrawCircle(position, blastRadius, searchColor);
		}
    }

	public override void _Process(double delta) {
		timer.Paused = Globals.inSlowdown;
		QueueRedraw();
	}

	/* connected signals */

	public void OnBodyEntered(Node2D body) {
		if (body.IsInGroup("player")) {
			player = (Player) body;
		}
	}

	public void OnBodyExited(Node2D body) {
		if (body.IsInGroup("player")) {
			player = null;
		}
	}

	public void OnTimeout() {
		MakeVisible();
	}

}
