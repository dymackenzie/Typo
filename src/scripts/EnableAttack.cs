using Godot;
using System;

public partial class EnableAttack : Node2D
{

	[Export] public int radius = 50;

	public int pointCount = 50;
    public bool killMode = false;
    public double originalWait;
    
    public Player player;
    public Timer timer;

    public override void _Ready() {
        foreach (Node node in GetTree().GetNodesInGroup("player")) {
            player = (Player) node;
        }
        timer = GetNode<Timer>("InKillMode");
        originalWait = timer.WaitTime;
        player.InSlowdown += BeginCountdown;
        player.KeySuccess += AddTime;
    }

    public override void _Process(double delta) {
        QueueRedraw();
    }

    public override void _Draw() {
        if (killMode) {
            DrawArc(Vector2.Zero, radius, 0, (float) Math.Tau * (float) (timer.TimeLeft / timer.WaitTime), pointCount, new Color(0.29f, 0.02f, 0.02f), 1, false);
        } else {
            DrawArc(Vector2.Zero, radius, 0, (float) Math.Tau, pointCount, new Color(1, 1, 1, 0.2f), 1, false);
        }
        
    }

    public void AddTime() {
        var currentTime = timer.TimeLeft;
        timer.WaitTime = currentTime + 0.5;
    }

    public void BeginCountdown(bool value) {
        killMode = value;
        timer.WaitTime = originalWait;
        if (value) {
            timer.Start();
        } else {
            timer.Stop();
        }
    }

    /*
    If timer runs out, end Kill Mode
    */
    public void OnInKillModeTimeout() {
        player.SwitchKillMode();
    }

}
