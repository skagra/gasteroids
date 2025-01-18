using System;
using System.Diagnostics;
using Godot;

namespace Asteroids;

public partial class Player : Area2D
{
    private const string _ACTION_THRUST = "Thrust";
    private const string _ACTION_ROTATE_CW = "Rotate CW";
    private const string _ACTION_ROTATE_ACW = "Rotate ACW";
    private const string _ACTION_FIRE = "Fire";
    private const string _ACTION_HYPERSPACE = "Hyperspace";

    private const string _ANIMATION_THRUST = "Thrust";

    private const float _MAX_SPEED = 1500f;

    private readonly Vector2 PoweredUpScale = new(1.3f, 1.3f);

    [Signal]
    public delegate void ShootEventHandler(Vector2 shipPosition, Vector2 shipLinearVelocity, float shipRotation);

    [Signal]
    public delegate void CollidedEventHandler(Player player, Node collidedWith);

    [Signal]
    public delegate void HyperspaceAccidentEventHandler(Player player);

    [ExportCategory("General Settings")]
    [Export]
    private AudioStream _thrustAudioStream;

    [Export]
    public float ThrustForce { get; set; } = 100f;
    [Export]
    public float RotationSpeed { get; set; } = 5.0f;
    [Export]
    public float LinearDampening { get; set; } = 100f;

    public Func<int> GetAsteroidsCount { get; set; }
    public bool PoweredUp { get; set; } = false;

    public Vector2 LinearVelocity { get; private set; }

    private AnimatedSprite2D _sprite;
    private CollisionPolygon2D _collisionPolygon;
    private Vector2 _spriteSize;
    private AudioStreamPlayer _thrustAudioStreamPlayer;

    private bool _isActive = false;
    private bool _hasCollidedThisFrame = false;
    private bool _fxEnabled = true;

    public Func<Vector2, Vector2> GravitationalPullCallback
    {
        get; set;
    }

    public override void _Ready()
    {
        _sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D") ?? throw new NullReferenceException("AnimatedSprite2D not found");
        _collisionPolygon = GetNode<CollisionPolygon2D>("CollisionPolygon2D") ?? throw new NullReferenceException("CollisionPolygon2D not found");
        _thrustAudioStreamPlayer = new()
        {
            Stream = _thrustAudioStream,
            Bus = Resources.AUDIO_BUS_NAME_FX
        };
        AddChild(_thrustAudioStreamPlayer);

        AreaEntered += Area2DAreaEntered;

        _spriteSize = _sprite.SpriteFrames.GetFrameTexture(_sprite.Animation, _sprite.Frame).GetSize();

        if (GetParent() is Window)
        {
            Activate();
        }
        else
        {
            Deactivate();
        }
    }

    public void EnableFx(bool enable)
    {
        _fxEnabled = enable;
    }

    public void Activate()
    {
        Activate(Screen.Centre);
    }

    public void Activate(Vector2 position)
    {
        Position = position;
        Rotation = 0f;

        LinearVelocity = Vector2.Zero;

        _collisionPolygon.Disabled = false;

        _sprite.SetAnimation(_ANIMATION_THRUST);

        _hasCollidedThisFrame = false;

        this.Enable(true);

        Show();

        _isActive = true;
    }

    public void Deactivate()
    {
        Hide();

        _collisionPolygon.SetDeferred(CollisionPolygon2D.PropertyName.Disabled, true);

        this.EnableDeferred(false);

        _isActive = false;
    }

    public override void _Input(InputEvent inputEvent)
    {
        if (_isActive)
        {
            if (inputEvent.IsActionPressed(_ACTION_FIRE))
            {
                FirePressed();
            }
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_isActive)
        {
            if (PoweredUp)
            {
                _sprite.Scale = PoweredUpScale;
            }
            else
            {
                _sprite.Scale = Vector2.One;
            }

            var variableGravity = GravitationalPullCallback?.Invoke(Position) ?? Vector2.Zero;
            LinearVelocity += variableGravity * (float)delta;

            if (Input.IsActionPressed(_ACTION_THRUST))
            {
                ThrustPressed(delta);
            }
            else
            {
                ThrustNotPressed(delta);
            }

            if (Input.IsActionPressed(_ACTION_ROTATE_CW))
            {
                RotateCWPressed(delta);
            }

            if (Input.IsActionPressed(_ACTION_ROTATE_ACW))
            {
                RotateACWPressed(delta);
            }

            if (Input.IsActionJustPressed(_ACTION_HYPERSPACE))
            {
                HyperspacePressed();
            }

            if (LinearVelocity.Length() > _MAX_SPEED)
            {
                LinearVelocity = LinearVelocity.Normalized() * _MAX_SPEED;
            }

        }
        Position = Screen.ClampToViewport(Position);
    }

    // Current position + long dimension of spite in the direction of its rotation
    // https://steamcommunity.com/app/400020/discussions/0/3855581634402220944/
    // It looks like the exact odds are: choose a random even number from 0-62, explode if 
    // random_number >= number_of_asteroids + 44. 
    // So, looks like hyperspace should never randomly explode if there are at least 19 asteroids on the screen. 
    // Anything less, and there's a chance. But that's just eyeballing the code
    private void HyperspacePressed()
    {
        Debug.Assert(GetAsteroidsCount != null, "GetAsteroidsCount is null");

        Position = new Vector2((float)GD.RandRange(Screen.Left, Screen.Right),
                               (float)GD.RandRange(Screen.Top, Screen.Bottom));

        if ((GD.Randi() % 63) >= GetAsteroidsCount() + 44)
        {
            Logger.I.SignalSent(this, SignalName.HyperspaceAccident);
            EmitSignal(SignalName.HyperspaceAccident, this);
        }
    }

    private void FirePressed()
    {
        var position = Position + ((_spriteSize.X / 2.0f) * Vector2.Right.Rotated(Rotation));

        Logger.I.SignalSent(this, SignalName.Shoot, position, LinearVelocity, Rotation);
        EmitSignal(SignalName.Shoot, position, LinearVelocity, Rotation);
    }

    private void ThrustPressed(double delta)
    {
        if (!_thrustAudioStreamPlayer.Playing && _fxEnabled)
        {
            _thrustAudioStreamPlayer.Play();
        }

        var acceleration = ThrustForce * (float)delta;
        var shipDirectionUnitVector = Vector2.Right.Rotated(Rotation);
        LinearVelocity += shipDirectionUnitVector * acceleration;
        Position += LinearVelocity * (float)delta;

        if (!_sprite.IsPlaying())
        {
            _sprite.Play(_ANIMATION_THRUST);
        }
    }

    private void ThrustNotPressed(double delta)
    {
        var acceleration = LinearDampening * (float)delta;
        LinearVelocity -= LinearVelocity.Normalized() * acceleration;

        Position += LinearVelocity * (float)delta;

        if (!_sprite.IsPlaying() && _sprite.Animation == _ANIMATION_THRUST)
        {
            _sprite.Stop();
        }
    }

    private void RotateCWPressed(double delta)
    {
        Rotation += RotationSpeed * (float)delta;
    }

    private void RotateACWPressed(double delta)
    {
        Rotation += -RotationSpeed * (float)delta;
    }

    private void Area2DAreaEntered(Area2D collidedWith)
    {
        Logger.I.SignalReceived(this, collidedWith, Area2D.SignalName.AreaEntered);

        if (_isActive && !_hasCollidedThisFrame)
        {
            _hasCollidedThisFrame = true;

            Logger.I.SignalSent(this, SignalName.Collided, collidedWith);
            EmitSignal(SignalName.Collided, this, collidedWith);
        }
    }
}
