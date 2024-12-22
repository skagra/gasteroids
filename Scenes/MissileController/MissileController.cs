using Godot;
using System.Collections.Generic;

namespace Asteroids;

public partial class MissileController : Node
{
    private readonly PackedScene _missileScene = GD.Load<PackedScene>("res://Scenes/Missile/Missile.tscn");
    private readonly List<Missile> _dormantMissiles = new();
    private readonly List<ActiveMissile> _activeMissiles = new();

    [Signal]
    public delegate void CollidedEventHandler(Missile missile, Node collidedWith);

    private sealed class ActiveMissile
    {
        public Missile Missile { get; set; }
        public double CountDown { get; set; }
    }

    [Export]
    public int MissileCount { get; set; } = 5;

    [Export]
    public float MissileDuration { get; set; } = 0.5f;

    [Export]
    public int MissileSpeed { get; set; } = 200;

    private AudioStreamPlayer2D _shootAudioStream;

    public override void _Ready()
    {
        GD.Print($"In MissileController.Ready MissileCount={MissileCount}");

        _shootAudioStream = GetNode<AudioStreamPlayer2D>("ShootAudioPlayer");

        // Missiles
        for (var i = 0; i < MissileCount; i++)
        {
            var dormantMissile = _missileScene.Instantiate<Missile>();
            dormantMissile.Name = $"Missile #{i + 1}";
            _dormantMissiles.Add(dormantMissile);
            dormantMissile.Collided += Entered;
            AddChild(dormantMissile);
            DisableMissile(dormantMissile);
        }
        GD.Print($"Missile direction={MissileDuration}");
    }

    public override void _Process(double delta)
    {
        ClearUpMissiles(delta);
    }

    private void Entered(Missile missile, Node collidedWith)
    {
        GD.Print($"Collision detected in '{this.Name}' with '{collidedWith.Name}'");

        // TODO Trap error condition
        var missileDetails = _activeMissiles.Find(missileDetails => missileDetails.Missile == missile);
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
            // TODO - Right vector!  XXXX
            newMissile.Velocity = MissileSpeed * Vector2.Right.Rotated(rotation) + linearVelocity;
            EnableMissile(newMissile);
        }
    }

    private static void DisableMissile(Missile missile)
    {
        missile.Hide();
        missile.SetProcess(false);
        missile.SetPhysicsProcess(false);
        missile.SetDeferred(Area2D.PropertyName.Monitorable, false);
        missile.SetDeferred(Area2D.PropertyName.Monitoring, false);
    }

    private static void EnableMissile(Missile missile)
    {
        missile.Show();
        missile.SetProcess(true);
        missile.SetPhysicsProcess(true);
        missile.SetDeferred(Area2D.PropertyName.Monitorable, true);
        missile.SetDeferred(Area2D.PropertyName.Monitoring, true);
    }

    public void KillMissile(Missile missile)
    {
        // TODO - Trap error condition
        var missileDetails = _activeMissiles.Find(missileDetails => missileDetails.Missile == missile);
        DisableMissile(missile);
        _activeMissiles.Remove(missileDetails);
        _dormantMissiles.Add(missile);
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
