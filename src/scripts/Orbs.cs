using Godot;
using System;
using System.Linq;

public partial class Orbs : RigidBody2D
{

    [Export] public int level = 1;
	[Export] float launch = 70.0f;
    [Export] float speed = 1.5f;
    [Export] float friction = 1f;
    [Export] float launchPeriod = 1f;
    [Export] float cooldownPeriod = 2.0f;

	public bool magnetize = false;
    public float timePass = 0f;

    public Globals Globals;
    public Player player;
	public Timer timer;
	public RandomNumberGenerator random = new();
	public Vector2 velocity;

    // for scaling
    public AnimatedSprite2D sprite;
    public CollisionShape2D collisionShape2D;

	public override void _Ready() {
        foreach (Node node in GetTree().GetNodesInGroup("player")) {
            player = (Player) node;
        }
        Globals = GetNode<Globals>("/root/Globals");

		InitiateTimer();
		random.Randomize();
        InitiateScale();
        LaunchVelocity();
	}

    /*
    Scales orb for more random shape
    */
    private void InitiateScale() {
        sprite = GetNode<AnimatedSprite2D>("Sprite2D");
        collisionShape2D = GetNode<CollisionShape2D>("Sprite2D/Hitbox/Hitbox");
        float scaleChange = random.RandfRange(0, 0.03f) * (1 + (level * 0.5f));
        sprite.Scale += new Vector2(scaleChange, scaleChange);
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
        velocity = new Vector2(random.RandfRange(-launch, launch), random.RandfRange(-launch, launch));
    }

	public override void _PhysicsProcess(double delta) {

        // deal with slowdown
		delta *= Globals.InSlowdown ? Globals.SlowdownRate : 1;

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
    Move towards player
    */
    public void Magnetize(double delta) {
        Vector2 direction = (player.GlobalPosition - GlobalPosition).Normalized();
        velocity += direction * speed;
        ApplyFriction();
        MoveAndCollide(velocity * (float) delta);
    }

	public void OnCooldownTimeout() {
		magnetize = true;
	}

    private void ApplyFriction() {
        float length = velocity.Length();
        if (length > 0) {
            float mul = Math.Max(length - friction, 0) / length;
            velocity *= new Vector2(mul, mul);
        }
    }

    /*
    If touches player, increment experience
    */
    public void OnHitboxBodyEntered(Node2D body) {
        if (body.IsInGroup("player")) {
            Globals = GetNode<Globals>("/root/Globals");
            Globals.AddExperience(level);
            QueueFree();
        }
    }

}
