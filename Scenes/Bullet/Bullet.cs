using Godot;

namespace Asteroids;

public partial class Bullet : Area2D
{
	public Vector2 Velocity { get; set; } = Vector2.Right;

	public override void _PhysicsProcess(double delta)
	{
		Position += Velocity * (float)delta;
	}
}
