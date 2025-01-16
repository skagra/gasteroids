using Godot;

namespace Asteroids;

public partial class Missile : Area2D
{
    // Signals
    [Signal]
    public delegate void CollidedEventHandler(Missile asteroid, Node collidedWith);

    // Values configurable via the inspector
    [ExportCategory("Testing")]
    [Export]
    private Vector2 _testingLinearVelocity = Vector2.Zero;

    public enum ShotModeType { Wrap, Reflect }
    public ShotModeType ShotMode { get; set; } = ShotModeType.Wrap;

    // Linear velocity
    public Vector2 Velocity { get; set; } = Vector2.Right;

    public override void _Process(double delta)
    {
        if (ShotMode == ShotModeType.Wrap)
        {
            Position = Screen.ClampToViewport(Position);
        }
        else
        {
            Velocity = Reflect();
        }
    }

    private Vector2 Reflect()
    {
        var result = Velocity;

        if (Position.X <= Screen.Left || Position.X >= Screen.Right)
        {
            result.X *= -1;
        }

        if (Position.Y <= Screen.Top || Position.Y >= Screen.Bottom)
        {
            result.Y *= -1;
        }

        return result;
    }

    public override void _Ready()
    {
        AreaEntered += Entered;

        if (GetParent() is Window)
        {
            Position = Screen.Centre;
            Velocity = _testingLinearVelocity;
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        Position += Velocity * (float)delta;
    }

    private void Entered(Area2D collidedWith)
    {
        Logger.I.SignalSent(this, SignalName.Collided, collidedWith);
        EmitSignal(SignalName.Collided, this, collidedWith);
    }
}
