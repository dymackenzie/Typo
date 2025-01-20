using Godot;
using System;

public partial class Globals : Node
{

    [Signal] public delegate void ExperienceChangedEventHandler(int increment);
    [Signal] public delegate void CameraShakeEventHandler(float scale);
	
    /*
    Varibles to handle slowdown rates.
    */
    [Export] public bool InSlowdown;
    [Export] public float SlowdownRate = 0.05f;

    /*
    Varibles to handle the player's skill level and wpm.
    */
    [Export] public int PlayerWPM = 80;

    /*
    Variables to handle the player's experience.
    */
    [Export] public int Experience = 200;
    [Export] public int ExperienceAddOns = 0;

    /*
    Variables to handle difficulty of game.
    */
    [Export] public int Difficulty = 0;

    public void SetPlayerWPM(int wpm) {
        PlayerWPM = wpm;
    }

    public void AddExperience(int increment) {
        Experience += increment;
        EmitSignal(nameof(ExperienceChanged), increment);
    }

    public void ShakeCamera(float scale) {
        EmitSignal(nameof(CameraShake), scale);
    }

}
