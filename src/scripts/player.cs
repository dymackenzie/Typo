using Godot;
using System;

public partial class player : CharacterBody2D
{
	public const float SPEED = 100.0f;
	public string playerState;

	public override void _PhysicsProcess(double delta)
	{

		Vector2 direction = Input.GetVector("a_left", "d_right", "w_up", "s_down");
		if (direction == Vector2.Zero) {
			playerState = "idle";
		} else if (direction != Vector2.Zero && direction.X > 0) {
			playerState = "run";
		} else if (direction != Vector2.Zero && direction.X < 0) {
			playerState = "run";
		}

		Velocity = direction * SPEED;
		MoveAndSlide();
		GetNode<AnimatedSprite2D>("AnimatedSprite2D").Play(playerState);
	}

}
