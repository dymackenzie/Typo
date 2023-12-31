using Godot;
using System;

public partial class enemy : CharacterBody2D
{

	[Export] public float SPEED = 50.0f;
	[Export] public float killRadius = 40.0f;

	
	public CharacterBody2D player = null;
	public Timer attackTimer;
	public AnimatedSprite2D sprite2D;

	public string state;
	public float randomNum;
	public Vector2 target;

    public override void _Ready() {
		state = "surround";
		attackTimer = GetNode<Timer>("attack_timer");
		sprite2D = GetNode<AnimatedSprite2D>("animated_enemy");

		RandomNumberGenerator random = new();
		random.Randomize();
		randomNum = random.Randf();
    }

    public override void _PhysicsProcess(double delta) {
		switch(state) {
			case "surround":
				Move(GetCirclePosition(randomNum), (float)delta);
				sprite2D.Play("walk");
				break;
			case "attack":
				Move(player.GlobalPosition, (float)delta);
				sprite2D.Play("walk");
				break;
			case "hit":
				Move(player.GlobalPosition, (float)delta);
				sprite2D.Play("attack");
				break;
		}
	}

	/*
	Calculates where player is and has enemy move toward player in a
	smooth motion.
	*/
	public void Move(Vector2 target, float delta) {
		Vector2 direction = (target - GlobalPosition).Normalized();
		float x = direction.X;
		float y = direction.Y / 2;
		Vector2 desiredVelocity = new Vector2(x, y) * SPEED;
		Vector2 steering = (desiredVelocity - Velocity) * delta * 2.5f;

		// flip enemy is needed
		if (player.GlobalPosition.X < GlobalPosition.X) {
			sprite2D.FlipH = true;
		} else {
			sprite2D.FlipH = false;
		}

		Velocity += steering;
		MoveAndCollide(Velocity * delta);
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
