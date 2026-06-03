# Typo

Typo is a fast-paced, arcade-style typing game built with Godot and C# (Mono). Type words to attack enemies, collect orbs, and survive increasingly challenging waves.

Download
- Play the released build: https://dymackenzie.itch.io/typo

Features
- Reflex-based typing gameplay with enemy patterns and projectiles
- Multiple enemy types and generators
- On-screen HUD: health, shield, orbs, WPM and more
- Local C# scripts and Godot scenes for easy extension

Requirements
- Godot Engine with Mono/C# support (matching the Godot version used when the project was created)
- .NET/Mono SDK compatible with the Godot Mono build

Getting started (run locally)
1. Install Godot with Mono support (see Godot docs for your platform).
2. Clone this repository.
3. Open the project folder in Godot (`project.godot`).
4. Ensure the C#/.NET SDK path is configured in the Godot Editor settings.
5. Press Play (F5) to run the project.

Building from source (optional)
- You can build the C# assemblies using your platform's .NET tooling if needed. The project contains `Typo.sln` and `Typo.csproj` for reference.
- Make sure the Godot editor recognizes the compiled assemblies or let Godot restore/compile them automatically.

Controls
- Type displayed words to attack enemies.
- Movement and other interactions are keyboard-driven (see in-game title/controls screens).

License
- See the included `LICENSE` file for licensing details.