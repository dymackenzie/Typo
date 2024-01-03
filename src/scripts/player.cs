using System;
using System.Buffers;
using System.Collections;
using Godot;

public partial class Player : CharacterBody2D
{
	// fields
	[Export] public float speed 		= 100.0f;
	[Export] public float dash_speed 	= 200.0f;
	[Export] public float dash_duration = 0.2f;
	[Export] public float friction 		= 0.7f;
	[Export] public float acceleration 	= 0.8f;
	[Export] public float zoom 			= 1.0f;
	[Export] public float zoomDuration 	= 0.4f;
	[Export] public Color killZone		= new(0.29f, 0.02f, 0.02f);

	// animation variable
	private bool 				inKillMode = false;
	private bool 				isTyping = false;
	private bool 				isRunning = false;

	private Globals 			globals;
	private Sprite2D 			playerSprite;
	private AnimationPlayer 	anim;
	private CharacterBody2D 	currentEnemy = null;
	private CollisionShape2D 	hitbox;
	private Node2D 				dash = null;

	// kill mode
	private readonly ArrayList 	enemies = new();
	private int 				currentLetterIndex = 0;
	private bool 				withinEnemyReach = false;

	public override void _Ready() {
		// globals
		globals = GetNode<Globals>("/root/Globals");
		playerSprite = GetNode<Sprite2D>("animated_player");
		anim = GetNode<AnimationPlayer>("animation_player");
		hitbox = GetNode<CollisionShape2D>("hitbox");
		dash = GetNode<Node2D>("dash");
	}

	/*
	Called every frame, delta is amount of time passed.
	*/
	public override void _PhysicsProcess(double delta) {

		// attack 
		if (Input.IsActionJustPressed("enter_attack") && enemies.Count != 0) {
			ResetTypingVariables();
			inKillMode = !inKillMode;
			CameraZoom(inKillMode);
		}
		// move
		if (!inKillMode) {
			Move((float)delta);
			HandleAnimations();
		} else {
			anim.Stop();
			KillMode((float)delta);
		}

	}

	/*
	Kill mode to handle fighting mechanics
	*/
	public void KillMode(float delta) {
		currentEnemy = (CharacterBody2D)enemies[0];
		if ((currentEnemy.GlobalPosition - GlobalPosition).Length() > 20.0f) {
			// first, dash to enemy
			dash.Call("InstanceGhost");
			Vector2 direction = (currentEnemy.GlobalPosition - GlobalPosition).Normalized();
			Vector2 desiredVelocity = new Vector2(direction.X, direction.Y / 2) * dash_speed;
			MoveAndCollide(desiredVelocity * delta * 3f);
		} else {
			isTyping = true;
		}
	}

    public override void _UnhandledInput(InputEvent @event)
    {
        if (isTyping) {
			if (@event is InputEventKey key && !@event.IsPressed()) {
				InputEventKey typedEvent = key;
				string keyTyped = typedEvent.AsTextKeycode();
				// // handle enter case
				// if (keyTyped == "Enter")
				// 	return;
				// start typing
				if (currentEnemy != null) {
					string prompt = (string)currentEnemy.Call("GetPrompt");
					string nextChar = prompt.Substr(currentLetterIndex, 1);
					// if word matches
					if (keyTyped == nextChar) {
						currentLetterIndex += 1;
						currentEnemy.Call("OnHit");
						// change string to match progress
						currentEnemy.Call("SetNextCharacter", currentLetterIndex, false);
						// when word is finished
						if (currentLetterIndex == prompt.Length) {
							ResetPrompt();
						}
					} else {
						// change string to match wrong letter
						currentEnemy.Call("SetNextCharacter", currentLetterIndex, true);
					}
				}
			}
		}
    }

	/*
	Is player finished typing the prompt?
	*/
	private void ResetPrompt() {
		// reset letter index, is typing, and withinEnemyReach after prompt
		ResetTypingVariables();
		enemies.Remove(currentEnemy);
		// check if player has gone through all enemies
		if (enemies.Count == 0) {
			// all enemies have been wiped
			CameraZoom(inKillMode = false);
		}
	}

	/*
	Reset typing variables
	*/
	private void ResetTypingVariables() {
		currentLetterIndex = 0;
		isTyping = withinEnemyReach = false;
	}

	/*
	Zomm in camera and zoom out
	*/
	private void CameraZoom(bool zoomIn) {
		Camera2D camera2D = GetNode<Camera2D>("animated_player/Camera2D");
		Tween tween = CreateTween().SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.Out);
		// emit slowdown signal
		globals.isInSlowdown = !globals.isInSlowdown;
		if (zoomIn) {
			// zoom in to player
			tween.TweenProperty(camera2D, "zoom", camera2D.Zoom + new Vector2(zoom, zoom), zoomDuration);
		} else {
			tween.TweenProperty(camera2D, "zoom", camera2D.Zoom - new Vector2(zoom, zoom), zoomDuration);
		}
	}

	/*
	Handles all animations
	*/
	private void HandleAnimations() {
		if ((bool)dash.Call("IsDashing")) {
			anim.Play("dash");
		} else if (isRunning) {
			anim.Play("run");
		} else {
			anim.Play("idle");
		}
	}

	/*
	Function to handle 8-directional movement.
	*/
	public void Move(float delta) {
		// grab input vector from WASD
		Vector2 direction = Input.GetVector("a_left", "d_right", "w_up", "s_down");
		direction = direction.Normalized();
		float x = direction.X;
		float y = direction.Y / 2;

		if (direction == Vector2.Zero) {
			// slowdown when no input
			x = Lerp(Velocity.X, 0, friction);
			y = Lerp(Velocity.Y, 0, friction);
			isRunning = false;
		} else if (direction != Vector2.Zero) {
			// dash
			if (Input.IsActionJustPressed("dash") && !(bool)dash.Call("IsDashing")) {
				dash.Call("StartDash", playerSprite, dash_duration);
			}
			float speed = (bool)dash.Call("IsDashing") ? dash_speed : this.speed;

			if (x > 0) {
				// if player is going right
				// flips the sprite horizontally
				playerSprite.FlipH = false;
			} else {
				// if player is going left
				// flips the sprite horizontally
				playerSprite.FlipH = true;
			}

			// accelerates when input
			x = Lerp(Velocity.X, x * speed, acceleration);
			y = Lerp(Velocity.Y, y * speed, acceleration);
			isRunning = true;
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
		if (body.IsInGroup("enemy") ) {
			body.Call("SetState", "surround");
		}
	}

	/*
	Enemy enters attacking area.
	*/
	public void OnAttackBodyEntered(Node2D body) {
		if (body.IsInGroup("enemy")) {
			Timer timer = (Timer)body.Call("GetAttackTimer");
			body.Call("SetPositionColor", killZone);
			enemies.Add((CharacterBody2D)body);
			timer.Start();
		}
	}

	/*
	Enemy exits attacking area.
	*/
	public void OnAttackBodyExited(Node2D body) {
		if (body.IsInGroup("enemy")) {
			Timer timer = (Timer)body.Call("GetAttackTimer");
			body.Call("SetPositionColor", new Color(1, 1, 1));
			body.Call("SetState", "surround");
			enemies.Remove((CharacterBody2D)body);
			timer.Stop();
		}
	}

	/*
	Linear interpolation function.
	*/
	private static float Lerp(float first, float second, float by) {
		return first + ((second - first) * by);
	}

}
