using System;
using Godot;

public partial class RangedEnemy : Enemy
{

	[Export] public float range = 100.0f;
	[Export] public float shootingCooldown = 0.5f;
	[Export] public float shootingCooldown2 = 2.2f;

	public FireballGenerator fireballs;
	public Timer shootingCooldownTimer;
	public Timer shootingCooldownTimer2;

	private bool isAttacking = false;
	private bool attackFinished = true;

	public override void _Ready() {
		base._Ready();
		fireballs = GetNode<FireballGenerator>("FireballGenerator");
		shootingCooldownTimer = GetNode<Timer>("RangedShootingCooldown");
		shootingCooldownTimer2 = GetNode<Timer>("RangedShootingCooldown2");

		// variable set up
		shootingCooldownTimer.WaitTime = shootingCooldown;
		shootingCooldownTimer2.WaitTime = shootingCooldown2;
	}

	public override void _PhysicsProcess(double delta) {
		// if not attacking, determine state
		if (!isAttacking)
			DetermineState();

		// deal with slowdown
		anim.SpeedScale = Globals.InSlowdown ? Globals.SlowdownRate : 1;
		delta *= Globals.InSlowdown ? Globals.SlowdownRate : 1;
		if (deathState)
			return;

		// flip sprite based on player position
		if (player.GlobalPosition.X < GlobalPosition.X) {
			sprite2D.FlipH = true;
			fireballs.Position = new Vector2(-8, 0);
		} else {
			sprite2D.FlipH = false;
			fireballs.Position = new Vector2(8, 0);
		}

		switch(state) {
			case EnemyState.SURROUND:
				Move(player.GlobalPosition, (float)delta);
				anim.Play("walk");
				break;
			case EnemyState.ATTACK:
			case EnemyState.HIT:
			case EnemyState.SHOOT:
				if (!isAttacking) {
					isAttacking = true;
					fireballs.GenerateFireball();
					shootingCooldownTimer2.Start();
					attackFinished = false;
				}
				anim.Play("attack");
				break;
		}

		if (anim.CurrentAnimation == "" ||
			(anim.CurrentAnimation == "walk" && state == EnemyState.SHOOT) ||
			(state == EnemyState.SHOOT && attackFinished)) {
			anim.Play("idle");
		}
	}

	public new void OnAnimationFinished(StringName animName) {
		base.OnAnimationFinished(animName);
		if ((string) animName == "attack") {
			attackFinished = true;
		}
	}

	// public void _OnRangedShootingCooldown() {
	// 	fireballs.GenerateFireball();
	// }

	public void _OnRangedShootingCooldown2() {
		isAttacking = false;
	}

	/*
	Override OnHit because ranged enemy has different death animation
	*/
	public new void OnHit(string s) {
		health -= healthUnit;
		EmitText(s);
		if (health <= 0 && hasShield) {
			int shieldBuff = 2;
			SetPrompt(difficulty - shieldBuff);
			currentLetterIndex = 0;
			health = (difficulty - shieldBuff) * healthUnit;
		}
		if (health <= 0) {
			OnRangedDeath();
		}
	}

	/*
	Override OnDeath because positional coords are not set correctly for sprite
	*/
	public void OnRangedDeath() {
		CollisionShape2D hitbox = GetNode<CollisionShape2D>("Hitbox");
		Sprite2D spritePos = GetNode<Sprite2D>("TruePosition");
		Tween tween = CreateTween().SetTrans(Tween.TransitionType.Quart).SetEase(Tween.EaseType.Out);
		tween.TweenProperty(prompt, "modulate:a", 0.4, 0.1); // change transparency of prompt

		prompt.ZIndex = -10;
		hitbox.Disabled = deathState = true;
		spritePos.Visible = false;
		anim.Play("death"); // play animation
	}

	/*
	Determine state of ranged enemy
	*/
	public void DetermineState() {
		float distance = (player.GlobalPosition - GlobalPosition).Length();
		state = distance < range ? EnemyState.SHOOT : EnemyState.SURROUND; // shoot or surround
	}

}
