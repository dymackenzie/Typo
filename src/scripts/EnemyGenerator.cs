using Godot;
using System;

public partial class EnemyGenerator : Node2D
{

    [Export] float cooldownDuration = 5; // in seconds
	[Export] public PackedScene basicEnemyScene;
	[Export] public PackedScene rangedEnemyScene;
	[Export] public PackedScene blastEnemyScene;
	[Export] public float blastEnemySpawnChance = 0.15f;
	[Export] public float rangedEnemySpawnChance = 0.4f;

	public Timer cooldown;
	public Player player;
    public RandomNumberGenerator random = new();

    // spawnpoints
    public PathFollow2D pathFollow;
    public Marker2D marker;

	public override void _Ready() {
		cooldown = GetNode<Timer>("EnemyCooldown");
		foreach (Node node in GetTree().GetNodesInGroup("player")) {
            player = (Player) node;
			pathFollow = GetNode<PathFollow2D>(node.GetPath() + "/enemy_spawning/enemy_spawn_range");
			marker = GetNode<Marker2D>(node.GetPath() + "/enemy_spawning/enemy_spawn_range/Marker2D");
        }
        cooldown.WaitTime = cooldownDuration;
        random.Randomize();
	}

	/*
	Controls enemy spawn timing.
	*/
	public void RandomEnemySpawn() {
		float enemySpawnPercentage = random.RandiRange(0, 100) / 100.0f;
		Enemy enemyInstance;
		if (enemySpawnPercentage < blastEnemySpawnChance) {
			enemyInstance = (Enemy) blastEnemyScene.Instantiate();
		} else if (enemySpawnPercentage < rangedEnemySpawnChance) {
			enemyInstance = (Enemy) rangedEnemyScene.Instantiate();
		} else {
			enemyInstance = (Enemy) basicEnemyScene.Instantiate();
		}
		pathFollow.Progress = random.RandiRange(0, 1623); // instantiate enemy at random point
		enemyInstance.GlobalPosition = marker.GlobalPosition;
		AddSibling(enemyInstance);
	}
	
	public void OnEnemyCooldownTimeout() {
		RandomEnemySpawn();
	}

}
