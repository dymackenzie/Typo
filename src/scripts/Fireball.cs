using Godot;
using System;

public partial class Fireball : RigidBody2D
{

	[Export] float launch = 1.0f;
    [Export] float speed = 1.5f;
    [Export] float friction = 1f;
    [Export] float launchPeriod = 1f;
    [Export] float cooldownPeriod = 2.0f;
	[Export] PackedScene explosion;

	public bool magnetize = true;
    public float timePass = 0f;

    public Globals Globals;
    public Player player;
	public Timer timer;
	public AnimationPlayer anim;
	public AnimatedSprite2D sprite;
	public Vector2 velocity;

	public override void _Ready() {
        foreach (Node node in GetTree().GetNodesInGroup("player")) {
            player = (Player) node;
        }
		anim = GetNode<AnimationPlayer>("AnimationPlayer");
		sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");

		// begin domino effect
		InitiateTimer();
		anim.Play("spawn");
        LaunchVelocity();
	}

    private void InitiateTimer() {
        timer = GetNode<Timer>("Cooldown");
        timer.WaitTime = cooldownPeriod;
		timer.Start();
    }

    /*
    Sets initial launch velocity
    */
    public void LaunchVelocity() {
		Vector2 direction = (player.GlobalPosition - GlobalPosition).Normalized();
		velocity += direction * launch;
		sprite.Rotation = direction.Angle(); // set sprite's rotation
    }

	public override void _PhysicsProcess(double delta) {
		// deal with slowdown
		anim.SpeedScale = Globals.inSlowdown ? Globals.slowdownRate : 1;
		delta *= Globals.inSlowdown ? Globals.slowdownRate : 1;

		// animation
		anim.Play("idle");

        if (timePass <= launchPeriod) {
            ApplyFriction();
            MoveAndCollide(velocity * (float) delta);
            timePass += (float) delta;
        }
        if (magnetize) {
            Magnetize(delta);
        }
	}

    /*
    Move towards player for a certain amount of time
    */
    public void Magnetize(double delta) {
        Vector2 direction = (player.GlobalPosition - GlobalPosition).Normalized();
        velocity += direction * speed;
        ApplyFriction();
        MoveAndCollide(velocity * (float) delta);
    }

	public void OnCooldownTimeout() {
		magnetize = false;
	}

    private void ApplyFriction() {
        float length = velocity.Length();
        if (length > 0) {
            float mul = Math.Max(length - friction, 0) / length;
            velocity *= new Vector2(mul, mul);
        }
    }

	private void PlayExplosionParticle() {
		GpuParticles2D explosionParticle = (GpuParticles2D) explosion.Instantiate();
		explosionParticle.GlobalPosition = GlobalPosition;
		GetParent().AddChild(explosionParticle);
	}

    /*
    If touches player, increment experience
    */
    public void OnHitboxBodyEntered(Node2D body) {
        if (body.IsInGroup("player")) {
            anim.Play("explosion");
			PlayExplosionParticle();
			EmitSignal(nameof(Player.PlayerHitEventHandler));
            QueueFree();
        }
    }

}
