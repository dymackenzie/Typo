using System.Runtime.InteropServices;
using Godot;

public partial class BlastEnemy : Enemy
{

	[Export] public float range = 80.0f;
	[Export] public float shootingCooldown = 1.6f;
	[Export] public float blastCooldown = 5f;

	public Timer shootingCooldownTimer;
	public Timer blastCooldownTimer;
	public Tween tween;

	private bool isAttacking = false;

	public override void _Ready() {
		base._Ready();
		shootingCooldownTimer = GetNode<Timer>("ShootingCooldown");
		blastCooldownTimer = GetNode<Timer>("BlastCooldown");

		// variable set up
		shootingCooldownTimer.WaitTime = shootingCooldown;
		blastCooldownTimer.WaitTime = blastCooldown;
	}

	public override void _PhysicsProcess(double delta) {
		// if not attacking, determine state
		if (!isAttacking)
			DetermineState();

		// deal with slowdown
		anim.SpeedScale = Globals.inSlowdown ? Globals.slowdownRate : 1;
		delta *= Globals.inSlowdown ? Globals.slowdownRate : 1;
		if (deathState)
			return;

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
					shootingCooldownTimer.Start();
				}
				anim.Play("attack");
				break;
		}
	}

	public new void OnAnimationFinished(StringName animName) {
		base.OnAnimationFinished(animName);
		if ((string) animName == "attack") {
			isAttacking = false;
		}
	}

	public void OnShootingCooldownTimeout() {
		// 
	}

	public void OnBlastCooldownTimeout() {
		// 
	}

	/*
	Override OnHit because ranged enemy has different death animation
	*/
	public new void OnHit(string s) {
		health -= healthUnit;
		EmitText(s);
		if (health <= 0) {
			OnBlastDeath();
		}
	}

	/*
	Override OnDeath because positional coords are not set correctly for sprite
	*/
	public void OnBlastDeath() {
		CollisionShape2D hitbox = GetNode<CollisionShape2D>("Hitbox");
		Sprite2D spritePos = GetNode<Sprite2D>("TruePosition");
		Tween tween = CreateTween().SetTrans(Tween.TransitionType.Quart).SetEase(Tween.EaseType.Out);
		tween.TweenProperty(prompt, "modulate:a", 0.4, 0.1); // change transparency of prompt

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
		state = distance < 20 ? EnemyState.ATTACK : state; // attack if close
	}

}
