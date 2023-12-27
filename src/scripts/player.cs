using Godot;
using System;

public partial class player : CharacterBody2D
{
	// movement variables
	[Export]
	public const float SPEED = 100.0f;
	public const float JUMP_HEIGHT = 0f;
	public const float JUMP_TIME_TO_PEAK = 0f;
	public const float JUMP_TIME_TO_DESCENT = 0f;

	private float jumpVelocity = (-2.0f * JUMP_HEIGHT) / JUMP_TIME_TO_PEAK;
	private float jumpGravity = (2.0f * JUMP_HEIGHT) / (JUMP_TIME_TO_PEAK * JUMP_TIME_TO_PEAK);
	private float fallGravity = (2.0f * JUMP_HEIGHT) / (JUMP_TIME_TO_DESCENT * JUMP_TIME_TO_DESCENT);

	// animation variable
	public string PLAYER_STATE;

	public override void _PhysicsProcess(double delta)
	{
		AnimatedSprite2D sprite2D = GetNode<AnimatedSprite2D>("AnimatedSprite2D");

		

		Vector2 direction = Input.GetVector("a_left", "d_right", "w_up", "s_down");
		if (direction == Vector2.Zero) {
			PLAYER_STATE = "idle";
		} else if (direction != Vector2.Zero && direction.X > 0) {
			// flips the sprite horizontally
			sprite2D.FlipH = false;
			PLAYER_STATE = "run";
		} else if (direction != Vector2.Zero && direction.X < 0) {
			// flips the sprite horizontally
			sprite2D.FlipH = true;
			PLAYER_STATE = "run";
		}

		Velocity = direction * SPEED;
		MoveAndSlide();
		sprite2D.Play(PLAYER_STATE);
	}

	/**
	* Returns either jump gravity or fall gravity depending on y-velocity
	*/
	private float getGravity() {
		return Velocity.Y < 0.0 ? jumpGravity : fallGravity;
	}

}
