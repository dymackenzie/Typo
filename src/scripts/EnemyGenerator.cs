using Godot;
using System;

public partial class EnemyGenerator : Node2D
{

    [Export] float cooldownDuration = 5; // in seconds
	[Export] float difficultyIncreaseDuration = 180; // in seconds
	[Export] public PackedScene basicEnemyScene;
	[Export] public PackedScene rangedEnemyScene;
	[Export] public PackedScene blastEnemyScene;
	[Export] public float blastEnemySpawnChance = 0.15f;
	[Export] public float rangedEnemySpawnChance = 0.4f;

	public Player player;
    public RandomNumberGenerator random = new();
	public Globals globals;

    // spawnpoints
    public PathFollow2D pathFollow;
    public Marker2D marker;

	private double time = 0;
	private double difficultyIncreaseTime = 0;

	public override void _Ready() {
		globals = GetNode<Globals>("/root/Globals");
		foreach (Node node in GetTree().GetNodesInGroup("player")) {
            player = (Player) node;
			pathFollow = GetNode<PathFollow2D>(node.GetPath() + "/enemy_spawning/enemy_spawn_range");
			marker = GetNode<Marker2D>(node.GetPath() + "/enemy_spawning/enemy_spawn_range/Marker2D");
        }
        random.Randomize();
	}

    public override void _PhysicsProcess(double delta) {
		// deal with slowdown
		delta *= globals.InSlowdown ? globals.SlowdownRate : 1;

		// after time is up, spawn enemy
        time += delta;
		difficultyIncreaseTime += delta;
		if (time >= cooldownDuration) {
			RandomEnemySpawn();
			time = 0;
		}
		if (difficultyIncreaseTime >= difficultyIncreaseDuration) {
			globals.Difficulty++;
			difficultyIncreaseTime = 0;
		}
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
			// spawn multiple enemies
			int enemyNumber = random.RandiRange(0, 2);
			for (int i = 0; i < enemyNumber; i++) {
				enemyInstance = (Enemy) basicEnemyScene.Instantiate();
				pathFollow.Progress = random.RandiRange(0, 1623); // instantiate enemy at random point
				// also i honestly don't know where this number came from... it's been too long to remember
				enemyInstance.GlobalPosition = marker.GlobalPosition;
				AddSibling(enemyInstance);
			}
			enemyInstance = (Enemy) basicEnemyScene.Instantiate();
		}
		pathFollow.Progress = random.RandiRange(0, 1623); // instantiate enemy at random point
		// also i honestly don't know where this number came from... it's been too long to remember
		enemyInstance.GlobalPosition = marker.GlobalPosition;
		AddSibling(enemyInstance);
	}
}
