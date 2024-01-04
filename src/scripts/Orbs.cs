using Godot;
using System;
using System.Linq;

public partial class Orbs : RigidBody2D
{

    public const float Z_AXIS = 5.0f;

    [Export] NodePath playerPath;
	[Export] float launch = 50.0f;
    [Export] float speed = 1.5f;
    [Export] float friction = 1f;
    [Export] float launchPeriod = 1f;
    [Export] float cooldownPeriod = 2.0f;

	public bool magnetize = false;
    public float timePass = 0f;

    public Player player;
	public Timer timer;
	public RandomNumberGenerator random = new();
	public Vector2 velocity;

	public override void _Ready() {
        foreach (Player player in GetTree().GetNodesInGroup("player").Cast<Player>()) {
            this.player = player;
        }
		timer = GetNode<Timer>("Cooldown");
        timer.WaitTime = cooldownPeriod;
		timer.Start();
		random.Randomize();
        LaunchVelocity();
	}

    public void LaunchVelocity() {
        velocity = new Vector2(random.RandfRange(-launch, launch), random.RandfRange(-launch, launch) + Z_AXIS);
    }

	public override void _PhysicsProcess(double delta) {
        if (timePass <= launchPeriod) {
            ApplyFriction();
            MoveAndCollide(velocity * (float) delta);
            timePass += (float) delta;
        }
        if (magnetize) {
            Magnetize(delta);
        }
	}

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

}
