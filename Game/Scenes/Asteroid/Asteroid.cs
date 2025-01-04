using Godot;

namespace Asteroids;

public partial class Asteroid : Area2D
{
    [Signal]
    public delegate void CollidedEventHandler(Asteroid asteroid, Node2D collidedWith);

    [Export]
    public Vector2 LinearVelocity { get; set; }
    public float AngularVelocity { get; set; }

    public override void _Ready()
    {
        AreaEntered += Area2DOnEntered;

        if (GetParent() is Window)
        {
            Position = Screen.Centre;
        }
    }

    public override void _Process(double delta)
    {
        Position += LinearVelocity * (float)delta;
        Position = Screen.ClampToViewport(Position);
        Rotation += AngularVelocity * (float)delta;
    }

    private void Area2DOnEntered(Area2D collidedWith)
    {
        Logger.I.SignalSent(this, SignalName.Collided, collidedWith);
        EmitSignal(SignalName.Collided, this, collidedWith);
    }
}
