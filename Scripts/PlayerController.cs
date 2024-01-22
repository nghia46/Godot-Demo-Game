using Godot;

// Declaring the namespace for the script
namespace GDGAME.Scripts;
// Declaring the partial class PlayerController, which extends CharacterBody2D

public partial class PlayerController : CharacterBody2D
{
	// Declaring and exporting variables for speed and jump velocity
	[Export] public float Speed = 100f;

	[Export] public float JumpVelocity = -250f;

	// Boolean to keep track of the player's facing direction
	private bool _isFacingRight = true;
	// Reference to the AnimatedSprite2D node
	private AnimatedSprite2D _animatedSprite2D;

	private CollisionShape2D _collisionShape2D;

	// Gravity variable obtained from the project settings
	private float _gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();

	private bool _isCrouching;
	private bool _isColliderWithLadder;

	// Called when the node enters the scene tree for the first time
	public override void _Ready()
	{
		// Getting the reference to the AnimatedSprite2D node
		_animatedSprite2D = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		//get the box collider
		_collisionShape2D = GetNode<CollisionShape2D>("CollisionShape2D");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame
	public override void _Process(double delta)
	{
		// Getting the player's velocity
		var velocity = Velocity;
		// Getting the input direction and handling the movement/deceleration
		var direction = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
		Velocity = velocity;
		// Handling move action
		GetMoveAction(direction, velocity, IsOnFloor());
		// Adding gravity if the player is not on the floor
		GetGravity(velocity, delta, IsOnFloor());
		// Flipping the player
		FlipPLayer(velocity.X);
		// Handling player animations
		PlayerAnimation(velocity);
		// Moving the player with sliding
		MoveAndSlide();
	}

	private void GetMoveAction(Vector2 direction, Vector2 velocity, bool isOnFloor)
	{
		//Handel jump
		if (Input.IsActionJustPressed("ui_accept") && isOnFloor)
		{
			velocity.Y = JumpVelocity;
			Velocity = velocity;
		}
		//Hand move
		if (direction != Vector2.Zero)
		{
			velocity.X = direction.X * Speed;
			//Climb
			if (Input.IsActionPressed("ui_up") && _isColliderWithLadder)
			{
				_gravity = 0;
			   // velocity = Vector2.Zero; // Stop horizontal movement while climbing
				var pos = Position;
				pos.Y -= 1; // Adjust this value based on the ladder height
				Position = pos;
				GD.Print("Climb Up");
			}else if (Input.IsActionPressed("ui_down") && _isColliderWithLadder)
			{
				_gravity = 0;
				//velocity = Vector2.Zero; // Stop horizontal movement while climbing
				var pos = Position;
				pos.Y += 1; // Adjust this value based on the ladder height
				Position = pos;
				GD.Print("Climb Down");
			}
			else
			{
				_gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();
			}
			Velocity = velocity;
		}
		else
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
			Velocity = velocity;
		}
		// Adjust the crouching logic
		if (Input.IsActionPressed("ui_down"))
		{
			_collisionShape2D.Scale = new Vector2(1f, 0.5f);
			_collisionShape2D.Position = new Vector2(0, 10f);
		}
		else
		{
			_collisionShape2D.Scale = new Vector2(1f, 1f);
			_collisionShape2D.Position = new Vector2(0, 5f);
		}
	}
	private void GetGravity(Vector2 velocity, double delta, bool isOnFloor)
	{
		if (isOnFloor) return;
		velocity.Y += _gravity * (float)delta;
		Velocity = velocity;
	}
	// Method to handle player animations based on velocity
	private void PlayerAnimation(Vector2 velocity)
	{
		_isCrouching = Input.IsActionPressed("ui_down");
		if (_isCrouching)
		{
			_animatedSprite2D.Play("Crouch");
		}
		else if (velocity.Y != 0)
		{
			_animatedSprite2D.Play(velocity.Y < 0 ? "Jumping" : "Falling");
		}
		else
		{
			_animatedSprite2D.Play(Mathf.Abs(velocity.X) > 0 ? "Run" : "Idle");
		}
	}
	// Method to flip the player's direction based on movement
	private void FlipPLayer(float dir)
	{
		var scale = Scale;
		if (dir < 0 && _isFacingRight || dir > 0 && !_isFacingRight)
		{
			scale.X *= -1;
			_isFacingRight = !_isFacingRight;
		}

		Scale = scale;
	}
	private void _on_climbable_check_body_entered(Node2D body)
	{
		_isColliderWithLadder = true;
	}
	private void _on_climbable_check_body_exited(Node2D body)
	{
		_isColliderWithLadder = false;
	}
}
