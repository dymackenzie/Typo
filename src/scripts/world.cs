using Godot;
using System;

public partial class world : Node2D
{

	[Export] NodePath playerPath;

	private Timer enemyTimer;
	private PackedScene enemy;
	private CharacterBody2D player;

	public override void _Ready() {
		enemyTimer = GetNode<Timer>("enemy_timer");
		enemy = GD.Load<PackedScene>("res://scenes/game/enemies/basic_enemy.tscn");
		// load player
		player = GetNode<CharacterBody2D>(playerPath);
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
		CharacterBody2D enemyInstance = (CharacterBody2D)enemy.Instantiate();
		Marker2D position = GetNode<Marker2D>("player/enemy_spawning/enemy_spawn_range/Marker2D");
		enemyInstance.GlobalPosition = position.GlobalPosition;
		// sets player that enemy should attack
		enemyInstance.Call("SetPlayer", player);
		AddChild(enemyInstance);
	}
}
