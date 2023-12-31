using Godot;
using System;

public partial class enemy : CharacterBody2D
{

	[Export] public float SPEED = 50.0f;
	[Export] public CharacterBody2D player;
	[Export] public float killRadius = 40.0f;

	public Vector2 velocity;
	public Vector2 target;
	public Timer attackTimer;
	public string state;
	public float randomNum;

    public override void _Ready() {
        velocity = Vector2.Zero;
		state = "surround";
		attackTimer = GetNode<Timer>("attack_timer");

		RandomNumberGenerator random = new();
		random.Randomize();
		randomNum = random.Randf();
    }

    public override void _PhysicsProcess(double delta) {
		switch(state) {
			case "surround":
				Move(GetCirclePosition(randomNum), delta);
				break;
			case "attack":
				Move(player.GlobalPosition, delta);
				break;
			case "hit":
				Move(player.GlobalPosition, delta);
				break;
		}
	}

	/*
	Calculates where player is and has enemy move toward player in a
	smooth motion.
	*/
	public void Move(Vector2 target, double delta) {
		Vector2 direction = (target - GlobalPosition).Normalized();
		Vector2 desiredVelocity = direction * SPEED;
		Vector2 steering = (desiredVelocity - Velocity) * (float)delta * 2.5f;
		Velocity += steering;
		MoveAndSlide();
	}

	/*
	Gets the circle position from around the player.
	*/
	public Vector2 GetCirclePosition(float random) {
		float x = player.GlobalPosition.X + (float)Math.Cos(random * Math.PI * 2.0f) * killRadius;
		float y = player.GlobalPosition.Y + (float)Math.Sin(random * Math.PI * 2.0f) * killRadius;
		return new Vector2(x, y);
	}

	public void OnAttackTimerTimeout() {
		this.state = "attack";
	}

	/*
	Getter for timer.
	*/
	public Timer GetAttackTimer() {
		return attackTimer;
	}

	/*
	Getter for state.
	*/
	public void SetState(string state) {
		this.state = state;
	}

}
