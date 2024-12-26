using Godot;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Asteroids;

public partial class MissileController : Node
{
    // Signals
    [Signal]
    public delegate void CollidedEventHandler(Missile missile, Node collidedWith);

    // Values configurable via the inspector
    [Export]
    public int MissileCount
    {
        get { return _missileCount; }
        set { _missileCount = value; SetUpMissiles(); }
    }

    [Export]
    public float MissileDuration { get; set; } = 0.5f;

    [Export]
    public float MissileSpeed { get; set; } = 200;

    [Export]
    private AudioStream _missileExplosionSound;

    private int _missileCount = 5;

    // [Export] .. TODO and need a player missile inherited scene and need to expose various player and missile controller properties in player controller
    [Export]
    private PackedScene _missileScene;

    private sealed class ActiveMissile
    {
        public Missile Missile { get; set; }
        public double CountDown { get; set; }
    }

    // Missiles not currently on screen
    private readonly List<Missile> _dormantMissiles = new();

    // Missiles on screen
    private readonly List<ActiveMissile> _activeMissiles = new();

    private AudioStreamPlayer2D _shootAudioStream = new();

    public override void _Ready()
    {
        // Audio
        _shootAudioStream.Stream = _missileExplosionSound;
        AddChild(_shootAudioStream);

        SetUpMissiles();
    }

    private void SetUpMissiles()
    {
        // Destroy any old missiles
        foreach (var missile in _activeMissiles)
        {
            missile.Missile.QueueFree();
        }
        _activeMissiles.Clear();

        foreach (var missile in _dormantMissiles)
        {
            missile.QueueFree();
        }
        _dormantMissiles.Clear();

        // Create new missiles
        for (var i = 0; i < MissileCount; i++)
        {
            var dormantMissile = _missileScene.Instantiate<Missile>();
            _dormantMissiles.Add(dormantMissile);
            dormantMissile.Collided += MissileOnCollided;
            AddChild(dormantMissile);
            dormantMissile.Name = $"Missile #{i + 1}";
            DisableMissile(dormantMissile);
        }
    }

    public override void _Process(double delta)
    {
        ClearUpMissiles(delta);
    }

    private void MissileOnCollided(Missile missile, Node collidedWith)
    {
        Logger.I.SignalReceived(this, missile, Missile.SignalName.Collided, collidedWith);
        Logger.I.SignalSent(this, SignalName.Collided, missile, collidedWith);
        EmitSignal(SignalName.Collided, missile, collidedWith);
    }

    public void SpawnMissile(Vector2 position, Vector2 linearVelocity, float rotation)
    {
        // Remove missile from the dormant list and add to the active list
        if (_dormantMissiles.Count > 0)
        {
            _shootAudioStream.Play();

            var newMissile = _dormantMissiles[0];
            _dormantMissiles.RemoveAt(0);
            _activeMissiles.Add(new ActiveMissile { Missile = newMissile, CountDown = MissileDuration });

            newMissile.Position = position;

            newMissile.Velocity = MissileSpeed * Vector2.Right.Rotated(rotation) + linearVelocity;
            EnableMissile(newMissile);
        }
    }

    private static void DisableMissile(Missile missile)
    {
        missile.Hide();
        missile.EnableDeferred(false);
    }

    private static void EnableMissile(Missile missile)
    {
        missile.Show();
        missile.EnableDeferred(true);
    }

    public void DeSpawnMissile(Missile missile)
    {
        var missileDetails = _activeMissiles.Find(missileDetails => missileDetails.Missile == missile);

        // If the missile collides with more than one asteroid in the same frame then this could be null
        if (missileDetails != null)
        {
            DisableMissile(missile);
            _activeMissiles.Remove(missileDetails);
            _dormantMissiles.Add(missile);
        }
    }

    private void ClearUpMissiles(double delta)
    {
        // Move missiles that have timed out to dormant state
        for (var missileIndex = _activeMissiles.Count - 1; missileIndex >= 0; missileIndex--)
        {
            var activeMissile = _activeMissiles[missileIndex];
            activeMissile.CountDown -= delta;
            // Has the missile aged out?
            if (activeMissile.CountDown <= 0)
            {
                // Make the missile dormant
                _dormantMissiles.Add(activeMissile.Missile);
                DisableMissile(activeMissile.Missile);
                _activeMissiles.RemoveAt(missileIndex);
            }
        }
    }
}
