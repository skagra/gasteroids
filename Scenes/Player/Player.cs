using Godot;

public partial class Player : RigidBody2D
{
	private const string _ACTION_THRUST = "Thrust";
	private const string _ACTION_ROTATE_CW = "Rotate CW";
	private const string _ACTION_ROTATE_ACW = "Rotate ACW";
	private const string _ACTION_FIRE = "Fire";

	[Signal]
	public delegate void ShootEventHandler(Vector2 position, Vector2 shipLinearVelocity, float shipRotation);

	[Export]
	private int _thrustForce = 300;

	[Export]
	private float _rotationSpeed = 5.0f;

	private Area2D _area2D;
	private AnimatedSprite2D _sprite;
	private Vector2 _spriteSize;

	public override void _Ready()
	{
		_area2D = GetNode<Area2D>("Area2D");
		_area2D.BodyEntered += Entered;
		_sprite = _area2D.GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		_spriteSize = _sprite.SpriteFrames.GetFrameTexture(_sprite.Animation, _sprite.Frame).GetSize(); // TODO Scaling!
	}

	public override void _Input(InputEvent inputEvent)
	{
		if (inputEvent.IsActionPressed(_ACTION_FIRE))
		{
			GD.Print("Emitting bullet signal");
			EmitSignal(SignalName.Shoot,
				Position + ((_spriteSize.X / 2.0f) * Vector2.Right.Rotated(_area2D.Rotation)), // Current position + long dimension of spite in the direction of its rotation
				LinearVelocity,
				_area2D.Rotation);
			// GetViewport().SetInputAsHandled(); //??
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		if (Input.IsActionPressed(_ACTION_THRUST))
		{
			ThrustPressed();
		}
		else
		{
			ThrustNotPressed();
		}

		if (Input.IsActionPressed(_ACTION_ROTATE_CW))
		{
			RotateCWPressed(delta);
		}

		if (Input.IsActionPressed(_ACTION_ROTATE_ACW))
		{
			RotateACWPressed(delta);
		}
	}

	private void ThrustPressed()
	{
		ApplyCentralForce(_area2D.Transform.X * _thrustForce);
		LinearDamp = 0;
		_sprite.Play("Thrust");
	}

	private void ThrustNotPressed()
	{
		LinearDamp = 0.5f;
		_sprite.Stop();
	}

	private void RotateCWPressed(double delta)
	{
		_area2D.Rotation = _area2D.Rotation + (_rotationSpeed * (float)delta);
	}

	private void RotateACWPressed(double delta)
	{
		_area2D.Rotation = _area2D.Rotation + (-_rotationSpeed * (float)delta);
	}

	private void Entered(Node2D a3d)
	{
		GD.Print("Entered");
	}
}
