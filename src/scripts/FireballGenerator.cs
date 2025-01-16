using Godot;
using System;

public partial class FireballGenerator : Marker2D
{
 
    [Export] PackedScene fireball;

	Timer fireballCooldownTimer;
    
    public void GenerateFireball() {
		Fireball fireballInstance = (Fireball) fireball.Instantiate();
		fireballInstance.GlobalPosition = GlobalPosition;
		GetParent().GetParent().AddChild(fireballInstance);
    }
}
