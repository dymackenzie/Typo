using Godot;
using System;

public partial class Globals : Node
{

    [Signal] public delegate void ExperienceChangedEventHandler(int level);
	
    public bool inSlowdown;
    public int Experience = 0;

    public void AddExperience(int increment) {
        Experience += increment;
        EmitSignal(nameof(ExperienceChanged), increment);
    }

}
