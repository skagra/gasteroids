using System;
using Godot;

namespace Asteroids;

public partial class Saucer : Area2D
{
    [Signal]
    public delegate void CollidedEventHandler(Saucer saucer, Node collidedWith);

    [Signal]
    public delegate void OffScreenEventHandler(Saucer saucer);

    [Export]
    public float Speed { get; set; } = 250f;

    [Export]
    public float MinPathDuration { get; set; } = 0.25f;

    [Export]
    public float MaxPathDuration { get; set; } = 0.5f;

    [Export]
    private AudioStream _saucerSound;

    private CollisionPolygon2D _collisionPolygon;

    private float _pathTimer;
    private float _pathDuration;
    private AudioStreamPlayer _saucerSoundPlayer = new();

    private Vector2 _velocity;
    public Vector2 Velocity { get => _velocity; }

    public bool IsActive { get => _isActive; }

    private enum Direction { LeftToRight, RightToLeft };

    private Direction _direction;

    private Explosion _explosion;

    private bool _isActive = false;

    private float _spawnTimer;

    public override void _Ready()
    {
        _collisionPolygon = GetNode<CollisionPolygon2D>("CollisionPolygon2D");

        _saucerSoundPlayer.Bus = Resources.AUDIO_BUS_NAME_FX;
        _saucerSoundPlayer.Stream = _saucerSound ?? throw new NullReferenceException("Saucer sound not set");
        AddChild(_saucerSoundPlayer);

        AreaEntered += Entered;

        Deactivate();
    }

    private bool _fxEnabled = true;

    public void EnableFx(bool enable)
    {
        _fxEnabled = enable;
    }

    private bool _collisionFlaggedThisFrame = false;

    private void Entered(Area2D collidedWith)
    {
        if (_isActive)
        {
            Deactivate();
            if (!_collisionFlaggedThisFrame)
            {
                Logger.I.SignalSent(this, SignalName.Collided, collidedWith);

                _collisionFlaggedThisFrame = true;

                Logger.I.SignalSent(this, SignalName.Collided, collidedWith);
                EmitSignal(SignalName.Collided, this, collidedWith);
            }
        }
    }

    public void Activate()
    {
        if (_fxEnabled)
        {
            _saucerSoundPlayer.Play();
        }

        _direction = (GD.Randi() % 2) switch
        {
            0 => Direction.LeftToRight,
            1 => Direction.RightToLeft,
            _ => throw new NotImplementedException()
        };

        Position = new Vector2(_direction == Direction.LeftToRight ? Screen.Left : Screen.Right,
                               (float)GD.RandRange(Screen.Top, Screen.Bottom));

        NewPath();
        this.Enable(true);
        _collisionPolygon.Disabled = false;

        _isActive = true;
        Show();
    }

    public void Deactivate()
    {
        Hide();
        _isActive = false;

        this.EnableDeferred(false);
        SetDeferred(CollisionPolygon2D.PropertyName.Disabled, true);

        _saucerSoundPlayer.Stop();
    }

    private void NewPath()
    {
        _pathTimer = 0;
        _pathDuration = (float)GD.RandRange(MinPathDuration, MaxPathDuration);
        _velocity = new Vector2(_direction == Direction.LeftToRight ? 1 : -1,
                                (float)GD.RandRange(-1f, 1f)).
                    Normalized() * Speed;
    }

    private void SwitchDirectionIfNeeded()
    {
        if (_pathTimer > _pathDuration)
        {
            NewPath();
        }
    }

    public override void _Process(double delta)
    {
        if (_isActive)
        {
            _collisionFlaggedThisFrame = false;

            _pathTimer += (float)delta;
            SwitchDirectionIfNeeded();
            Position += _velocity * (float)delta;

            if (_fxEnabled && !_saucerSoundPlayer.IsPlaying())
            {
                _saucerSoundPlayer.Play();
            }
            ;
            if ((_direction == Direction.LeftToRight && Position.X >= Screen.Right) ||
                (_direction == Direction.RightToLeft && Position.X <= Screen.Left))
            {
                Deactivate();

                Logger.I.SignalSent(this, SignalName.OffScreen);
                EmitSignal(SignalName.OffScreen, this);
            }

            Position = Screen.ClampToViewport(Position);
        }
    }
}
