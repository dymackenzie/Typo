using Godot;
using System;

public partial class BlastArea : Area2D
{
	
	[Export] public bool enabled = false;
	[Export] public float blastRadius = 25.0f;
	[Export] public Color searchColor;
	[Export] public Color killColor;

	public Player player = null;
	public Vector2 position;
	public CollisionShape2D shape2D;

	public override void _Ready() {
		shape2D = GetNode<CollisionShape2D>("CollisionShape2D");
		shape2D.Shape.Set("radius", blastRadius);
		// make fade in on instance
		Tween tween = CreateTween().SetTrans(Tween.TransitionType.Quint).SetEase(Tween.EaseType.Out);
		tween.TweenProperty(this, "modulate:a", 1, 0.5);
	}

	public override void _Draw() {
		GD.Print(Modulate.A);
		if (enabled)
        	DrawCircle(position, blastRadius, killColor);
		else
			DrawCircle(position, blastRadius, searchColor);
    }

	public override void _Process(double delta) {
		QueueRedraw();
		if (enabled && player != null) {
			player.OnDamage();
		}
	}

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

}
