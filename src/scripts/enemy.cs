using Godot;
using System;

public partial class Enemy : CharacterBody2D
{

	public enum EnemyState {
		SHOOT,
		SURROUND,
		ATTACK,
		HIT,
		DEATH
	}

	[Export] public float speed 		= 50.0f;
	[Export] public float killRadius 	= 50.0f;
	[Export] public float healthUnit	= 20;
	[Export] public int difficulty	 	= 6;
	[Export] public int orbNumber 		= 3;
	[Export] public bool hasShield		= false;
	[Export] public Color textCorrect 	= new("#00FF00");
	[Export] public Color textWrong 	= new("#FF0000");
	[Export] public PackedScene poof;

	public float health = 0;

	public Globals					Globals;
	public Words					Words = new();
	public Player 					player;
	public Timer 					attackTimer;
	public AnimatedSprite2D			sprite2D;
	public OrbGenerator				orbs;
	public AnimationPlayer			anim;
	public RandomNumberGenerator 	random = new();
	public PackedScene				floatingText;

	public EnemyState 				state = EnemyState.SURROUND;
	public RichTextLabel 			prompt;
	public bool						deathState;
	public bool						canAttack;
	public string 					promptText;
	public int 						currentLetterIndex;	
	

    public override void _Ready() {
		foreach (Node node in GetTree().GetNodesInGroup("player")) {
            player = (Player) node;
        }
		Globals = GetNode<Globals>("/root/Globals");
		floatingText = GD.Load<PackedScene>("res://scenes/game/enemies/FloatingText.tscn");
		prompt = GetNode<RichTextLabel>("TypingText");
		attackTimer = GetNode<Timer>("AttackTimer");
		sprite2D = GetNode<AnimatedSprite2D>("AnimatedSprite");
		anim = GetNode<AnimationPlayer>("AnimationPlayer");
		orbs = GetNode<OrbGenerator>("OrbGenerator");
		random.Randomize();

		// word prompt
		prompt.Visible = false;
		SetPrompt(difficulty);

		// add overall difficulty to local difficulty
		difficulty += Globals.Difficulty;

		// health based on difficulty
		health = difficulty * healthUnit;
		orbs.orbNumber = orbNumber + Globals.ExperienceAddOns; // orb number
    }

    public override void _PhysicsProcess(double delta) {
		// deal with slowdown
		IsAttacking();
		anim.SpeedScale = Globals.InSlowdown ? Globals.SlowdownRate : 1;
		delta *= Globals.InSlowdown ? Globals.SlowdownRate : 1;
		if (deathState)
			return;
		switch(state) {
			case EnemyState.SURROUND:
				Move(GetCirclePosition(random.Randf()), (float)delta);
				anim.Play("walk");
				break;
			case EnemyState.ATTACK:
				Move(player.GlobalPosition, (float)delta);
				anim.Play("walk");
				break;
			case EnemyState.HIT:
				Move(player.GlobalPosition, (float)delta);
				anim.Play("attack1");
				break;
		}
	
	}

	public void IsAttacking() {
		if (player.shield.IsStopped() && canAttack) {
			player.OnDamage();
		}
	}

	public void OnHit(string s) {
		health -= healthUnit;
		EmitText(s);
		if (health <= 0 && hasShield) {
			OnShieldBreak();
		}
		if (health <= 0 && !hasShield) {
			OnDeath();
		}
	}

	/*
	Helper function to do floating text damage visuals
	*/
	public void EmitText(string s) {
		FloatingText text = (FloatingText) floatingText.Instantiate();
		text.SetText(s);
		AddChild(text);
	}

	/*
	When shield is broken for enemy, change prompt and reset health
	*/
	public void OnShieldBreak() {
		int shieldBuff = 2;
		SetPrompt(difficulty - shieldBuff);
		health = (difficulty - shieldBuff) * healthUnit;
		currentLetterIndex = 0;
	}

	/*
	When enemy health is <= 0, animate death and disable hitboxes
	*/
	public void OnDeath() {
		CollisionShape2D hitbox = GetNode<CollisionShape2D>("Hitbox");
		CollisionShape2D damageArea = GetNode<CollisionShape2D>("Damage/CollisionShape2D");
		Sprite2D spritePos = GetNode<Sprite2D>("TruePosition");
		Tween tween = CreateTween().SetTrans(Tween.TransitionType.Quart).SetEase(Tween.EaseType.Out);

		var direction = (player.GlobalPosition - GlobalPosition).Normalized();
		tween.TweenProperty(prompt, "modulate:a", 0.4, 0.1);
		tween.TweenProperty(sprite2D, "position", -direction * 3, 0.1);

		prompt.ZIndex = -10;
		hitbox.Disabled = damageArea.Disabled = deathState = true;
		spritePos.Visible = false;
		anim.Play("death");
	}

	public void OnAnimationFinished(StringName animName) {
		if ((string) animName == "death") {
			EmitSmoke();
			QueueFree();
			orbs.GenerateOrbs();
		}
	}

	/*
	Helper function to randomly select prompt and set it to the prompt text
	*/
	public void SetPrompt(int difficulty) {
		promptText = (string)Words.Call("GetRandomPrompt", difficulty);
		prompt.Text = SetCenterTags(promptText);
	}

	/*
	Helper function to emit smoke particles
	*/
	private void EmitSmoke() {
		GpuParticles2D smoke = (GpuParticles2D) poof.Instantiate();
		smoke.GlobalPosition = GlobalPosition;
		smoke.Restart();
		smoke.Emitting = true;
		AddSibling(smoke);
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

	/*
	When player enters damage area
	*/
	public void OnDamageBodyEntered(Node2D body) {
		if (body.IsInGroup("player")) {
			canAttack = true;
		}
	}

	public void OnDamageBodyExited(Node2D body) {
		if (body.IsInGroup("player")) {
			canAttack = false;
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
		} else if (state == "shoot") {
			this.state = EnemyState.SHOOT;
		} else if (state == "death") {
			this.state = EnemyState.DEATH;
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
	Colors the text according to user input (wrong or correct)
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
