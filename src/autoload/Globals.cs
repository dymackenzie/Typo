using Godot;
using System;

public partial class Globals : Node
{

    [Signal] public delegate void ExperienceChangedEventHandler(int increment);
    [Signal] public delegate void CameraShakeEventHandler(float scale);
	
    public bool inSlowdown;
    public float slowdownRate = 0.05f;

    public int Experience = 200;
    public int ExperienceAddOns = 0;

    public void AddExperience(int increment) {
        Experience += increment;
        EmitSignal(nameof(ExperienceChanged), increment);
    }

    public void ShakeCamera(float scale) {
        EmitSignal(nameof(CameraShake), scale);
    }

}
