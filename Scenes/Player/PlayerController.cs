using System;
using Godot;

namespace Asteroids;

public partial class PlayerController : Node
{
    [Signal]
    public delegate void ExplodingEventHandler();

    [Signal]
    public delegate void ExplodedEventHandler();

    [Export]
    private PackedScene _explosion;

    [Export]
    public float PlayerThrustForce
    {
        get => _player.ThrustForce;
        set => _player.ThrustForce = value;
    }

    [Export]
    public float PlayerRotationSpeed
    {
        get => _player.RotationSpeed;
        set => _player.RotationSpeed = value;
    }

    [Export]
    public int MissileCount
    {
        get => _missileController.MissileCount;
        set => _missileController.MissileCount = value;
    }

    [Export]
    public float MissileDuration
    {
        get => _missileController.MissileDuration;
        set => _missileController.MissileDuration = value;
    }

    [Export]
    public float MissileSpeed
    {
        get => _missileController.MissileSpeed;
        set => _missileController.MissileSpeed = value;
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

    private Player _player;
    private MissileController _missileController;

    public override void _Ready()
    {
        _player = GetNode<Player>("Player");
        _missileController = GetNode<MissileController>("MissileController");

        _player.Collided += PlayerOnCollided;
        _player.Shoot += PlayerOnShoot;

        _missileController.Collided += MissileControllerOnCollided;

        _player.Deactivate();
    }

    public void Activate()
    {
        _player.Activate();
    }

    public void Activate(Vector2 position)
    {
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

    private void SpawnExplosion()
    {
        var playerExplosion = _explosion.Instantiate<Explosion>();
        playerExplosion.Name = "Player Explosion";
        playerExplosion.Position = _player.Position;
        playerExplosion.AngularVelocity = _player.AngularVelocity;
        playerExplosion.LinearVelocity = _player.LinearVelocity;
        CallDeferred(MethodName.AddChild, playerExplosion);
        playerExplosion.ExplosionCompleted += ExplosionOnExplosionCompleted;

        Logger.I.SignalSent(this, SignalName.Exploding);
        EmitSignal(SignalName.Exploding);
    }

    private void ExplosionOnExplosionCompleted(Explosion playerExplosion)
    {
        Logger.I.SignalReceived(this, playerExplosion, Explosion.SignalName.ExplosionCompleted);

        RemoveChild(playerExplosion);
        playerExplosion.QueueFree();

        EmitSignal(SignalName.Exploded);
        Logger.I.SignalSent(this, SignalName.Exploded);

    }
}