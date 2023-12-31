using Godot;
using System;

public partial class enable_attack : Node2D
{

	[Export] public int radius = 50;
	public int pointCount = 50;

    public override void _Draw()
    {
        DrawArc(Vector2.Zero, radius, 0, (float)Math.Tau, pointCount, new Color(1, 1, 1, 0.2f), 1, false);
    }

}
