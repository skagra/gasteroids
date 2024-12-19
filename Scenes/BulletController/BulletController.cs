using Godot;
using System.Collections.Generic;

namespace Asteroids;

public partial class BulletController : Node
{
    private readonly PackedScene _bulletScene = GD.Load<PackedScene>("res://Scenes/Bullet/Bullet.tscn");
    private readonly List<Bullet> _dormantBullets = new();
    private readonly List<ActiveBullet> _activeBullets = new();

    [Signal]
    public delegate void CollidedEventHandler(Bullet bullet, Node collidedWith);

    private sealed class ActiveBullet
    {
        public Bullet Bullet { get; set; }
        public ulong SpawnTime { get; set; }
    }

    [Export]
    public int BulletCount { get; set; } = 5;

    [Export]
    private ulong BulletDurationMs { get; set; } = 1000;

    public override void _Ready()
    {
        // Missiles
        for (var i = 0; i < BulletCount; i++)
        {
            var dormantBullet = _bulletScene.Instantiate<Bullet>();
            dormantBullet.Name = $"Bullet #{i + 1}";
            _dormantBullets.Add(dormantBullet);
            dormantBullet.Collided += Entered;
        }
    }

    public override void _Process(double delta)
    {
        ClearUpBullets();
    }

    private void Entered(Bullet bullet, Node collidedWith)
    {
        GD.Print("Bullet hit something");
        // TODO Trap error condition
        var bulletDetails = _activeBullets.Find(bulletDetails => bulletDetails.Bullet == bullet);
        EmitSignal(SignalName.Collided, bullet, collidedWith);
    }

    public void SpawnBullet(Vector2 position, Vector2 linearVelocity, float rotation)
    {
        // Remove missile from the dormant list and add to the active list
        if (_dormantBullets.Count > 0)
        {
            var newBullet = _dormantBullets[0];
            _dormantBullets.RemoveAt(0);
            _activeBullets.Add(new ActiveBullet { Bullet = newBullet, SpawnTime = Time.GetTicksMsec() });

            newBullet.Position = position;
            newBullet.Velocity = 300f * Vector2.Right.Rotated(rotation) + linearVelocity; // TODO - Right vector! TODO Hard coded speed
            AddChild(newBullet);
        }
    }

    public void KillBullet(Bullet bullet)
    {
        // TODO - Trap error condition
        var bulletDetails = _activeBullets.Find(bulletDetails => bulletDetails.Bullet == bullet);
        CallDeferred(MethodName.RemoveChild, bullet);

        _activeBullets.Remove(bulletDetails);
        _dormantBullets.Add(bullet);
    }

    private void ClearUpBullets()
    {
        // Move missiles that have timed out to dormant state
        for (var bulletIndex = _activeBullets.Count - 1; bulletIndex >= 0; bulletIndex--)
        {
            var activeBullet = _activeBullets[bulletIndex];

            // Has the missile aged out?
            if (Time.GetTicksMsec() - activeBullet.SpawnTime > BulletDurationMs)
            {
                // Make the missile dormant
                _dormantBullets.Add(activeBullet.Bullet);
                RemoveChild(activeBullet.Bullet);
                _activeBullets.RemoveAt(bulletIndex);
            }
        }
    }
}
