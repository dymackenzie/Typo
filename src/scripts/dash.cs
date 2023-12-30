using Godot;
using System;

public partial class dash : Node2D
{

	private Timer timer;
	private Timer ghostTimer;
	private PackedScene ghostScene;
	private AnimatedSprite2D sprite;

	public override void _Ready() {
		timer = GetNode<Timer>("duration");
		ghostTimer = GetNode<Timer>("ghost");
		ghostScene = GD.Load<PackedScene>("res://scenes/game/player/dash_ghost.tscn");
	}

	/*
	Starts dash timer and instantiates ghosts
	*/
	public void StartDash(AnimatedSprite2D sprite, float duration) {
		this.sprite = sprite;

		timer.WaitTime = duration;
		timer.Start();
		ghostTimer.Start();
		
		InstanceGhost();
	}

	public bool IsDashing() {
		return !timer.IsStopped();
	}

	/*
	Adds ghost sprite to scene.
	*/
	private void InstanceGhost() {
		// add ghost to scene
		Sprite2D ghost = (Sprite2D)ghostScene.Instantiate();
		GetParent().GetParent().AddChild(ghost);

		// set ghost's attributes to match dashing spritegit 
		ghost.GlobalPosition = GlobalPosition;
		ghost.FlipH = sprite.FlipH;
	}



	/*
	When ghost timer ends, instantiate another ghost sprite.
	*/
	private void _on_ghost_timeout() {
		InstanceGhost();
	}

	private void _on_duration_timeout() {
		ghostTimer.Stop();
	}	

	
}



