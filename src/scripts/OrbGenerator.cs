using Godot;
using System;

public partial class OrbGenerator : Marker2D
{

    [Export] public int orbNumber = 3;
    [Export] public int orbLevel = 1;

    PackedScene orb;

    public override void _Ready() {
        orb = GD.Load<PackedScene>("res://scenes/game/Orbs.tscn");
    }

    public void SetOrbNumber(int i) {
        orbNumber = i;
    } 

    public void GenerateOrbs() {
        for (int i = 0; i < orbNumber; i++) {
            Orbs orbs = (Orbs) orb.Instantiate();
            orbs.GlobalPosition = GlobalPosition;
            orbs.level = orbLevel;
            GetParent().GetParent().AddChild(orbs);
        }
    }

}
