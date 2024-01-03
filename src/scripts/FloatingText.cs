using Godot;

public partial class FloatingText : Marker2D
{
	
	private Label label;
	private RandomNumberGenerator random = new();
	private Vector2 velocity = Vector2.Zero;
	

	public override void _Ready() {
		RandomizeVelocity();
		AnimatedText();
	}

	public void SetAmount(float amount) {
		label = GetNode<Label>("Label");
		label.Text = amount.ToString();
	}

	/*
	Randomize velocity
	*/
	private void RandomizeVelocity() {
		random.Randomize();
		int horizontalVelocity = (int) random.Randi() % 61 - 30;
		velocity = new(horizontalVelocity, 30);
	}

	/*
	Animates the damage indicator
	*/
	private void AnimatedText() {
		Tween tween = CreateTween();
		tween.SetTrans(Tween.TransitionType.Linear).SetEase(Tween.EaseType.Out);
		tween.TweenProperty(this, "scale", new Vector2(0.2f, 0.2f), 0.3);
		tween.TweenInterval(0.2);
		tween.TweenProperty(this, "scale", new Vector2(0.1f, 0.1f), 0.1);
		tween.TweenCallback(Callable.From(QueueFree));
	}

    public override void _PhysicsProcess(double delta)
    {
		Position -= velocity * (float) delta;
    }

}
