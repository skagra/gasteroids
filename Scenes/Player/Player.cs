using System;
using System.Diagnostics;
using Godot;

namespace Asteroids;

public partial class Player : RigidBody2D
{
    private const string _ACTION_THRUST = "Thrust";
    private const string _ACTION_ROTATE_CW = "Rotate CW";
    private const string _ACTION_ROTATE_ACW = "Rotate ACW";
    private const string _ACTION_FIRE = "Fire";
    private const string _ACTION_HYPERSPACE = "Hyperspace";

    private const string _ANIMATION_THRUST = "Thrust";

    [Signal]
    public delegate void ShootEventHandler(Vector2 shipPosition, Vector2 shipLinearVelocity, float shipRotation);

    [Signal]
    public delegate void CollidedEventHandler(Player player, Node collidedWith);

    [Signal]
    public delegate void HyperspaceAccidentEventHandler(Player player);

    [ExportCategory("General Settings")]
    [Export]
    public float ThrustForce { get; set; } = 300;
    [Export]
    public float RotationSpeed { get; set; } = 5.0f;

    public Func<int> GetAsteroidsCount { get; set; }

    private Area2D _area2D;
    private AnimatedSprite2D _sprite;
    private CollisionPolygon2D _collisionPolygon;
    private Vector2 _spriteSize;
    private AudioStreamPlayer2D _thrustAudioStream;

    private bool _isActive = false;
    private bool _hasCollidedThisFrame = false;

    public Func<Vector2, Vector2> GravitationalPullCallback
    {
        get; set;
    }

    public override void _Ready()
    {
        _area2D = GetNode<Area2D>("Player Area2D") ?? throw new NullReferenceException("Player Area2D not found");
        _collisionPolygon = _area2D.GetNode<CollisionPolygon2D>("CollisionPolygon2D") ?? throw new NullReferenceException("CollisionPolygon2D not found");
        _sprite = _area2D.GetNode<AnimatedSprite2D>("AnimatedSprite2D") ?? throw new NullReferenceException("AnimatedSprite2D not found");
        _thrustAudioStream = _area2D.GetNode<AudioStreamPlayer2D>("ThrustAudioPlayer") ?? throw new NullReferenceException("ThrustAudioPlayer not found");
        _thrustAudioStream.Bus = Resources.AUDIO_BUS_NAME_FX;

        _area2D.AreaEntered += Area2DAreaEntered;

        // TODO Scaling
        _spriteSize = _sprite.SpriteFrames.GetFrameTexture(_sprite.Animation, _sprite.Frame).GetSize();

        if (GetParent() is not Window)
        {
            Deactivate();
        }
        else
        {
            Activate();
        }
    }

    private bool _fxEnabled = true;

    public void EnableFx(bool enable)
    {
        _fxEnabled = enable;
    }

    public void Activate()
    {
        Activate(Screen.Centre);
    }

    public new float LinearDamp
    {
        get; set;
    }

    public void Activate(Vector2 position)
    {
        Position = position;
        LinearVelocity = Vector2.Zero;
        AngularVelocity = 0f;
        base.LinearDamp = LinearDamp;

        _area2D.Rotation = 0f;
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

        this.Enable(false);

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
            var variableGravity = GravitationalPullCallback?.Invoke(Position) ?? Vector2.Zero;
            ApplyCentralForce(variableGravity);

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

            if (Input.IsActionJustPressed(_ACTION_HYPERSPACE))
            {
                HyperspacePressed();
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
        Position = new Vector2((float)GD.RandRange(Screen.Left, Screen.Right),
                               (float)GD.RandRange(Screen.Top, Screen.Bottom));

        var chanceToExplode = GD.Randi() % 63;
        if ((GD.Randi() % 63) >= GetAsteroidsCount() + 44)
        {
            Logger.I.SignalSent(this, SignalName.HyperspaceAccident);
            EmitSignal(SignalName.HyperspaceAccident, this);
        }
    }

    private void FirePressed()
    {
        var position = Position + ((_spriteSize.X / 2.0f) * Vector2.Right.Rotated(_area2D.Rotation));

        Logger.I.SignalSent(this, SignalName.Shoot, position, LinearVelocity, _area2D.Rotation);
        EmitSignal(SignalName.Shoot, position, LinearVelocity, _area2D.Rotation);
    }

    private void ThrustPressed()
    {
        if (!_thrustAudioStream.Playing && _fxEnabled)
        {
            _thrustAudioStream.Play();
        }
        ApplyCentralForce(_area2D.Transform.X * ThrustForce);
        base.LinearDamp = 0f;
        if (!_sprite.IsPlaying())
        {
            _sprite.Play(_ANIMATION_THRUST);
        }
    }

    private void ThrustNotPressed()
    {
        base.LinearDamp = LinearDamp;
        if (!_sprite.IsPlaying() && _sprite.Animation == _ANIMATION_THRUST)
        {
            _sprite.Stop();
        }
    }

    private void RotateCWPressed(double delta)
    {
        _area2D.Rotation += RotationSpeed * (float)delta;
    }

    private void RotateACWPressed(double delta)
    {
        _area2D.Rotation += -RotationSpeed * (float)delta;
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
