using System;
using System.Collections.Generic;
using System.Diagnostics;
using Godot;

namespace Asteroids;

public partial class PowerUpController : Node
{
    private const int _POWER_UP_THRESHOLD = 10;

    [Signal]
    public delegate void CollectedEventHandler(int powerUpType);

    [Export]
    private AudioStream _collectedSound;
    [Export]
    public EventHub _eventHub;

    public enum PowerUpType { ExtraLife, MultiShot, ReflectiveShot };

    private AudioStreamPlayer _audioStreamPlayer;
    private PackedScene _extraLifePowerUpPrefab = GD.Load<PackedScene>("res://Scenes/Powerup/ExtraLifePowerUp.tscn");
    private PackedScene _multiShotPowerUpPrefab = GD.Load<PackedScene>("res://Scenes/Powerup/MultiShotPowerUp.tscn");
    private PackedScene _reflectiveShotPowerUpPrefab = GD.Load<PackedScene>("res://Scenes/Powerup/ReflectiveShotPowerUp.tscn");

    private class PowerUpDetails
    {
        public PowerUpType PowerUpType { get; set; }
        public PowerUp PowerUp { get; set; }
    }

    private List<PowerUpDetails> _activePowerUps = new();

    private bool _fxEnabled = false;
    public void EnableFx(bool enable)
    {
        _fxEnabled = enable;
    }

    public override void _Ready()
    {
        Debug.Assert(_collectedSound != null);
        _audioStreamPlayer = new()
        {
            Stream = _collectedSound
        };
        AddChild(_audioStreamPlayer);

        _eventHub.AsteroidCollided += OnAsteroidCollided;
        if (GetParent() is Window)
        {
            SpawnPowerUp(Screen.Centre);
        }
    }

    private void OnAsteroidCollided(Asteroid asteroid, AsteroidSize size, Node collidedWith)
    {
        if (Identities.IsPlayerMissile(collidedWith))
        {
            if ((GD.Randi() % _POWER_UP_THRESHOLD) == 0)
            {
                SpawnPowerUp(asteroid.Position);
            }
        }
    }

    public void SpawnPowerUp(Vector2 location)
    {
        var powerUpType = (PowerUpType)(GD.Randi() % Enum.GetValues<PowerUpType>().Length);
        var powerUp = powerUpType switch
        {
            PowerUpType.ExtraLife => _extraLifePowerUpPrefab.Instantiate<PowerUp>(),
            PowerUpType.MultiShot => _multiShotPowerUpPrefab.Instantiate<PowerUp>(),
            PowerUpType.ReflectiveShot => _reflectiveShotPowerUpPrefab.Instantiate<PowerUp>(),
            _ => throw new NotImplementedException()
        };
        powerUp.Position = location;
        powerUp.Collected += PowerUpOnCollected;
        powerUp.Done += PowerUpOnDone;

        _activePowerUps.Add(new PowerUpDetails
        {
            PowerUp = powerUp,
            PowerUpType = powerUpType,
        });

        CallDeferred(MethodName.AddChild, powerUp);
    }

    private void Destroy(PowerUp powerUp)
    {
        var powerUpDetailsIndex = _activePowerUps.FindIndex(ap => ap.PowerUp == powerUp);
        var powerUpDetails = _activePowerUps[powerUpDetailsIndex];

        powerUpDetails.PowerUp.QueueFree();
        _activePowerUps.RemoveAt(powerUpDetailsIndex);
    }

    private void PowerUpOnDone(PowerUp powerUp)
    {
        Destroy(powerUp);
    }

    private void PowerUpOnCollected(PowerUp powerUp, Area2D collidedWith)
    {
        if (_fxEnabled)
        {
            _audioStreamPlayer.Play();
        }
        var powerUpDetails = _activePowerUps.Find(ap => ap.PowerUp == powerUp);
        EmitSignal(SignalName.Collected, (int)powerUpDetails.PowerUpType);
    }
}
