using Godot;
using System;

public partial class enemy : CharacterBody2D
{

	public enum EnemyState {
		SURROUND,
		ATTACK,
		HIT
	}

	[Export] public float speed = 50.0f;
	[Export] public float killRadius = 40.0f;
	[Export] public float health = 100.0f;

	public CharacterBody2D player = null;
	public Timer attackTimer;
	public AnimatedSprite2D sprite2D;
	public EnemyState state = EnemyState.SURROUND;

	public RandomNumberGenerator random = new();

    public override void _Ready() {
		attackTimer = GetNode<Timer>("attack_timer");
		sprite2D = GetNode<AnimatedSprite2D>("animated_enemy");
		random.Randomize();
    }

    public override void _PhysicsProcess(double delta) {
		switch(state) {
			case EnemyState.SURROUND:
				Move(GetCirclePosition(random.Randf()), (float)delta);
				sprite2D.Play("walk");
				break;
			case EnemyState.ATTACK:
				Move(player.GlobalPosition, (float)delta);
				sprite2D.Play("walk");
				break;
			case EnemyState.HIT:
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
		Vector2 desiredVelocity = new Vector2(x, y) * speed;
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

	/*
	When the attack timer runs out, have enemies attack 
	*/
	public void OnAttackTimerTimeout() {
		state = EnemyState.ATTACK;
	}

	public Timer GetAttackTimer() {
		return attackTimer;
	}

	public void SetState(string state) {
		if (state == "surround") {
			this.state = EnemyState.SURROUND;
		} else if (state == "attack") {
			this.state = EnemyState.ATTACK;
		} else if (state == "hit") {
			this.state = EnemyState.HIT;
		}
	}

	public void SetPlayer(CharacterBody2D player) {
		this.player = player;
	}


}
