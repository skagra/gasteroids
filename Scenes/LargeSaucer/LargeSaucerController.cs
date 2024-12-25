using System.Net.NetworkInformation;
using Godot;

namespace Asteroids;

public partial class LargeSaucerController : Node
{
    private readonly PackedScene _explosion = GD.Load<PackedScene>("res://Scenes/Explosion/Explosion.tscn");

    private LargeSaucer _largeSaucer;

    public override void _Ready()
    {
        _largeSaucer = GetNode<LargeSaucer>("LargeSaucer");
        _largeSaucer.Collided += LargeSaucerOnCollided;
        _largeSaucer.OffScreen += LargeSaucerOnOffScreen;
    }

    public override void _Process(double delta)
    {
        if (GD.Randi() % 100 == 0 && // TODO
            !_largeSaucer.IsActive)
        {
            _largeSaucer.Activate();
        }
    }

    private void LargeSaucerOnCollided(LargeSaucer largeSaucer, Node collidedWith)
    {
        Logger.I.SignalReceived(this, largeSaucer, LargeSaucer.SignalName.Collided, collidedWith);

        CreateExplosion();
    }

    private void LargeSaucerOnOffScreen(LargeSaucer largeSaucer)
    {
        Logger.I.SignalReceived(this, largeSaucer, LargeSaucer.SignalName.OffScreen);

        _largeSaucer.Deactivate();
    }

    private void CreateExplosion()
    {
        var explosion = _explosion.Instantiate<Explosion>();
        explosion.Position = _largeSaucer.Position;
        explosion.AngularVelocity = 0f;
        explosion.LinearVelocity = _largeSaucer.Velocity;
        explosion.ExplosionCompleted += ExplosionOnCompleted;

        CallDeferred(MethodName.AddChild, explosion);
    }


    private void ExplosionOnCompleted(Explosion asteroidExplosion)
    {
        Logger.I.SignalReceived(this, asteroidExplosion, Explosion.SignalName.ExplosionCompleted);

        RemoveChild(asteroidExplosion);
        asteroidExplosion.QueueFree();
    }
}
