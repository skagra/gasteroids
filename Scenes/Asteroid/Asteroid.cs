using Godot;
using System;

public partial class Asteroid : RigidBody2D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		BodyEntered += Entered;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	private void Entered(Node a3d)
	{
		GD.Print("Asteroid Detected Collision");

	}
}
