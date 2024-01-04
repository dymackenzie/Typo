using Godot;
using System;

public partial class Globals : Node
{

    [Signal] public delegate void ExperienceChangedEventHandler();
	
    public bool inSlowdown;
    private int Experience = 0;

    public void SetExperience(int experience) {
        Experience = experience;
        EmitSignal(nameof(ExperienceChanged));
    }

    public int GetExperience() {
        return Experience;
    }

}
