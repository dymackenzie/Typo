using Godot;
using System;

public partial class player : CharacterBody2D
{
	// movement variables
	public const float SPEED = 100.0f;
	public const float FRICTION = 0.8f;
	public const float ACCELERATION = 0.8f;


	// animation variable
	public string PLAYER_STATE;
	private bool isJumping;
	private bool isFalling;

	private AnimatedSprite2D sprite2D;
	private CollisionShape2D hitbox;

	/*
	Constructor
	*/
    public override void _Ready()
    {
		PLAYER_STATE = "idle";
		sprite2D = GetNode<AnimatedSprite2D>("animated player");
		hitbox = GetNode<CollisionShape2D>("hitbox");
    }

	/*
	Called every frame, "delta" is amount of time passed.
	*/
    public override void _PhysicsProcess(double delta)
	{
		Vector2 direction = Input.GetVector("a_left", "d_right", "w_up", "s_down");
		direction = direction.Normalized();
		float x = direction.X;
		float y = direction.Y / 2;

		if (Input.IsActionJustPressed("jump_space")) {

		}

		PLAYER_STATE = "run";
		if (x == 0 && y == 0) {
			// slowdown when no input
			x = Lerp(Velocity.X, 0, FRICTION);
			y = Lerp(Velocity.Y, 0, FRICTION);
			PLAYER_STATE = "idle";
		} else if (x != 0 || y != 0) {
			if (x > 0) {
				// if player is going right
				// flips the sprite horizontally
				sprite2D.FlipH = false;
			} else {
				// if player is going left
				// flips the sprite horizontally
				sprite2D.FlipH = true;
			}
			// accelerates when input
			x = Lerp(Velocity.X, x * SPEED, ACCELERATION);
			y = Lerp(Velocity.Y, y * SPEED, ACCELERATION);
		}

		Velocity = new Vector2(x, y);
		MoveAndSlide();
		sprite2D.Play(PLAYER_STATE);
	}

	/*
	Linear interpolation function.
	*/
	private float Lerp(float first, float second, float by) {
		return first + ((second - first) * by);
	}

}
