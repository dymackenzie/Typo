using Godot;
using System;

public partial class FireballGenerator : Marker2D
{
    PackedScene fireball;
	Timer fireballCooldownTimer;

    public override void _Ready() {
        fireball = GD.Load<PackedScene>("res://scenes/game/enemies/Fireball.tscn");
    }

    public void GenerateFireball() {
		Fireball fireballInstance = (Fireball) fireball.Instantiate();
		fireballInstance.GlobalPosition = GlobalPosition;
		GetParent().GetParent().AddChild(fireballInstance);
    }
}
