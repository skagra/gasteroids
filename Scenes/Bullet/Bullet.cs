using Godot;

namespace Asteroids;

public partial class Bullet : Area2D
{
    [Signal]
    public delegate void CollidedEventHandler(Bullet asteroid, Node collidedWith);

    public Vector2 Velocity { get; set; } = Vector2.Right;

    [ExportCategory("Testing")]
    [Export]
    private Vector2 _testingLinearVelocity = Vector2.Zero;

    public override void _Process(double delta)
    {
        Position = Screen.Instance.ClampToViewport(Position);
    }

    public override void _Ready()
    {
        AreaEntered += Entered;

        if (GetParent() is Window)
        {
            Position = Screen.Instance.GetCentre();
            Velocity = _testingLinearVelocity;
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        Position += Velocity * (float)delta;
    }

    private void Entered(Area2D collidedWith)
    {
        GD.Print($"Collision detected in '{this.Name}' with '{collidedWith.Name}'");

        EmitSignal(SignalName.Collided, this, collidedWith);
    }
}
