using Godot;
using System;

namespace Asteroids;

public partial class AsteroidExplosion : AnimatedSprite2D
{
	[Signal]
	public delegate void ExplosionCompletedEventHandler(AsteroidExplosion explosion);

	public Vector2 LinearVelocity { get; set; }
	public float AngularVelocity { get; set; }

	public override void _Ready()
	{
		Play();
		GD.Print($"AV {AngularVelocity}");
	}

	public override void _Process(double delta)
	{
		Position += LinearVelocity * (float)delta;
		Rotation += AngularVelocity * (float)delta;
	}

	public void AnimationCompleted()
	{
		EmitSignal(SignalName.ExplosionCompleted, this);
	}
}
