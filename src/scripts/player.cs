using System.Collections;
using Godot;

public partial class player : CharacterBody2D
{
	// fields
	[Export] public float speed = 100.0f;
	[Export] public float dash_speed = 200.0f;
	[Export] public float dash_duration = 0.2f;
	[Export] public float friction = 0.7f;
	[Export] public float acceleration = 0.8f;
	[Export] public float zoom = 1.0f;
	[Export] public float zoomDuration = 0.4f;
	// [Export] public float jump_height = 20f;

	// animation variable
	private string playerState = "idle";
	// private bool isJumping = false;
	// private bool isFalling = false;
	private bool inKillMode = false;

	private AnimatedSprite2D sprite2D;
	private CollisionShape2D hitbox;
	private Node2D dash;

	// enemies to attack
	public ArrayList enemies = new();

	public override void _Ready()
	{
		sprite2D = GetNode<AnimatedSprite2D>("animated_player");
		hitbox = GetNode<CollisionShape2D>("hitbox");
		dash = GetNode<Node2D>("dash");
	}

	/*
	Called every frame, delta is amount of time passed.
	*/
	public override void _PhysicsProcess(double delta) {

		// move
		if (!inKillMode) {
			Move((float)delta);
		}

		// attack
		if (Input.IsActionJustPressed("enter_attack")) {
			inKillMode = true;
			KillMode();
		}

		// // jumping
		// if (Input.IsActionPressed("jump_space") && !isJumping) {
		// 	hitbox.Disabled = true;
		// 	isJumping = true;
		// 	Jump();
		// }

		sprite2D.Play(playerState);
		
	}

	private void KillMode() {
		Camera2D camera2D = GetNode<Camera2D>("animated_player/Camera2D");
		Tween tween = CreateTween().SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.Out);
		tween.TweenProperty(camera2D, "zoom", camera2D.Zoom + new Vector2(zoom, zoom), zoomDuration);
		foreach (CharacterBody2D enemy in enemies) {
			
		}
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
			x = Lerp(Velocity.X, 0, friction);
			y = Lerp(Velocity.Y, 0, friction);
			playerState = "idle";
		} else if (direction != Vector2.Zero) {

			// dash
			if (Input.IsActionJustPressed("dash") && !(bool)dash.Call("IsDashing")) {
				dash.Call("StartDash", sprite2D, dash_duration);
			}
			float speed = (bool)dash.Call("IsDashing") ? dash_speed : this.speed;

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
			x = Lerp(Velocity.X, x * speed, acceleration);
			y = Lerp(Velocity.Y, y * speed, acceleration);
			playerState = "run";
		}
		
		Velocity = new Vector2(x, y);
		MoveAndCollide(Velocity * delta);

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
	Enemy enters attacking area.
	*/
	public void OnAttackBodyEntered(Node2D body) {
		if (body.IsInGroup("enemy")) {
			Timer timer = (Timer)body.Call("GetAttackTimer");
			timer.Start();

			// CharacterBody2D enemy = (CharacterBody2D)body;
			Sprite2D spritePos = body.GetNode<Sprite2D>("position");
			// change sprite color
			spritePos.Modulate = new Color(0.29f, 0.02f, 0.02f); // dark red

			enemies.Add((CharacterBody2D)body);
		}
	}

	/*
	Enemy exits attacking area.
	*/
	public void OnAttackBodyExited(Node2D body) {
		if (body.IsInGroup("enemy")) {
			Timer timer = (Timer)body.Call("GetAttackTimer");
			timer.Stop();
			body.Call("SetState", "surround");

			// CharacterBody2D enemy = (CharacterBody2D)body;
			Sprite2D spritePos = body.GetNode<Sprite2D>("position");
			// change sprite color
			spritePos.Modulate = new Color(1, 1, 1); // white

			enemies.Remove((CharacterBody2D)body);
		}
	}

	/*
	Linear interpolation function.
	*/
	private static float Lerp(float first, float second, float by) {
		return first + ((second - first) * by);
	}

	// /*
	// Jump function.
	// */
	// private void Jump() {
	// 	Tween tween = CreateTween().SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Quad);
	// 	// jump
	// 	tween.TweenProperty(sprite2D, "position", new Vector2(0, -jump_height), 0.4);
	// 	// fall
	// 	tween.TweenProperty(sprite2D, "position", Vector2.Zero, 0.3);
	// 	// after animations, allow jumping and enable hitboxes
	// 	tween.TweenCallback(Callable.From(() => { isJumping = false; hitbox.Disabled = false; } ));
	// }

}
