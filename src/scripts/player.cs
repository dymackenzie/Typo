using Godot;
using System;
using System.Linq.Expressions;
using System.Reflection.Metadata.Ecma335;

public partial class player : CharacterBody2D
{
	// movement variables
	public const float SPEED = 100.0f;
	public const float FRICTION = 0.8f;
	public const float ACCELERATION = 0.8f;
	public const float JUMP_HEIGHT = 20f;

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
		isJumping = false;
		isFalling = false;
		sprite2D = GetNode<AnimatedSprite2D>("animated player");
		hitbox = GetNode<CollisionShape2D>("hitbox");
	}

	/*
	Called every frame, "delta" is amount of time passed.
	*/
	public override void _PhysicsProcess(double delta) {

		Vector2 direction = Input.GetVector("a_left", "d_right", "w_up", "s_down");
		direction = direction.Normalized();
		float x = direction.X;
		float y = direction.Y / 2;

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

		if (Input.IsActionJustPressed("jump_space") && !isJumping) {
			isJumping = true;
			Jump();
		}

		Velocity = new Vector2(x, y);
		MoveAndSlide();
		sprite2D.Play(PLAYER_STATE);
	}

	/*
	Jump function.
	*/
	private void Jump() {

		Tween tween = CreateTween();
		tween.SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Quad);

		// jump
		sprite2D.Play("jump");
		tween.TweenProperty(sprite2D, "position", new Vector2(0, -JUMP_HEIGHT), 0.4);

		// fall
		sprite2D.Play("fall");
		tween.TweenProperty(sprite2D, "position", Vector2.Zero, 0.3);

		tween.TweenCallback(Callable.From(() => { isJumping = false; } ));

		//* disable hitboxes when jumping
		
	}

	/*
	Linear interpolation function.
	*/
	private float Lerp(float first, float second, float by) {
		return first + ((second - first) * by);
	}

}
