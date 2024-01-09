using Godot;
using Godot.NativeInterop;
using System;
using System.Runtime.InteropServices;

public partial class RangedEnemy : Enemy
{

	[Export] public float range = 150.0f;
	[Export] public float shootingCooldown = 5;
	[Export] public float bulletSpeed = 10.0f;

	public Color kill = new("4a0505");
	public BlastArea blastArea;
	public PackedScene blast;
	public Line2D beam;
	public Timer timer;
	public Tween tween;

    public override void _Ready() {
        base._Ready();
		beam = GetNode<Line2D>("Beam");
		timer = GetNode<Timer>("ShootingCooldown");
		blast = GD.Load<PackedScene>("res://scenes/game/enemies/BlastArea.tscn");
		timer.WaitTime = shootingCooldown;
    }

    public override void _PhysicsProcess(double delta) {
		DetermineState();
		if (state != EnemyState.SHOOT) {
			base._PhysicsProcess(delta);
		} else {
			if (timer.IsStopped()) {
				Tween tween = CreateTween();
				BlastSearch();
				tween.TweenInterval(1);
				tween.TweenCallback(Callable.From(() => { anim.Play("fire_fist"); }));
				timer.Start();
			}
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
	Shoot projectile
	*/
	public void Shoot() {
		Tween tween = CreateTween();
		blastArea.enabled = true;
		tween.TweenInterval(1);
		tween.TweenCallback(Callable.From(() => { blastArea.QueueFree(); }));
	}

	public new void OnAnimationFinished(StringName animName) {
		base.OnAnimationFinished(animName);
		if ((string) animName == "fire_fist") {
			Shoot();
		}
	}	

	/*
	Determine state of ranged enemy
	*/
	public void DetermineState() {
		float distance = (player.GlobalPosition - GlobalPosition).Length();
		state = distance < range ? EnemyState.SHOOT : EnemyState.SURROUND; // in shooting range
		state = distance < 50.0f ? EnemyState.ATTACK : state; // in attacking range
		state = distance < 20.0f ? EnemyState.HIT : state; // in hitting range
	}

}
