using Godot;
using System;
using System.Linq.Expressions;
using System.Reflection.Metadata.Ecma335;

public partial class player : CharacterBody2D
{
	// fields
	public const float SPEED = 100.0f;
	public const float DASH_SPEED = 200.0f;
	public const float DASH_DURATION = 0.2f;
	public const float FRICTION = 0.7f;
	public const float ACCELERATION = 0.8f;
	public const float JUMP_HEIGHT = 20f;

	// animation variable
	private string PLAYER_STATE;
	private bool isJumping;
	private bool isFalling;

	private AnimatedSprite2D sprite2D;
	private CollisionShape2D hitbox;
	private Node2D dash;

	/*
	Constructor
	*/
	public override void _Ready()
	{
		PLAYER_STATE = "run";
		isJumping = false;
		isFalling = false;
		sprite2D = GetNode<AnimatedSprite2D>("animated player");
		hitbox = GetNode<CollisionShape2D>("hitbox");
		dash = GetNode<Node2D>("dash");
	}

	/*
	Called every frame, delta is amount of time passed.
	*/
	public override void _PhysicsProcess(double delta) {

		// dash
		if (Input.IsActionJustPressed("dash") && !(bool)dash.Call("IsDashing")) {
			dash.Call("StartDash", sprite2D, DASH_DURATION);
		}

		float speed = (bool)dash.Call("IsDashing") ? DASH_SPEED : SPEED;

		// jumping
		if (Input.IsActionPressed("jump_space") && !isJumping) {
			hitbox.Disabled = true;
			isJumping = true;
			Jump();
		}

		// move
		Move(speed);
		
		sprite2D.Play(PLAYER_STATE);
		
	}

	/*
	Function to handle 8-directional movement.
	*/
	private void Move(float speed) {

		Vector2 direction = Input.GetVector("a_left", "d_right", "w_up", "s_down");
		direction = direction.Normalized();
		float x = direction.X;
		float y = direction.Y / 2;

		if (direction == Vector2.Zero) {
			// slowdown when no input
			x = Lerp(Velocity.X, 0, FRICTION);
			y = Lerp(Velocity.Y, 0, FRICTION);
			PLAYER_STATE = "idle";
		} else if (direction != Vector2.Zero) {
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
			x = Lerp(Velocity.X, x * speed, ACCELERATION);
			y = Lerp(Velocity.Y, y * speed, ACCELERATION);
			PLAYER_STATE = "run";
		}
		
		Velocity = new Vector2(x, y);
		MoveAndSlide();

	}

	/*
	Jump function.
	*/
	private void Jump() {
		Tween tween = CreateTween().SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Quad);

		// jump
		sprite2D.Play("jump");
		tween.TweenProperty(sprite2D, "position", new Vector2(0, -JUMP_HEIGHT), 0.4);

		// fall
		sprite2D.Play("fall");
		tween.TweenProperty(sprite2D, "position", Vector2.Zero, 0.3);

		// after animations, allow jumping and enable hitboxes
		tween.TweenCallback(Callable.From(() => { isJumping = false; hitbox.Disabled = false; } ));
	}

	/*
	Linear interpolation function.
	*/
	private float Lerp(float first, float second, float by) {
		return first + ((second - first) * by);
	}

}
