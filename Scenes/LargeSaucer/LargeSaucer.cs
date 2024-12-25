using System;
using Godot;

namespace Asteroids;

public partial class LargeSaucer : Area2D
{
    [Signal]
    public delegate void CollidedEventHandler(LargeSaucer largeSaucer, Node collidedWith);

    [Signal]
    public delegate void OffScreenEventHandler(LargeSaucer largeSaucer);

    [Export]
    private float Speed { get; set; } = 250f;

    [Export]
    private float MinPathDuration { get; set; } = 0.25f;

    [Export]
    private float MaxPathDuration { get; set; } = 0.5f;

    [Export]
    private AudioStream _saucerSound;

    private CollisionPolygon2D _collisionPolygon;

    private float _pathTimer;
    private float _pathDuration;
    private AudioStreamPlayer2D _saucerSoundPlayer = new();

    private Vector2 _velocity;
    public Vector2 Velocity { get => _velocity; }

    public bool IsActive { get => _isActive; }

    private enum Direction { LeftToRight, RightToLeft };

    private Direction _direction;

    private Explosion _explosion;

    private bool _isActive = false;

    public override void _Ready()
    {
        _collisionPolygon = GetNode<CollisionPolygon2D>("CollisionPolygon2D");

        _saucerSoundPlayer.Stream = _saucerSound;
        AddChild(_saucerSoundPlayer);

        AreaEntered += Entered;

        _isActive = false;

        Deactivate();
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

                EmitSignal(SignalName.Collided, this, collidedWith);
            }
        }
    }

    public void Activate()
    {
        _saucerSoundPlayer.Play();

        _direction = (GD.Randi() % 2) switch
        {
            0 => Direction.LeftToRight,
            1 => Direction.RightToLeft,
            _ => throw new NotImplementedException()
        };

        Position = new Vector2(_direction == Direction.LeftToRight ? Screen.Instance.Left : Screen.Instance.Right,
                               (float)GD.RandRange(Screen.Instance.Top, Screen.Instance.Bottom));

        NewPath();
        SetProcess(true);
        SetPhysicsProcess(true);
        Monitorable = true;
        Monitoring = true;
        _collisionPolygon.Disabled = false;

        _isActive = true;
        Show();
    }

    public void Deactivate()
    {
        Hide();
        _isActive = false;

        SetProcess(false);
        SetPhysicsProcess(false);
        SetDeferred(PropertyName.Monitorable, false);
        SetDeferred(PropertyName.Monitoring, false);
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

            if (!_saucerSoundPlayer.IsPlaying())
            {
                _saucerSoundPlayer.Play();
            };
            if ((_direction == Direction.LeftToRight && Position.X >= Screen.Instance.Right) ||
                (_direction == Direction.RightToLeft && Position.X <= Screen.Instance.Left))
            {
                Deactivate();

                Logger.I.SignalSent(this, SignalName.OffScreen);
                EmitSignal(SignalName.OffScreen, this);
            }

            Position = Screen.Instance.ClampToViewport(Position);
        }
    }
}
