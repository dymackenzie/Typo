using Godot;
using System;

public partial class player : CharacterBody2D
{
	// movement variables
	public const float SPEED = 100.0f;

	// animation variable
	public string PLAYER_STATE;

	private AnimatedSprite2D sprite2D;
	private CollisionShape2D hitbox;

    public override void _Ready()
    {
		PLAYER_STATE = "idle";
		sprite2D = GetNode<AnimatedSprite2D>("animated player");
		hitbox = GetNode<CollisionShape2D>("hitbox");
    }

    public override void _PhysicsProcess(double delta)
	{
		float x = Input.GetActionStrength("d_right") - Input.GetActionStrength("a_left");
		float y = (Input.GetActionStrength("s_down") - Input.GetActionStrength("w_up")) / 2;
		Vector2 direction = new(x, y);
		direction = direction.Normalized();

		if (Input.IsActionJustPressed("jump_space")) {
			
		}

		PLAYER_STATE = "run";
		if (direction == Vector2.Zero) {
			PLAYER_STATE = "idle";
		} else if (direction != Vector2.Zero && direction.X > 0) {
			// flips the sprite horizontally
			sprite2D.FlipH = false;
		} else if (direction != Vector2.Zero && direction.X < 0) {
			// flips the sprite horizontally
			sprite2D.FlipH = true;
		}

		Velocity = direction * SPEED;
		MoveAndSlide();
		sprite2D.Play(PLAYER_STATE);
	}

}
