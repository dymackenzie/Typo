using Godot;
using System;

public partial class dash : Node2D
{

	private Timer timer;
	private PackedScene ghostScene;
	private AnimatedSprite2D sprite;

	public override void _Ready() {
		timer = GetNode<Timer>("duration");
		ghostScene = GD.Load<PackedScene>("res://scenes/game/dash_ghost.tscn");
	}

	/*
	Starts dash timer and instantiates ghosts
	*/
	public void StartDash(AnimatedSprite2D sprite, float duration) {
		this.sprite = sprite;

		timer.WaitTime = duration;
		timer.Start();
		InstanceGhost();
	}

	public bool IsDashing() {
		return !timer.IsStopped();
	}

	private void InstanceGhost() {

		// add ghost to scene
		Sprite2D ghost = (Sprite2D)ghostScene.Instantiate();
		GetParent().GetParent().AddChild(ghost);

		// set ghost's attributes to match dashing sprite
		ghost.GlobalPosition = GlobalPosition;
		ghost.FlipH = sprite.FlipH;
	}

	
}
