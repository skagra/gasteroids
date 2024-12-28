using System;
using Godot;

namespace Asteroids;

public partial class SaucerController : Node
{
    [Signal]
    public delegate void CollidedEventHandler(Saucer saucer, Node collidedWith);

    [Export]
    private PackedScene _explosion;

    [Export]
    private PackedScene _saucerScene;

    [Export]
    public float SpawnTimerMax { get; set; } = 5;

    [Export]
    public float SpawnTimerMin { get; set; } = 2;

    [Export]
    public int MissileSpawnRandomness { get; set; } = 20;

    [Export]
    private AudioStream _explosionSound;

    private Saucer _saucer;

    private MissileController _missileController;

    private float _spawnTimer;

    private readonly AudioStreamPlayer2D _explosionPlayer = new();

    public Func<Vector2> TargetCallback
    {
        get; set;
    }

    public bool IsActive { get; private set; }

    public bool IsSaucerActive { get => _saucer.IsActive; }

    private bool _fxEnabled = true;

    public override void _Ready()
    {
        _explosionPlayer.Bus = Constants.AUDIO_BUS_NAME_FX;
        _explosionPlayer.Stream = _explosionSound;
        AddChild(_explosionPlayer);

        _saucer = _saucerScene.Instantiate<Saucer>();
        AddChild(_saucer);

        _missileController = GetNode<MissileController>("MissileController");

        _saucer.Collided += SaucerOnCollided;
        _saucer.OffScreen += SaucerOnOffScreen;

        _missileController.Collided += MissileControllerOnCollided;

        Deactivate();
    }

    public void EnableFx(bool enable)
    {
        _fxEnabled = enable;
        _saucer.EnableFx(enable);
        _missileController.EnableFx(enable);
    }

    public override void _Process(double delta)
    {
        if (IsActive)
        {
            if (_saucer.IsActive)
            {
                if (GD.Randi() % MissileSpawnRandomness == 0)
                {
                    SpawnMissile();
                }
            }
            else
            {
                _spawnTimer -= (float)delta;
                if (_spawnTimer < 0)
                {
                    _saucer.Activate();
                }
            }
        }
    }

    private void ResetTimer()
    {
        _spawnTimer = (float)GD.RandRange(SpawnTimerMax, SpawnTimerMin);
    }

    public void Activate()
    {
        ResetTimer();
        IsActive = true;
        this.Enable(true);
    }

    public void Deactivate(bool deSpawnAllMissiles = false)
    {
        IsActive = false;
        this.Enable(false);
        _saucer.Deactivate();
        if (deSpawnAllMissiles)
        {
            _missileController.DeSpawnAllMissiles();
        }
    }

    private void MissileControllerOnCollided(Missile missile, Node collidedWith)
    {
        Logger.I.SignalReceived(this, missile, Missile.SignalName.Collided, collidedWith);
        _missileController.DeSpawnMissile(missile);
    }

    private void SpawnMissile()
    {
        // Target player!
        if (TargetCallback != null)
        {
            var target = TargetCallback();
            var directionVector = _saucer.Position.DirectionTo(target);
            var rotation = directionVector.Angle();
            _missileController.SpawnMissile(_saucer.Position, Vector2.Zero, rotation);
        }
        else
        {
            // Random
            var rotation = (float)GD.RandRange(0, 2 * Math.PI);
            _missileController.SpawnMissile(_saucer.Position, _saucer.Velocity, rotation);
        }
    }

    private void SaucerOnCollided(Saucer saucer, Node collidedWith)
    {
        Logger.I.SignalReceived(this, saucer, Saucer.SignalName.Collided, collidedWith);

        _saucer.Deactivate();
        CreateExplosion();

        ResetTimer();

        Logger.I.SignalSent(this, SignalName.Collided, collidedWith);
        EmitSignal(SignalName.Collided, saucer, collidedWith);
    }

    private void SaucerOnOffScreen(Saucer saucer)
    {
        Logger.I.SignalReceived(this, saucer, Saucer.SignalName.OffScreen);
        _saucer.Deactivate();

        ResetTimer();
    }

    private void CreateExplosion()
    {
        if (_fxEnabled)
        {
            _explosionPlayer.Play();
        }

        var explosion = _explosion.Instantiate<Explosion>();
        explosion.Name = "Saucer Explosion";
        explosion.Position = _saucer.Position;
        explosion.AngularVelocity = 0f;
        explosion.LinearVelocity = _saucer.Velocity;
        explosion.ExplosionCompleted += ExplosionOnCompleted;
        explosion.Animation = "Explosion";
    }


    private void ExplosionOnCompleted(Explosion asteroidExplosion)
    {
        Logger.I.SignalReceived(this, asteroidExplosion, Explosion.SignalName.ExplosionCompleted);

        RemoveChild(asteroidExplosion);
        asteroidExplosion.QueueFree();

        ResetTimer();
    }
}
