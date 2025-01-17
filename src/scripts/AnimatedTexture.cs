using Godot;
using System;

public partial class AnimatedTexture : TextureRect
{
	[Export] SpriteFrames sprites;
	[Export] string currentAnimation = "default";
	[Export] int frameIndex = 0;
	[Export] double speedScale = 1.0f;
	[Export] bool autoPlay = false;
	[Export] bool playing = false;

	double refreshRate = 1.0f;
	double fps = 30.0f;
	double frameDelta = 0;

	public override void _Ready() {
		fps = sprites.GetAnimationSpeed(currentAnimation);
		refreshRate = sprites.GetFrameDuration(currentAnimation, frameIndex);
		if (autoPlay) {
			Play();
		}
	}

	public override void _Process(double delta) {
		if (sprites == null || playing == false) {
			return;
		}
		if (sprites.HasAnimation(currentAnimation) == false) {
			playing = false;
			GD.Print("Animation not found: " + currentAnimation);
		}
		GetAnimationData(currentAnimation);
		frameDelta += (speedScale * delta);
		if (frameDelta >= refreshRate/fps) {
			Texture = GetNextFrame();
			frameDelta = 0;
		}
	}

	public void Play(string animationName = "default")
	{
		frameIndex = 0;
		frameDelta = 0;
		currentAnimation = animationName;
		GetAnimationData(currentAnimation);
		playing = true;
	}

	public void GetAnimationData(string animation) {
		fps = sprites.GetAnimationSpeed(animation);
		refreshRate = sprites.GetFrameDuration(animation, frameIndex);
	}

	public Texture2D GetNextFrame() {
		frameIndex++;
		if (frameIndex >= sprites.GetFrameCount(currentAnimation)) {
			frameIndex = 0;
		}
		GetAnimationData(currentAnimation);
		return sprites.GetFrameTexture(currentAnimation, frameIndex);
	}

	public void Resume() {
		playing = true;
	}

	public void Pause() {
		playing = false;
	}

	public void Stop() {
		playing = false;
		frameIndex = 0;
	}
}
