using System.Runtime.InteropServices;
using Godot;

public partial class BlastEnemy : Enemy
{

	[Export] public float range = 120.0f;
	[Export] public float shootingCooldown = 0.5f;
	[Export] public float blastCooldown = 10.0f;

	public Timer shootingCooldownTimer;
	public Timer blastCooldownTimer;
	public BlastArea blastArea;
	public PackedScene blast;
	public Tween tween;

	private bool isAttacking = false;

	public override void _Ready() {
		base._Ready();
		shootingCooldownTimer = GetNode<Timer>("ShootingCooldown");
		blastCooldownTimer = GetNode<Timer>("BlastCooldown");
		blast = GD.Load<PackedScene>("res://scenes/game/enemies/BlastArea.tscn");

		// variable set up
		shootingCooldownTimer.WaitTime = shootingCooldown;
		blastCooldownTimer.WaitTime = blastCooldown;
	}

	public override void _PhysicsProcess(double delta) {
		// if not attacking, determine state
		if (blastCooldownTimer.IsStopped())
			DetermineState();

		// swinging at player and attacking?
		IsAttacking();

		// deal with slowdown
		anim.SpeedScale = Globals.inSlowdown ? Globals.slowdownRate : 1;
		delta *= Globals.inSlowdown ? Globals.slowdownRate : 1;
		if (deathState)
			return;

		// flip sprite based on player position
		if (player.GlobalPosition.X < GlobalPosition.X) {
			sprite2D.FlipH = true;
		} else {
			sprite2D.FlipH = false;
		}

		switch(state) {
			case EnemyState.ATTACK:
				Move(player.GlobalPosition, (float)delta);
				anim.Play("walk");
				break;
			case EnemyState.SURROUND:
				Move(GetCirclePosition(random.Randf()), (float)delta);
				anim.Play("walk");
				break;
			case EnemyState.HIT:
				Move(player.GlobalPosition, (float)delta);
				anim.Play("attack");
				break;
			case EnemyState.SHOOT:
				if (blastCooldownTimer.IsStopped()) {
					shootingCooldownTimer.Start();
					blastCooldownTimer.Start();
				}
				break;
		}

		if (anim.CurrentAnimation == "" ||
			(anim.CurrentAnimation == "walk" && state == EnemyState.SHOOT)) {
			anim.Play("idle");
		}
	}

	public new void OnAnimationFinished(StringName animName) {
		base.OnAnimationFinished(animName);
		if ((string) animName == "blast") {
			anim.Play("idle");
		}
	}

	public void OnShootingCooldownTimeout() {
		if (deathState)
			return;
		anim.Play("blast");
		BlastSearch();
	}

	/*
	When player enters damage area
	*/
	public void _OnDamageBodyEntered(Node2D body) {
		if (body.IsInGroup("player")) {
			canAttack = true;
		}
	}

	public void _OnDamageBodyExited(Node2D body) {
		if (body.IsInGroup("player")) {
			canAttack = false;
		}
	}

	/*
	Generates area where player is
	*/
	public void BlastSearch() {
		blastArea = (BlastArea) blast.Instantiate();
		blastArea.GlobalPosition = player.GlobalPosition;
		AddSibling(blastArea);
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
		if (state == EnemyState.HIT)
			return;
		state = distance < range ? EnemyState.SHOOT : EnemyState.SURROUND; // shoot or surround
		state = distance < 20 ? EnemyState.ATTACK : state; // attack if close
		
	}

}
