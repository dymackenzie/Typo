using Godot;
using System;

public partial class Fireball : RigidBody2D
{

    [Signal] public delegate void PlayerHitEventHandler();

	[Export] float launch = 40.0f;
    [Export] float speed = 20.0f;
    [Export] float friction = 1.0f;
    [Export] float launchPeriod = 0.2f;
    [Export] float lifetime = 10f;
	[Export] PackedScene explosion;

	public bool magnetize = true;
    public float timePass = 0f;

    public Globals Globals;
    public Player player;
	public Timer timer;
	public AnimationPlayer anim;
	public AnimatedSprite2D sprite;
	public Vector2 velocity;
    public Vector2 direction;

	public override void _Ready() {
        foreach (Node node in GetTree().GetNodesInGroup("player")) {
            player = (Player) node;
        }
        Globals = GetNode<Globals>("/root/Globals");
		anim = GetNode<AnimationPlayer>("AnimationPlayer");
		sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");

		// begin domino effect
		InitiateTimer();
        LaunchVelocity();
	}

    public override void _PhysicsProcess(double delta) {
		// deal with slowdown
		anim.SpeedScale = Globals.inSlowdown ? Globals.slowdownRate : 1;
		delta *= Globals.inSlowdown ? Globals.slowdownRate : 1;

		// if animation is explosion, don't move
        if (anim.CurrentAnimation == "explosion")
            return;

        anim.Play("spawn");

        if (timePass <= launchPeriod) {
            MoveAndCollide(velocity * (float) delta);
            timePass += (float) delta;
        }
        sprite.Rotation = direction.Angle();
        velocity += direction * speed;
        MoveAndCollide(velocity * (float) delta);
	}

    private void InitiateTimer() {
        timer = GetNode<Timer>("Cooldown");
        timer.WaitTime = lifetime;
		timer.Start();
    }

    public void OnCooldownTimeout() {
		anim.Play("explosion");
        PlayExplosionParticle();
        QueueFree();
	}

    /*
    Sets initial launch velocity
    */
    public void LaunchVelocity() {
		direction = (player.GlobalPosition - GlobalPosition).Normalized();
		velocity += direction * launch;
		sprite.Rotation = direction.Angle(); // set sprite's rotation
    }

    /*
    Helper function to apply friction to velocity, slows down to 0
    */
    private void ApplyFriction() {
        float length = velocity.Length();
        if (length > 0) {
            float mul = Math.Max(length - friction, 0) / length;
            velocity *= new Vector2(mul, mul);
        }
    }

    /*
    Function to instantiate and play explosion particle
    */
	private void PlayExplosionParticle() {
		GpuParticles2D explosionParticle = (GpuParticles2D) explosion.Instantiate();
		explosionParticle.GlobalPosition = GlobalPosition;
		AddSibling(explosionParticle);
	}

    /*
    OnAnimationFinished
    */
    public void OnAnimationFinished(StringName animName) {
		if ((string) animName == "explosion") {
			PlayExplosionParticle();
			QueueFree();
		}
	}

    /*
    If touches player, explode and hit player
    */
    public void OnHitboxBodyEntered(Node2D body) {
        if (body.IsInGroup("player")) {
            anim.Play("explosion");
			EmitSignal(nameof(PlayerHit));
        }
    }

}
