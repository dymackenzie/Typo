using Godot;
using System;

public partial class WPMCounter : Control
{

    Player player;
    Label label;
    Color color;

    public override void _Ready() {
        foreach (Node node in GetTree().GetNodesInGroup("player")) {
            player = (Player) node;
        }
        label = GetNode<Label>("Label");
        color = label.Modulate;
        player.WPMChanged += OnWPMChanged;
    }

    public void OnWPMChanged(double WPM) {
        label.Text = "WPM: " + (int) WPM;
    }

}
