using Godot;
using System;

public partial class BlastArea : Area2D
{
	
	[Export] public bool enabled = false;
	[Export] public float blastRadius = 30.0f;
	[Export] public Color searchColor;
	[Export] public Color killColor;
	[Export] PackedScene explosion;
	[Export] PackedScene trail;
	[Export] Sprite2D ring;
	[Export] Sprite2D ring2;

	public Globals Globals;
	public Player player = null;
	public Vector2 position;
	public CollisionShape2D shape2D;

	// timer
	public float time = 0.0f;
	public float timeLimit = 2.0f;

	public override void _Ready() {
		Globals = GetNode<Globals>("/root/Globals");
		shape2D = GetNode<CollisionShape2D>("CollisionShape2D");
		shape2D.Shape.Set("radius", blastRadius);
		ring.Scale = new Vector2(blastRadius / 18.0f + 1f, blastRadius / 18.0f + 1f);
		ring2.Scale = new Vector2(blastRadius / 18.0f, blastRadius / 18.0f);
		
		// make fade in on instance
		Tween tween = CreateTween().SetTrans(Tween.TransitionType.Linear).SetEase(Tween.EaseType.Out);
		tween.TweenProperty(this, "modulate:a", 0.75, timeLimit);
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
		if (enabled) {
        	// DrawCircle(position, blastRadius * 1.2f, killColor);
			PlayExplosionParticle();
			RingExplosion();
		} else {
			// DrawArc(Vector2.Zero, blastRadius, 0, (float) Math.Tau * (float) -(time / timeLimit), 50, killColor, 1, false);
			// DrawArc(Vector2.Zero, blastRadius + 0.9f, 0, (float) Math.Tau * (float) -((time + 0.3f) / timeLimit), 50, killColor, 1, false);
			float angle = (float) Math.Tau * (float) -(time / timeLimit);
			PlayTrailParticle(
				GlobalPosition + blastRadius * new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)), 
				new Vector2(Mathf.Cos(angle), Mathf.Sin(angle))
			);
			// DrawCircle(position, blastRadius, searchColor);
		}
    }

    private void RingExplosion() {
		// explode ring outwards
		Vector2 scale = ring.Scale;
		Vector2 scale2 = ring2.Scale;
        Tween tween = CreateTween().SetTrans(Tween.TransitionType.Quint).SetEase(Tween.EaseType.Out);
		tween.SetParallel();
		tween.TweenProperty(ring, "modulate:a", 0.5, 0.2f);
		tween.TweenProperty(ring, "modulate:a", 0.6, 0.2f);
		tween.TweenProperty(ring, "scale", scale + new Vector2(0.05f, 0.05f), 0.2f);
		tween.TweenProperty(ring, "scale", scale2 + new Vector2(0.05f, 0.05f), 0.2f);
		tween.TweenProperty(ring, "rotation", Math.Tau, 0.4f);
		tween.TweenProperty(ring2, "rotation", -Math.Tau, 0.4f);
    }

    public override void _Process(double delta) {
		if (Globals.inSlowdown) {
			time += (float) delta * Globals.slowdownRate;
		} else {
			time += (float) delta;
		}
		if (time >= timeLimit) {
			MakeVisible();
		}
		QueueRedraw();
	}

	/*
	Function to instantiate and play trail particle
	*/
	private void PlayTrailParticle(Vector2 position, Vector2 direction) {
		GpuParticles2D trailParticle = (GpuParticles2D) trail.Instantiate();
		trailParticle.GlobalPosition = position;
		trailParticle.Rotation = direction.Angle();
		trailParticle.Emitting = true;
		AddSibling(trailParticle);
	}

	/*
    Function to instantiate and play explosion particle
    */
	private void PlayExplosionParticle() {
		GpuParticles2D explosionParticle = (GpuParticles2D) explosion.Instantiate();
        explosionParticle.Scale = new Vector2(0.2f, 0.2f);
		explosionParticle.GlobalPosition = GlobalPosition;
        explosionParticle.Emitting = true;
		AddSibling(explosionParticle);
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

}
