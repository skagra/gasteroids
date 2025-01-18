using System;
using System.Diagnostics;
using Godot;
using static Asteroids.Missile;

namespace Asteroids;

public partial class PlayerController : Node
{
    private const float _MULTI_SHOT_ROTATION = 0.25f;

    [Signal]
    public delegate void ExplodingEventHandler(PlayerController playerController);

    [Signal]
    public delegate void ExplodedEventHandler(PlayerController playerController);

    [Export]
    private PackedScene _explosion;

    [Export]
    private AudioStream _explosionSound;

    [Export]
    public float PlayerThrustForce { get; set; }

    [Export]
    public float PlayerRotationSpeed { get; set; }

    [Export]
    public int MissileCount { get; set; }

    [Export]
    public float MissileDuration { get; set; }

    [Export]
    public float MissileSpeed { get; set; }

    [Export]
    public float ShakeTime { get; set; }

    [Export]
    public float LinearDampening { get; set; }

    public bool MultiShot { get; set; } = false;

    private ShotModeType _shotMode;

    public ShotModeType ShotMode
    {
        get => _shotMode;
        set
        {
            _missileController.ShotMode = value;
            _cwMissileController.ShotMode = value;
            _acwMissileController.ShotMode = value;
        }
    }

    public bool PoweredUp
    {
        get => _player.PoweredUp;
        set => _player.PoweredUp = value;
    }

    public Vector2 PlayerPosition
    {
        get => _player.Position;
    }

    public Func<Vector2, Vector2> PlayerGravitationalPullCallback
    {
        get => _player.GravitationalPullCallback;
        set => _player.GravitationalPullCallback = value;
    }

    public Func<int> GetAsteroidsCount
    {
        get => _player.GetAsteroidsCount;
        set => _player.GetAsteroidsCount = value;
    }

    private Player _player;
    private MissileController _missileController;
    private MissileController _acwMissileController;
    private MissileController _cwMissileController;
    private AudioStreamPlayer _explosionPlayer = new();
    private ShakingCamera _camera;
    private Timer _shakeTimer = new();

    private bool _fxEnabled = true;

    public override void _Ready()
    {
        Debug.Assert(_explosion != null, "Explosion not set");

        _camera = (ShakingCamera)GetViewport().GetCamera2D();
        _shakeTimer.OneShot = true;
        _shakeTimer.Timeout += ShakeTimerOnTimeout;
        AddChild(_shakeTimer);

        _explosionPlayer.Bus = Resources.AUDIO_BUS_NAME_FX;
        _explosionPlayer.Stream = _explosionSound ?? throw new NullReferenceException("Explosion sound not set");
        AddChild(_explosionPlayer);

        _player = GetNode<Player>("Player") ?? throw new NullReferenceException("Player not found");
        _missileController = GetNode<MissileController>("MissileController");
        _cwMissileController = GetNode<MissileController>("CWMissileController");
        _acwMissileController = GetNode<MissileController>("ACWMissileController");

        _missileController.Muted = false;
        _cwMissileController.Muted = true;
        _cwMissileController.Muted = true;

        _player.Collided += PlayerOnCollided;
        _player.Shoot += PlayerOnShoot;

        _player.HyperspaceAccident += PlayerOnHyperspaceAccident;

        _missileController.Collided += MissileControllerOnCollided;
        _cwMissileController.Collided += MissileControllerOnCollided;
        _acwMissileController.Collided += MissileControllerOnCollided;

        if (GetParent() is Window)
        {
            Activate(Screen.Centre);
        }
        else
        {
            Deactivate();
        }
    }

    public void EnableFx(bool enable)
    {
        _fxEnabled = enable;
        _missileController.EnableFx(enable);
        _cwMissileController.EnableFx(enable);
        _acwMissileController.EnableFx(enable);
        _player.EnableFx(enable);
    }

    public void Activate(Vector2 position)
    {
        _player.ThrustForce = PlayerThrustForce;
        _player.RotationSpeed = PlayerRotationSpeed;
        _player.LinearDampening = LinearDampening;

        _missileController.MissileCount = MissileCount;
        _missileController.MissileDuration = MissileDuration;
        _missileController.MissileSpeed = MissileSpeed;

        _cwMissileController.MissileCount = MissileCount;
        _cwMissileController.MissileDuration = MissileDuration;
        _cwMissileController.MissileSpeed = MissileSpeed;

        _acwMissileController.MissileCount = MissileCount;
        _acwMissileController.MissileDuration = MissileDuration;
        _acwMissileController.MissileSpeed = MissileSpeed;

        _player.Activate(position);
    }

    public void Deactivate()
    {
        _player.Deactivate();
    }

    private void PlayerOnShoot(Vector2 position, Vector2 shipLinearVelocity, float shipRotation)
    {
        Logger.I.SignalReceived(this, _player, Player.SignalName.Shoot, position, shipLinearVelocity, shipRotation);
        _missileController.SpawnMissile(position, shipLinearVelocity, shipRotation);
        if (MultiShot)
        {
            _cwMissileController.SpawnMissile(position, shipLinearVelocity, shipRotation + _MULTI_SHOT_ROTATION);
            _acwMissileController.SpawnMissile(position, shipLinearVelocity, shipRotation - _MULTI_SHOT_ROTATION);
        }
    }

    private void MissileControllerOnCollided(Missile missile, Node collidedWith)
    {
        Logger.I.SignalReceived(this, missile, Missile.SignalName.Collided, collidedWith);
        _missileController.DeSpawnMissile(missile);
    }

    private void PlayerOnCollided(Player player, Node collidedWith)
    {
        Logger.I.SignalReceived(this, collidedWith, Area2D.SignalName.AreaEntered);

        Deactivate();
        SpawnExplosion();
    }

    private void PlayerOnHyperspaceAccident(Player player)
    {
        Deactivate();
        SpawnExplosion();
    }

    private void ShakeTimerOnTimeout()
    {
        _camera.Deactivate();
    }

    private void SpawnExplosion()
    {
        if (_fxEnabled)
        {
            _explosionPlayer.Play();
        }

        _camera.Activate();
        _shakeTimer.Start(ShakeTime);

        var playerExplosion = _explosion.Instantiate<Explosion>();
        playerExplosion.Name = "Player Explosion";
        playerExplosion.Position = _player.Position;
        playerExplosion.LinearVelocity = _player.LinearVelocity;
        CallDeferred(MethodName.AddChild, playerExplosion);
        playerExplosion.ExplosionCompleted += ExplosionOnExplosionCompleted;

        Logger.I.SignalSent(this, SignalName.Exploding);
        EmitSignal(SignalName.Exploding, this);
    }

    private void ExplosionOnExplosionCompleted(Explosion playerExplosion)
    {
        Logger.I.SignalReceived(this, playerExplosion, Explosion.SignalName.ExplosionCompleted);

        RemoveChild(playerExplosion);
        playerExplosion.QueueFree();

        EmitSignal(SignalName.Exploded, this);
        Logger.I.SignalSent(this, SignalName.Exploded);
    }
}