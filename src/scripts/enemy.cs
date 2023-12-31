using Godot;
using System;

public partial class enemy : CharacterBody2D
{

	[Export] public float SPEED = 50.0f;
	[Export] public float killRadius = 40.0f;

	public Vector2 target;
	public CharacterBody2D player = null;
	public Timer attackTimer;
	public string state;
	public float randomNum;

    public override void _Ready() {
		state = "surround";
		attackTimer = GetNode<Timer>("attack_timer");

		RandomNumberGenerator random = new();
		random.Randomize();
		randomNum = random.Randf();
    }

    public override void _PhysicsProcess(double delta) {
		switch(state) {
			case "surround":
				Move(GetCirclePosition(randomNum), (float)delta);
				break;
			case "attack":
				Move(player.GlobalPosition, (float)delta);
				break;
			case "hit":
				Move(player.GlobalPosition, (float)delta);
				break;
		}
	}

	/*
	Calculates where player is and has enemy move toward player in a
	smooth motion.
	*/
	public void Move(Vector2 target, float delta) {
		Vector2 direction = (target - GlobalPosition).Normalized();
		Vector2 desiredVelocity = direction * SPEED;
		Vector2 steering = (desiredVelocity - Velocity) * delta * 2.5f;
		Velocity += steering;
		MoveAndSlide();
	}

	/*
	Gets the circle position from around the player.
	*/
	public Vector2 GetCirclePosition(float random) {
		if (player != null) {
			float x = player.GlobalPosition.X + (float)Math.Cos(random * Math.PI * 2.0f) * killRadius;
			float y = player.GlobalPosition.Y + (float)Math.Sin(random * Math.PI * 2.0f) * killRadius;
			return new Vector2(x, y);
		} else {
			return Vector2.Zero;
		}
	}

	public void OnAttackTimerTimeout() {
		state = "attack";
	}

	public Timer GetAttackTimer() {
		return attackTimer;
	}

	public void SetState(string state) {
		this.state = state;
	}

	public void SetPlayer(CharacterBody2D player) {
		this.player = player;
	}


}
