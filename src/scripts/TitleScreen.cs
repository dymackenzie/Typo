using Godot;
using System;
using System.Text.RegularExpressions;

public partial class TitleScreen : CanvasLayer
{

	[Export] TextEdit input;
	[Export] Label error;

	Globals globals;

	public override void _Ready() {
		globals = GetNode<Globals>("/root/Globals");
		error.Visible = false;
	}

	public override void _Process(double delta) {
		if (Input.IsKeyLabelPressed(Key.Enter)) {

			string userInput = input.Text;
			input.Text = "";

			if (userInput.Contains('q')) {
				GetTree().Quit();
			} else if (userInput.Contains("settings")) {
				GD.Print("settings page");
			} else if (Regex.Matches(userInput ,@"[a-zA-Z]").Count > 0) {
				error.Visible = true;
			} else {
				error.Visible = false;
				try {
					int wpm = int.Parse(userInput);
					globals.SetPlayerWPM(wpm);
					GetTree().ChangeSceneToFile("res://scenes/game/World.tscn");
				}
				catch (FormatException) {
					error.Visible = true;
				}
			}
		}
	}



}
