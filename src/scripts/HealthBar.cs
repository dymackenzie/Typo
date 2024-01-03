using Godot;
using System;

public partial class HealthBar : TextureProgressBar
{

    [Export] NodePath playerPath;

    private Player player;

    public override void _Ready() {
        player = GetNode<Player>(playerPath);
        player.HealthChanged += Update;
    }

    public void Update() {
        Value = player.health * 16.7f;
    }


}
