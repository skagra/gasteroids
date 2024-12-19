using System;
using Godot;

namespace Asteroids;

public partial class Asteroid : Area2D
{
    [Signal]
    public delegate void CollidedEventHandler(Asteroid asteroid, Node2D collidedWith);

    public Vector2 LinearVelocity { get; set; }
    public float AngularVelocity { get; set; }

    [ExportCategory("Testing")]
    [Export]
    private Vector2 _testingLinearVelocity;
    [Export]
    float _testingAngularVelocity;

    public override void _Ready()
    {
        GD.Print("Asteroid ready!");
        AreaEntered += Entered;

        if (GetParent() is Window)
        {
            Position = GetViewportRect().Size * 0.5f;
            LinearVelocity = _testingLinearVelocity;
            AngularVelocity = _testingAngularVelocity;
        }
    }

    public override void _Process(double delta)
    {
        Position += LinearVelocity * (float)delta;
        Position = Screen.Instance.ClampToViewport(Position);
        Rotation += AngularVelocity * (float)delta;
    }

    private void Entered(Area2D collidedWith)
    {
        GD.Print("Asteroid Entered");
        EmitSignal(SignalName.Collided, this, collidedWith);
    }
}
