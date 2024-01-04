using Godot;
using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection.Metadata.Ecma335;

public partial class Enemy : CharacterBody2D
{

	public enum EnemyState {
		SURROUND,
		ATTACK,
		HIT
	}

	[Export] public float speed 		= 50.0f;
	[Export] public float killRadius 	= 50.0f;
	[Export] public float healthUnit	= 20;
	[Export] public int difficulty	 	= 6;
	[Export] public float slowdownRate  = 0.05f;
	[Export] public int orbNumber 		= 3;
	[Export] public Color textCorrect 	= new("#00FF00");
	[Export] public Color textWrong 	= new("#FF0000");

	public float health = 0;

	public Words					words = new();
	public Player 					player = null;
	public Timer 					attackTimer;
	public AnimatedSprite2D 		sprite2D;
	public OrbGenerator				orbs;
	public EnemyState 				state = EnemyState.SURROUND;
	public RandomNumberGenerator 	random = new();
	public PackedScene				floatingText;

	public RichTextLabel 			prompt;
	public bool						inSlowdown;
	public bool						isAttacking;
	public bool						isDying;
	public string 					promptText;
	public int 						currentLetterIndex;	

    public override void _Ready() {
		floatingText = GD.Load<PackedScene>("res://scenes/game/enemies/FloatingText.tscn");
		prompt = GetNode<RichTextLabel>("TypingText");
		attackTimer = GetNode<Timer>("AttackTimer");
		sprite2D = GetNode<AnimatedSprite2D>("Sprite");
		orbs = GetNode<OrbGenerator>("OrbGenerator");
		random.Randomize();
		// word prompt
		prompt.Visible = false;
		promptText = (string)words.Call("GetRandomPrompt", difficulty);
		prompt.Text = SetCenterTags(promptText);
		// health based on difficulty
		health = difficulty * healthUnit;
		// orb number
		orbs.orbNumber = orbNumber;
		ConnectSignals();
    }

    public override void _PhysicsProcess(double delta) {
		// deal with slowdown
		IsAttacking();
		sprite2D.SpeedScale = inSlowdown ? slowdownRate : 1;
		delta *= inSlowdown ? slowdownRate : 1;
		if (!isDying) {
			switch(state) {
				case EnemyState.SURROUND:
					Move(GetCirclePosition(random.Randf()), (float)delta);
					sprite2D.Play("walk");
					break;
				case EnemyState.ATTACK:
					Move(player.GlobalPosition, (float)delta);
					sprite2D.Play("walk");
					break;
				case EnemyState.HIT:
					Move(player.GlobalPosition, (float)delta);
					sprite2D.Play("attack");
					break;
			}
		}
	}

	public void IsAttacking() {
		if (player.shield.IsStopped() && isAttacking) {
			player.OnDamage();
		}
	}

	public void OnHit(string s) {
		health -= healthUnit;
		EmitText(s);
		if (health <= 0) {
			OnDeath();
		}	
	}

	public void EmitText(string s) {
		FloatingText text = (FloatingText) floatingText.Instantiate();
		text.SetText(s);
		AddChild(text);
	}

	public void OnDeath() {
		CollisionShape2D hitbox = GetNode<CollisionShape2D>("Hitbox");
		CollisionShape2D damageArea = GetNode<CollisionShape2D>("Damage/CollisionShape2D");
		Sprite2D spritePos = GetNode<Sprite2D>("TruePosition");

		Tween tween = CreateTween();
		tween.TweenProperty(prompt, "modulate:a", 0.3, 0.2);

		hitbox.Disabled = damageArea.Disabled = true;
		spritePos.Visible = false;
		isDying = true; // disable hitbox
		sprite2D.Play("death");
	}

	public void OnAnimatedEnemyAnimationFinished() {
		if (isDying) {
			QueueFree();
			orbs.GenerateOrbs();
		}
	}

	/*
	Calculates where player is and has enemy move toward player in a
	smooth motion.
	*/
	public void Move(Vector2 target, float delta) {
		// get directional vector
		Vector2 direction = (target - GlobalPosition).Normalized();
		float x = direction.X;
		float y = direction.Y / 2;
		Vector2 desiredVelocity = new Vector2(x, y) * speed;
		Vector2 steering = (desiredVelocity - Velocity) * delta * 2.5f; // for smoother movement
		// flip enemy is needed
		if (player.GlobalPosition.X < GlobalPosition.X) {
			sprite2D.FlipH = true;
		} else {
			sprite2D.FlipH = false;
		}
		Velocity += steering;
		MoveAndCollide(Velocity * delta);
	}

	/*
	Gets the circle position from around the player.
	*/
	public Vector2 GetCirclePosition(float random) {
		if (player != null) {
			float x = player.GlobalPosition.X + (float)Math.Cos(random * Math.PI * 2.0f) * killRadius;
			float y = player.GlobalPosition.Y + (float)Math.Sin(random * Math.PI * 2.0f) * killRadius;
			return new Vector2(x, y);
		} else {
			return Vector2.Zero;
		}
	}

	public void Slowdown(bool slowdown) {
		inSlowdown = slowdown;
	}

	public void ConnectSignals() {
		player.InSlowdown += Slowdown;
	}

	/*
	When player enters damage area
	*/
	public void OnDamageBodyEntered(Node2D body) {
		if (body.IsInGroup("player")) {
			isAttacking = true;
		}
	}

	public void OnDamageBodyExited(Node2D body) {
		if (body.IsInGroup("player")) {
			isAttacking = false;
		}
	}

	/*
	When the attack timer runs out, have enemies attack 
	*/
	public void OnAttackTimerTimeout() {
		state = EnemyState.ATTACK;
	}

	/*
	Changes state through provided string
	*/
	public void SetState(string state) {
		if (state == "surround") {
			this.state = EnemyState.SURROUND;
		} else if (state == "attack") {
			this.state = EnemyState.ATTACK;
		} else if (state == "hit") {
			this.state = EnemyState.HIT;
		}
	}

	/*
	Changes position color
	*/
	public void SetPositionColor(Color color) {
		Sprite2D spritePos = GetNode<Sprite2D>("TruePosition");
		spritePos.Modulate = color;
	}

	/*
	Gets prompt with a parsed bb-code removed string
	*/
	public string GetPrompt() {
		RegEx regex = new();
		regex.Compile("\\[.+?\\]");
		return regex.Sub(promptText, "", true);
	}

	/*
	Colors the text according to user input
	*/
	public void SetNextCharacter(bool wrong) {
		string correctText = string.Concat(GetBBCodeColorTag(textCorrect), GetPrompt().AsSpan(0, currentLetterIndex), GetEndColorTag());
		string wrongText = "";
		string remainingText = "";
		if (currentLetterIndex < GetPrompt().Length) {
			wrongText = !wrong ? GetPrompt().Substring(currentLetterIndex, 1) : string.Concat(GetBBCodeColorTag(textWrong), GetPrompt().AsSpan(currentLetterIndex, 1), GetEndColorTag());
			remainingText = GetPrompt()[(currentLetterIndex + 1)..];
		}
		prompt.Text = SetCenterTags(correctText + wrongText + remainingText);
	}

	/*
	Formats color into a BBCode tag
	*/
	private string GetBBCodeColorTag(Color color) {
		return "[color=#" + color.ToHtml(false) + "]";
	}

	/*
	Formats color into a BBCode tag
	*/
	private string GetEndColorTag() {
		return "[/color]";
	}

	/*
	Sets center tags
	*/
	private string SetCenterTags(string toCenter) {
		return "[center]" + toCenter + "[/center]";
	}

}
