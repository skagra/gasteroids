using System;
using Godot;

namespace Asteroids;

public partial class Asteroid : Area2D
{
	[Signal]
	public delegate void CollidedEventHandler(Asteroid asteroid, Node2D collidedWith);

	public Vector2 LinearVelocity { get; set; }
	public float AngularVelocity { get; set; }

	public override void _Ready()
	{
		GD.Print("Asteroid ready!");
		AreaEntered += Entered;

		if (GetParent() is Window)
		{
			Position = GetViewportRect().Size * 0.5f;
			LinearVelocity = new Vector2(20, 30);
			AngularVelocity = 2.0f;
		}
	}

	public override void _Process(double delta)
	{
		Position += LinearVelocity * (float)delta;
		Position = Screen.Instance.ClampToViewport(Position, GetViewport());
		Rotation += AngularVelocity * (float)delta;
	}

	private void Entered(Area2D collidedWith)
	{
		GD.Print("Asteroid Entered");
		EmitSignal(SignalName.Collided, this, collidedWith);
	}
}
