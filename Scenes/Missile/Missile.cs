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

    // Linear velocity
    public Vector2 Velocity { get; set; } = Vector2.Right;

    public override void _Process(double delta)
    {
        Position = Screen.Instance.ClampToViewport(Position);
    }

    public override void _Ready()
    {
        AreaEntered += Entered;

        if (GetParent() is Window)
        {
            Position = Screen.Instance.Centre;
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
