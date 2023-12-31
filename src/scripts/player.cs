using Godot;

public partial class player : CharacterBody2D
{
	// fields
	[Export] public float SPEED = 100.0f;
	[Export] public float DASH_SPEED = 200.0f;
	[Export] public float DASH_DURATION = 0.2f;
	[Export] public float FRICTION = 0.7f;
	[Export] public float ACCELERATION = 0.8f;
	[Export] public float JUMP_HEIGHT = 20f;

	// animation variable
	private string playerState;
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
		playerState = "idle";
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

		// move
		Move((float)delta);

		// // jumping
		// if (Input.IsActionPressed("jump_space") && !isJumping) {
		// 	hitbox.Disabled = true;
		// 	isJumping = true;
		// 	Jump();
		// }

		sprite2D.Play(playerState);
		
	}

	/*
	Function to handle 8-directional movement.
	*/
	private void Move(float delta) {

		Vector2 direction = Input.GetVector("a_left", "d_right", "w_up", "s_down");
		direction = direction.Normalized();
		float x = direction.X;
		float y = direction.Y / 2;

		if (direction == Vector2.Zero) {
			// slowdown when no input
			x = Lerp(Velocity.X, 0, FRICTION);
			y = Lerp(Velocity.Y, 0, FRICTION);
			playerState = "idle";
		} else if (direction != Vector2.Zero) {

			// dash
			if (Input.IsActionJustPressed("dash") && !(bool)dash.Call("IsDashing")) {
				dash.Call("StartDash", sprite2D, DASH_DURATION);
			}
			float speed = (bool)dash.Call("IsDashing") ? DASH_SPEED : SPEED;

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
			playerState = "run";
		}
		
		Velocity = new Vector2(x, y);
		MoveAndCollide(Velocity * delta);

	}

	/*
	Jump function.
	*/
	private void Jump() {
		Tween tween = CreateTween().SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Quad);
		// jump
		tween.TweenProperty(sprite2D, "position", new Vector2(0, -JUMP_HEIGHT), 0.4);
		// fall
		tween.TweenProperty(sprite2D, "position", Vector2.Zero, 0.3);
		// after animations, allow jumping and enable hitboxes
		tween.TweenCallback(Callable.From(() => { isJumping = false; hitbox.Disabled = false; } ));
	}

	/*
	Linear interpolation function.
	*/
	private static float Lerp(float first, float second, float by) {
		return first + ((second - first) * by);
	}

	/*
	If enemy enters hitting area, set state of enemy to hit.
	*/
	public void OnHitBodyEntered(Node2D body) {
		if (body.IsInGroup("enemy")) {
			Timer timer = (Timer)body.Call("GetAttackTimer");
			timer.Stop();
			body.Call("SetState", "hit");
		}
	}

	/*
	If enemy exits hitting area, set state of enemy to surround.
	*/
	public void OnHitBodyExited(Node2D body) {
		if (body.IsInGroup("enemy")) {
			Timer timer = (Timer)body.Call("GetAttackTimer");
			timer.Start();
			body.Call("SetState", "surround");
		}
	}

	/*
	If enemy enters attacking area, start timer.
	*/
	public void OnAttackBodyEntered(Node2D body) {
		if (body.IsInGroup("enemy")) {
			Timer timer = (Timer)body.Call("GetAttackTimer");
			timer.Start();
		}
	}

	/*
	If enemy exits attacking area, stop timer and set state to surround.
	*/
	public void OnAttackBodyExited(Node2D body) {
		if (body.IsInGroup("enemy")) {
			Timer timer = (Timer)body.Call("GetAttackTimer");
			timer.Stop();
			body.Call("SetState", "surround");
		}
	}

}
