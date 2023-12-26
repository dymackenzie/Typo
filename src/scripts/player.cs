using Godot;
using System;

public partial class player : CharacterBody2D
{
	public const float SPEED = 100.0f;
	public string playerState;

	public override void _PhysicsProcess(double delta)
	{
		AnimatedSprite2D sprite2D = GetNode<AnimatedSprite2D>("AnimatedSprite2D");

		Vector2 direction = Input.GetVector("a_left", "d_right", "w_up", "s_down");
		if (direction == Vector2.Zero) {
			playerState = "idle";
		} else if (direction != Vector2.Zero && direction.X > 0) {
			// flips the sprite horizontally
			sprite2D.FlipH = false;
			playerState = "run";
		} else if (direction != Vector2.Zero && direction.X < 0) {
			// flips the sprite horizontally
			sprite2D.FlipH = true;
			playerState = "run";
		}

		Velocity = direction * SPEED;
		MoveAndSlide();
		sprite2D.Play(playerState);
	}

}
