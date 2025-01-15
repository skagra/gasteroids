using System;
using System.Collections.Generic;
using Godot;

namespace Asteroids;

public partial class PowerUpController : Node
{
    [Signal]
    public delegate void CollectedEventHandler(int powerUpType);

    [Export]
    public EventHub _eventHub;

    public enum PowerUpType { ExtraLife };

    public PackedScene _extraLifePowerUpPrefab = GD.Load<PackedScene>("res://Scenes/Powerup/PowerUp.tscn");

    private class PowerUpDetails
    {
        public PowerUpType PowerUpType { get; set; }
        public PowerUp PowerUp { get; set; }
    }

    private List<PowerUpDetails> _activePowerUps = new();

    public override void _Ready()
    {
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
            if (GD.Randi() % 2 == 0)  // TODO
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
        var powerUpDetails = _activePowerUps.Find(ap => ap.PowerUp == powerUp);
        EmitSignal(SignalName.Collected, (int)powerUpDetails.PowerUpType);
    }
}
