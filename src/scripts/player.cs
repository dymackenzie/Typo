using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Godot;

public partial class Player : CharacterBody2D
{

	[Signal] public delegate void CameraShakeRequestedEventHandler();
	[Signal] public delegate void HealthChangedEventHandler();
	[Signal] public delegate void InSlowdownEventHandler(bool slowdown);
	[Signal] public delegate void WPMChangedEventHandler(double WPM);

	// fields
	[Export] public float speed 		= 100.0f;
	[Export] public float dash_speed 	= 200.0f;
	[Export] public float dash_duration = 0.2f;
	[Export] public float friction 		= 0.7f;
	[Export] public float acceleration 	= 0.8f;
	[Export] public Color killZone		= new(0.29f, 0.02f, 0.02f);
	[Export] public int health			= 6;

	// animation variable
	public bool 				inKillMode = false;
	public bool 				isTyping = false;
	public bool 				isRunning = false;
	public bool					isDamage = false;
	public bool					isAttacking = false;
	public bool					isBeingDamaged = false;

	public Sprite2D 			playerSprite;
	public CollisionShape2D		hitbox;
	public AnimationPlayer 		anim;
	public ShakingCamera		camera;
	public Enemy 				currentEnemy = null;
	public Dash 				dash = null;
	public Timer				shield;

	// kill mode
	public List<Enemy> 			enemies = new();
	public bool 				withinEnemyReach = false;

	// WPM
	public ulong 				timeStart;
	public double				WPM = 0;

	public override void _Ready() {
		playerSprite = GetNode<Sprite2D>("Sprite");
		hitbox = GetNode<CollisionShape2D>("Hitbox");
		shield = GetNode<Timer>("ShieldDelay");
		camera = GetNode<ShakingCamera>("Sprite/ShakingCamera");
		anim = GetNode<AnimationPlayer>("AnimationPlayer");
		dash = GetNode<Dash>("Dash");
	}

	/*
	Called every frame, delta is amount of time passed.
	*/
	public override void _PhysicsProcess(double delta) {
		// attack 
		if (Input.IsActionJustPressed("enter_attack") && enemies.Count != 0) {
			anim.Stop();
			currentEnemy = enemies[0];
			isTyping = withinEnemyReach = false;
			playerSprite.Frame = 40;
			hitbox.Disabled = true;
			EmitSignal(nameof(InSlowdown), true);
			camera.CameraZoom(inKillMode = !inKillMode);
		}
		// move
		if (!inKillMode) {
			Move((float)delta);
		} else if (currentEnemy != null) {
			KillMode((float)delta);
		}
		HandleAnimations();
	}

	/*
	Kill mode to handle fighting mechanics
	*/
	public void KillMode(float delta) {
		currentEnemy.prompt.Visible = true;
		if ((currentEnemy.GlobalPosition - GlobalPosition).Length() > 15.0f) {
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
		if (currentLetterIndex == 0)
			timeStart = Time.GetTicksMsec();
		// IF STRING MATCHES, DEAL DAMAGE AND ALL THAT STUFF
		if (keyTyped == nextChar) {
			currentLetterIndex += 1;
			currentEnemy.currentLetterIndex = currentLetterIndex;
			currentEnemy.OnHit(nextChar);
			currentEnemy.SetNextCharacter(false);
			EmitSignal(nameof(CameraShakeRequested));
			if (currentLetterIndex == prompt.Length) {
				ResetPrompt(); // when player has gone through word
			}
		} else {
			currentEnemy.SetNextCharacter(true);
		}
	}

	/*
	Is player finished typing the prompt?
	*/
	private void ResetPrompt() {
		// reset letter index, is typing, and withinEnemyReach after prompt
		currentEnemy.currentLetterIndex = 0;
		isTyping = withinEnemyReach = false;
		CalculateWPM();
		AttackAnimation();
		currentEnemy = null;
	}

	/*
	Calculates WPM from typing
	*/
	public void CalculateWPM() {
		ulong timePassed = Time.GetTicksMsec() - timeStart;
		double seconds = timePassed / 60000.0;
		double words = currentEnemy.difficulty / 5.0;
		if (WPM == 0)
			WPM = words / seconds;
		else
			WPM = (WPM + words / seconds) / 2;
		EmitSignal(nameof(WPMChanged), WPM);
	}

	public void OnDamage() {
		health -= 1;
		EmitSignal(nameof(HealthChanged));
		isDamage = true;
		shield.Start();
	}

	/*
	Handles attack animation
	*/
	private void AttackAnimation() {
		string[] animations = new string[] {"attack_sweep", "attack_swoop", "attack_up", "attack_down"};
		RandomNumberGenerator random = new();
		anim.Play(animations[random.RandiRange(0, 3)]);
		currentEnemy.EmitText((currentEnemy.difficulty * currentEnemy.healthUnit).ToString());
	}

	public void OnAnimationFinished(StringName animName) {
		string[] animations = new string[] {"attack_sweep", "attack_swoop", "attack_up", "attack_down"};
		if (animations.Contains<string>((string) animName)) {
			// check if player has gone through all enemies
			enemies.Remove(currentEnemy);
			if (enemies.Count == 0) {
				// all enemies have been wiped
				hitbox.Disabled = false;
				EmitSignal(nameof(InSlowdown), false);
				camera.CameraZoom(inKillMode = false);
			} else {
				playerSprite.Frame = 40;
				currentEnemy = enemies[0];
			}	
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
			} else if (isDamage) {
				isDamage = false;
				anim.Play("damage");
			} else {
				anim.Play("idle");
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
			if (Input.IsActionJustPressed("dash") && !dash.IsDashing()) {
				dash.StartDash(dash_duration);
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
