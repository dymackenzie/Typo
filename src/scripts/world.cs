using Godot;
using System;

public partial class world : Node2D
{

	private Timer enemyTimer;
	private PackedScene enemy;

	public override void _Ready() {
		enemyTimer = GetNode<Timer>("enemy_timer");
		enemy = GD.Load<PackedScene>("res://scenes/game/enemies/basic_enemy.tscn");
	}

	/*
	Controls enemy spawn timing.
	*/
	public void OnEnemyTimerTimeout() {
		// generate random number on path follow to spawn enemy at
		GD.Print("CALLED");
		RandomNumberGenerator random = new();
		random.Randomize();
		PathFollow2D pathFollow2D = GetNode<PathFollow2D>("player/enemy_spawning/enemy_spawn_range");
		pathFollow2D.Progress = random.RandiRange(0, 1623);

		// instantiate enemy at random point
		CharacterBody2D instance = (CharacterBody2D)enemy.Instantiate();
		Marker2D position = GetNode<Marker2D>("player/enemy_spawning/enemy_spawn_range/Marker2D");
		instance.GlobalPosition = position.GlobalPosition;
		AddChild(instance);
	}
}
