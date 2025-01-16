using Godot;
using System;

public partial class ShieldBar : TextureRect
{
	[Export] NodePath playerPath;

    private Player player;

    public override void _Ready() {
        player = GetNode<Player>(playerPath);
		Visible = true;
        player.ShieldChanged += Update;
    }

	public void Update() {
		Visible = !Visible;
	}
}
