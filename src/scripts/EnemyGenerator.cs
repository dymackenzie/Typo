using Godot;
using System;

public partial class EnemyGenerator : Node2D
{

    [Export] NodePath playerPath;
    [Export] float cooldownDuration = 5; // in seconds

	public Timer cooldown;
	public PackedScene basicEnemyScene;
	public Player player;
    public RandomNumberGenerator random = new();

    // spawnpoints
    public PathFollow2D pathFollow;
    public Marker2D marker;

	public override void _Ready() {
		cooldown = GetNode<Timer>("EnemyCooldown");
		basicEnemyScene = GD.Load<PackedScene>("res://scenes/game/enemies/BasicEnemy.tscn");
		player = GetNode<Player>(playerPath);
        pathFollow = GetNode<PathFollow2D>(playerPath + "/enemy_spawning/enemy_spawn_range");
        marker = GetNode<Marker2D>(playerPath + "/enemy_spawning/enemy_spawn_range/Marker2D");
        cooldown.WaitTime = cooldownDuration;
        random.Randomize();
	}

	/*
	Controls enemy spawn timing.
	*/
	public void RandomEnemySpawn() {
        Enemy enemyInstance = (Enemy) basicEnemyScene.Instantiate();
		pathFollow.Progress = random.RandiRange(0, 1623); // instantiate enemy at random point
		enemyInstance.GlobalPosition = marker.GlobalPosition;
		enemyInstance.player = player;
		AddSibling(enemyInstance);
	}

	
	public void OnEnemyCooldownTimeout() {
		RandomEnemySpawn();
	}

}
