using System;
using System.Buffers;
using System.Collections;
using Godot;

public partial class Player : CharacterBody2D
{

	[Signal] public delegate void CameraShakeRequestedEventHandler();
	[Signal] public delegate void HealthChangedEventHandler();
	[Signal] public delegate void InSlowdownEventHandler();

	// fields
	[Export] public float speed 		= 100.0f;
	[Export] public float dash_speed 	= 200.0f;
	[Export] public float dash_duration = 0.2f;
	[Export] public float friction 		= 0.7f;
	[Export] public float acceleration 	= 0.8f;
	[Export] public Color killZone		= new(0.29f, 0.02f, 0.02f);
	[Export] public int health			= 6;

	// animation variable
	private bool 				inKillMode = false;
	private bool 				isTyping = false;
	private bool 				isRunning = false;
	private bool				isAttacking = false;

	private Globals 			globals;
	private Sprite2D 			playerSprite;
	private AnimationPlayer 	anim;
	private ShakingCamera		camera;
	private Enemy 				currentEnemy = null;
	private Dash 				dash = null;

	// kill mode
	private readonly ArrayList 	enemies = new();
	private bool 				withinEnemyReach = false;

	public override void _Ready() {
		// globals
		globals = GetNode<Globals>("/root/Globals");
		playerSprite = GetNode<Sprite2D>("animated_player");
		camera = GetNode<ShakingCamera>("animated_player/ShakingCamera");
		anim = GetNode<AnimationPlayer>("animation_player");
		dash = GetNode<Dash>("Dash");
	}

	/*
	Called every frame, delta is amount of time passed.
	*/
	public override void _PhysicsProcess(double delta) {
		// attack 
		if (Input.IsActionJustPressed("enter_attack") && enemies.Count != 0) {
			currentEnemy = (Enemy)enemies[0];
			isTyping = withinEnemyReach = false;
			camera.CameraZoom(!inKillMode);
		}
		// move
		if (!inKillMode) {
			Move((float)delta);
		} else {
			KillMode((float)delta);
		}
		HandleAnimations();
	}

	/*
	Kill mode to handle fighting mechanics
	*/
	public void KillMode(float delta) {
		currentEnemy = (Enemy)enemies[0];
		currentEnemy.SetPromptVisibility(true);
		if ((currentEnemy.GlobalPosition - GlobalPosition).Length() > 20.0f) {
			// first, dash to enemy
			dash.InstanceGhost();
			Vector2 direction = (currentEnemy.GlobalPosition - GlobalPosition).Normalized();
			Vector2 desiredVelocity = new Vector2(direction.X, direction.Y / 2) * dash_speed;
			MoveAndCollide(desiredVelocity * delta);
		} else {
			isTyping = true;
		}
	}

    public override void _UnhandledInput(InputEvent @event) {
        if (isTyping) {
			if (@event is InputEventKey key && !@event.IsPressed()) {
				InputEventKey typedEvent = key;
				string keyTyped = typedEvent.AsTextKeycode();
				if (currentEnemy != null) {
					Typing(keyTyped);
				}
			}
		}
    }

	/*
	Handling all typing and wrong letters
	*/
	private void Typing(string keyTyped) {
		int currentLetterIndex = currentEnemy.currentLetterIndex;
		string prompt = currentEnemy.GetPrompt();
		string nextChar = prompt.Substr(currentLetterIndex, 1);
		// IF STRING MATCHES, DEAL DAMAGE AND ALL THAT STUFF
		if (keyTyped == nextChar) {
			currentLetterIndex += 1;
			isAttacking = true;
			currentEnemy.currentLetterIndex = currentLetterIndex;
			currentEnemy.OnHit();
			currentEnemy.SetNextCharacter(false);
			EmitSignal(nameof(CameraShakeRequested));
			if (currentLetterIndex == prompt.Length) {
				ResetPrompt(); // when player has gone through word
			}
		} else {
			isAttacking = false;
			currentEnemy.SetNextCharacter(true);
		}
	}

	/*
	Is player finished typing the prompt?
	*/
	private void ResetPrompt() {
		// reset letter index, is typing, and withinEnemyReach after prompt
		currentEnemy.currentLetterIndex = 0;
		isTyping = withinEnemyReach = isAttacking = false;
		enemies.Remove(currentEnemy);
		// check if player has gone through all enemies
		if (enemies.Count == 0) {
			// all enemies have been wiped
			camera.CameraZoom(inKillMode = false);
		}
	}
	

	/*
	Handles all animations
	*/
	private void HandleAnimations() {
		if (!inKillMode) {
			if (dash.IsDashing()) {
				anim.Play("dash");
			} else if (isRunning) {
				anim.Play("run");
			} else {
				anim.Play("idle");
			}
		} else {
			if (isAttacking) {
				anim.Play("attack_sweep");
			}
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
				dash.StartDash(playerSprite, dash_duration);
			}
			float speed = dash.IsDashing() ? dash_speed : this.speed;
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
			Enemy enemy = (Enemy) body;
			enemy.attackTimer.Stop();
			enemy.SetState("hit");
		}
	}

	/*
	If enemy exits hitting area, set state of enemy to surround.
	*/
	public void OnHitBodyExited(Node2D body) {
		if (body.IsInGroup("enemy") ) {
			Enemy enemy = (Enemy) body;
			enemy.SetState("surround");
		}
	}

	/*
	Enemy enters attacking area.
	*/
	public void OnAttackBodyEntered(Node2D body) {
		if (body.IsInGroup("enemy")) {
			Enemy enemy = (Enemy) body;
			enemy.attackTimer.Start();
			enemy.SetPositionColor(killZone);
			enemies.Add(enemy);
		}
	}

	/*
	Enemy exits attacking area.
	*/
	public void OnAttackBodyExited(Node2D body) {
		if (body.IsInGroup("enemy")) {
			Enemy enemy = (Enemy) body;
			enemy.attackTimer.Stop();
			enemy.SetPositionColor(Colors.White);
			enemy.SetState("surround");
			enemies.Remove(enemy);
		}
	}

	/*
	Linear interpolation function.
	*/
	private static float Lerp(float first, float second, float by) {
		return first + ((second - first) * by);
	}

}
