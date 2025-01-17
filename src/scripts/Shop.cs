using Godot;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public partial class Shop : CanvasLayer
{

	[Export] public float fadeTime = 0.5f;
	[Export] CanvasLayer UI;
	[Export] public ShopScript shopScript;

	Control control;

	public bool isOpen = false;
	

	public override void _Ready() {
		control = GetNode<Control>("Main");

		// set opacity to 0
		Color modulate = control.Modulate;
		control.Modulate = new Color(modulate.R, modulate.G, modulate.B, 0);

		isOpen = false;
		GetTree().Paused = false;
	}

	public override void _Process(double delta) {
		HandleInput();
	}

	public void HandleInput() {
		// open and close shop
		if (Input.IsActionJustPressed("ui_focus_next")) {
			if (!isOpen) {
				Open();
			} else {
				Close();
			}
		}
	}

	public void Open() {
		// make UI disappear
		UI.Visible = false;

		GetTree().Paused = true;
		isOpen = true;
		// make fade in on instance
		Tween tween = CreateTween().SetTrans(Tween.TransitionType.Quint).SetEase(Tween.EaseType.Out);
		tween.TweenProperty(control, "modulate:a", 1, fadeTime);
	}

	public void Close() {
		// make fade in on instance
		Tween tween = CreateTween().SetTrans(Tween.TransitionType.Quint).SetEase(Tween.EaseType.Out);
		tween.TweenProperty(control, "modulate:a", 0, fadeTime);
		isOpen = false;
		GetTree().Paused = false;

		// make UI appear
		UI.Visible = true;
	}

}