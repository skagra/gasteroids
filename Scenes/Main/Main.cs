using Godot;
using System;
using System.Collections.Generic;

public partial class Main : Node
{
	private RigidBody2D _player;

	private readonly PackedScene _asteroidScene = GD.Load<PackedScene>("res://Scenes/Asteroid/Asteroid.tscn");
	private readonly PackedScene _bulletScene = GD.Load<PackedScene>("res://Scenes/Bullet/Bullet.tscn");

	private readonly List<Bullet> _dormantBullets = new();
	private readonly List<ActiveBullet> _activeBullets = new();

	private sealed class ActiveBullet
	{
		public Bullet Bullet { get; set; }
		public ulong SpawnTime { get; set; }
	}

	private int _bulletCount = 5;
	public override void _EnterTree()
	{
		// var instance = _asteroidScene.Instantiate<RigidBody2D>();
		// instance.Position = new Vector2(600, 300);
		// AddChild(instance);

		// Missiles
		for (var i = 0; i < _bulletCount; i++)
		{
			var dormantBullet = _bulletScene.Instantiate<Bullet>();
			_dormantBullets.Add(dormantBullet);
			//AddChild(dormantBullet);
		}
	}

	private ulong _bulletDurationMs = 1000;
	private void ClearUpBullets()
	{
		// Move missiles that have timed out to dormant state
		for (var bulletIndex = _activeBullets.Count - 1; bulletIndex >= 0; bulletIndex--)
		{
			var activeBullet = _activeBullets[bulletIndex];

			// Has the missile aged out?
			if (Time.GetTicksMsec() - activeBullet.SpawnTime > _bulletDurationMs)
			{
				// Make the missile dormant
				_dormantBullets.Add(activeBullet.Bullet);
				RemoveChild(activeBullet.Bullet);
				_activeBullets.RemoveAt(bulletIndex);
			}
		}
	}

	private void OnPlayerShoot(Vector2 position, Vector2 shipLinearVelocity, float shipRotation) // lv should be speed instead
	{
		if (_dormantBullets.Count > 0)
		{
			// Remove missile from the dormant list and add to the active list
			var newBullet = _dormantBullets[0];
			_dormantBullets.RemoveAt(0);
			_activeBullets.Add(new ActiveBullet { Bullet = newBullet, SpawnTime = Time.GetTicksMsec() });

			newBullet.Position = position;
			newBullet.Velocity = 300f * Vector2.Right.Rotated(shipRotation) + shipLinearVelocity; // TODO - Right vector!
			AddChild(newBullet);
		}
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_player = GetNode<RigidBody2D>("Player");
		var vp = GetViewport().GetVisibleRect();
		_player.Position = new Vector2(vp.Size.X / 2.0f, vp.Size.Y / 2.0f);

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		ClearUpBullets();
	}
}
