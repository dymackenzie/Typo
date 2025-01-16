using Godot;
using System;

public partial class Shop : CanvasLayer
{

	public bool isOpen = false;

	public override void _Ready() {
		close();
	}

	public override void _Process(double delta) {
		if (Input.IsActionJustPressed("ui_focus_next")) {
			if (!isOpen) {
				open();
			} else {
				close();
			}
		}
	}

	private void open() {
		Visible = true;
		isOpen = true;
	}

	private void close() {
		Visible = false;
		isOpen = false;
	}
}
