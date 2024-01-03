using Godot;

public partial class ShakingCamera : Camera2D
{

    [Export] public float duration         = 0.8f;
    [Export] public float amplitude        = 6.0f;
    [Export] public float damping          = 0.4f;
    [Export] public bool shake             = false; 
    [Export] public float zoomScale        = 1.0f;
    [Export] public float zoomDuration     = 0.4f;
    [Export] public bool shakeEnable       = false;

    private Tween tween;
    private Globals globals;
    private Timer timer;
    private RandomNumberGenerator random = new();

    public override void _Ready() {
        globals = GetNode<Globals>("/root/Globals");
        timer = GetNode<Timer>("Timer");
        random.Randomize();
        SetDuration(duration);
        SetProcess(false);
    }

    public void SetDuration(float duration) {
        timer.WaitTime = duration;
    }

    public override void _Process(double delta) {
        Offset = new Vector2(
            random.RandfRange(amplitude, -amplitude) * damping,
            random.RandfRange(amplitude, -amplitude) * damping
        );
    }

    public void OnCameraShakeRequested() {
        if (!shakeEnable)
            return;
        SetShake(true);
    }

    public void OnTimerTimeout() {
        SetShake(false);
    }

    public void SetShake(bool value) {
        shake = value;
        SetProcess(shake);
        Offset = new Vector2();
        if (shake) {
            timer.Start();
        }
    }

    /*
	Zomm in camera and zoom out
	*/
	public void CameraZoom(bool zoomIn) {
		Tween tween = CreateTween().SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.Out);
		globals.isInSlowdown = !globals.isInSlowdown; // emit slowdown signal
		if (zoomIn) 
            tween.TweenProperty(this, "zoom", Zoom + new Vector2(zoomScale, zoomScale), zoomDuration);
        else 
            tween.TweenProperty(this, "zoom", Zoom - new Vector2(zoomScale, zoomScale), zoomDuration);
	}

}
