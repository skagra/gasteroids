using System;
using System.Collections.Generic;
using Godot;

namespace Asteroids;

public partial class AsteroidField : Node
{
	[Signal]
	public delegate void CollisionEventHandler(Asteroid asteroid, AsteroidSize size, Node collidedWith);

	[Signal]
	public delegate void FieldClearedEventHandler();

	private class AsteroidDetails
	{
		// The asteroid
		public Asteroid Asteroid { get; set; }

		// Size of the asteroid
		public AsteroidSize AsteroidSize { get; set; }

		// Flag the asteroid for destruction on the next cycle
		public bool ReadyForCleanUp { get; set; } = false;
	}

	[ExportCategory("Main")]

	[ExportGroup("Speed")]
	[Export]
	public float MinSpeed { get; set; } = 5;
	[Export]
	public float MaxSpeed { get; set; } = 200;

	[ExportGroup("Rotation")]
	[Export]
	public bool IsRotationEnabled { get; set; } = true;
	[Export]
	public float MinRadiansPerSecond { get; set; } = -0.3f;
	[Export]
	public float MaxRadiansPerSecond { get; set; } = 3.0f;

	[ExportCategory("Testing")]
	[Export(PropertyHint.Range, "0,100,")]
	private int _testingNumAsteroids = 10;

	private const string _ASTEROID_SCENE_BASE = "res://Scenes/Asteroid/";

	private readonly List<PackedScene> _largeAsteroidPrefabs =
		new() { GD.Load<PackedScene>($"{_ASTEROID_SCENE_BASE}AsteroidType1Large.tscn"),
				GD.Load<PackedScene>($"{_ASTEROID_SCENE_BASE}AsteroidType2Large.tscn"),
				GD.Load<PackedScene>($"{_ASTEROID_SCENE_BASE}AsteroidType3Large.tscn") };
	private readonly List<PackedScene> _mediumAsteroidPrefabs =
		new() { GD.Load<PackedScene>($"{_ASTEROID_SCENE_BASE}AsteroidType1Medium.tscn"),
				GD.Load<PackedScene>($"{_ASTEROID_SCENE_BASE}AsteroidType2Medium.tscn"),
				GD.Load<PackedScene>($"{_ASTEROID_SCENE_BASE}AsteroidType3Medium.tscn") };
	private readonly List<PackedScene> _smallAsteroidPrefabs =
		new() { GD.Load<PackedScene>($"{_ASTEROID_SCENE_BASE}AsteroidType1Small.tscn"),
				GD.Load<PackedScene>($"{_ASTEROID_SCENE_BASE}AsteroidType2Small.tscn"),
				GD.Load<PackedScene>($"{_ASTEROID_SCENE_BASE}AsteroidType3Small.tscn") };

	private readonly PackedScene _asteroidExplosion = GD.Load<PackedScene>($"{_ASTEROID_SCENE_BASE}AsteroidExplosion.tscn");

	// List of all active asteroids
	private readonly List<AsteroidDetails> _activeAsteroids = new();

	private AudioStream _bangLarge = GD.Load<AudioStream>("res://Audio/bangLarge.wav");
	private AudioStream _bangMedium = GD.Load<AudioStream>("res://Audio/bangMedium.wav");
	private AudioStream _bangSmall = GD.Load<AudioStream>("res://Audio/bangSmall.wav");

	private readonly AudioStreamPlayer2D _audioStreamPlayerBangLarge = new();
	private readonly AudioStreamPlayer2D _audioStreamPlayerBangMedium = new();
	private readonly AudioStreamPlayer2D _audioStreamPlayerBangSmall = new();

	public override void _Ready()
	{
		_audioStreamPlayerBangLarge.Stream = _bangLarge;
		AddChild(_audioStreamPlayerBangLarge);
		_audioStreamPlayerBangMedium.Stream = _bangMedium;
		AddChild(_audioStreamPlayerBangMedium);
		_audioStreamPlayerBangSmall.Stream = _bangSmall;
		AddChild(_audioStreamPlayerBangSmall);

		if (GetParent() is Window)
		{
			CreateField(_testingNumAsteroids, new Rect2(0, 0, 0, 0), false);
		}
	}

	public override void _Process(double delta)
	{
		CleanUpFlaggedAsteroids();
	}

	// Clear active asteroid list and destroy all associated GameObjects
	public void DestroyField()
	{
		foreach (var asteroid in _activeAsteroids)
		{
			asteroid.Asteroid.QueueFree();
		}
		_activeAsteroids.Clear();
	}

	// Create a sheet of large asteroids with random position, velocity and angular velocity,
	// while avoiding the exclusionZone Rect
	public void CreateField(int numAsteroids, Rect2 exclusionZone, bool onlyLarge = true)
	{
		DestroyField();
		for (var i = 0; i < numAsteroids; i++)
		{
			if (onlyLarge)
			{
				CreateRandomAsteroid(AsteroidSize.Large, exclusionZone);
			}
			else
			{
				var values = Enum.GetValues(typeof(AsteroidSize));
				CreateRandomAsteroid((AsteroidSize)values.GetValue(GD.RandRange(0, values.Length - 1)), exclusionZone);
			}
		}
	}

	// Create an asteroid of the given size, with random position, velocity and angular velocity,
	// while avoiding the exclusionZone Rect.  Add the 
	private AsteroidDetails CreateRandomAsteroid(AsteroidSize size, Rect2 exclusionZone)
	{
		var vp = GetViewport().GetVisibleRect();

		var newAsteroidX = (float)GD.RandRange(vp.Position.X, vp.End.X - exclusionZone.Size.X);
		var newAsteroidY = (float)GD.RandRange(vp.Position.Y, vp.End.Y - exclusionZone.Size.Y);
		if (newAsteroidX > exclusionZone.Position.X)
		{
			newAsteroidX += exclusionZone.Size.X;
		}
		if (newAsteroidY > exclusionZone.Position.Y)
		{
			newAsteroidY += exclusionZone.Size.Y;
		}
		var newAsteroidPosition = new Vector2(newAsteroidX, newAsteroidY);

		// Pick random asteroid prefab of the required size
		var astroidScene = size switch
		{
			AsteroidSize.Large => _largeAsteroidPrefabs[GD.RandRange(0, _largeAsteroidPrefabs.Count - 1)],
			AsteroidSize.Medium => _mediumAsteroidPrefabs[GD.RandRange(0, _mediumAsteroidPrefabs.Count - 1)],
			AsteroidSize.Small => _smallAsteroidPrefabs[GD.RandRange(0, _smallAsteroidPrefabs.Count - 1)],
			_ => throw new NotImplementedException()
		};

		// Create the asteroid GameObject and set rotation, linear velocity, angular velocity and location
		var asteroid = astroidScene.Instantiate<Asteroid>();
		asteroid.Position = newAsteroidPosition;
		asteroid.LinearVelocity = Vector2.Right.Rotated((float)GD.RandRange(0f, 2f * Math.PI)) * (float)GD.RandRange(MinSpeed, MaxSpeed);
		asteroid.AngularVelocity = (float)(IsRotationEnabled ? GD.RandRange(MinRadiansPerSecond, MaxRadiansPerSecond) : 0f);

		// Using deferred mode as Gadot objects to setting the new Asteroid position otherwise
		// asteroid.Connect("Collided", new Callable(this, "CollidedWithAsteroid"), (int)ConnectFlags.Deferred);
		asteroid.Collided += CollidedWithAsteroid;

		// Return the created asteroid and associated information
		var asteroidDetails = new AsteroidDetails { Asteroid = asteroid, AsteroidSize = size, ReadyForCleanUp = false };
		_activeAsteroids.Add(asteroidDetails);
		//AddChild(asteroid);
		CallDeferred(MethodName.AddChild, asteroid);
		return asteroidDetails;
	}

	// Create a random asteroid at the position of the given asteroid
	// at one size smaller than the given asteroid
	private AsteroidDetails CreateSplitAsteroid(AsteroidDetails existingAsteroid)
	{
		var newAsteroidSize = existingAsteroid.AsteroidSize switch
		{
			AsteroidSize.Large => AsteroidSize.Medium,
			AsteroidSize.Medium => AsteroidSize.Small,
			_ => throw new NotImplementedException()
		};

		var asteroidDetails = CreateRandomAsteroid(newAsteroidSize, new Rect2(0, 0, 0, 0));
		asteroidDetails.Asteroid.Position = existingAsteroid.Asteroid.Position;

		return asteroidDetails;
	}

	// Split the given asteroid into two smaller asteroids if the asteroid is Large or Medium,
	// and play the associated explosion audio
	private void SplitAsteroid(AsteroidDetails asteroid)
	{
		switch (asteroid.AsteroidSize)
		{
			case AsteroidSize.Large:
				_audioStreamPlayerBangLarge.Play();
				break;
			case AsteroidSize.Medium:
				_audioStreamPlayerBangMedium.Play();
				break;
			case AsteroidSize.Small:
				_audioStreamPlayerBangSmall.Play();
				break;
		}

		if (asteroid.AsteroidSize != AsteroidSize.Small)
		{
			CreateSplitAsteroid(asteroid);
			CreateSplitAsteroid(asteroid);
		}
		else
		{
			var asteroidExplosion = _asteroidExplosion.Instantiate<AsteroidExplosion>();
			asteroidExplosion.Position = asteroid.Asteroid.Position;
			asteroidExplosion.AngularVelocity = asteroid.Asteroid.AngularVelocity;
			asteroidExplosion.LinearVelocity = asteroid.Asteroid.LinearVelocity;
			// AddChild(asteroidExplosion);
			CallDeferred(MethodName.AddChild, asteroidExplosion);
			asteroidExplosion.ExplosionCompleted += DestroyExplosion;
		}
	}

	private void DestroyExplosion(AsteroidExplosion asteroidExplosion)
	{
		RemoveChild(asteroidExplosion);
		asteroidExplosion.QueueFree();
	}

	private void CollidedWithAsteroid(Asteroid asteroid, Node2D collidedWith)
	{
		GD.Print($"Collision detected in field {collidedWith.GetParent().Name}");

		var asteroidDetails = _activeAsteroids.Find(ad => ad.Asteroid == asteroid);

		if (asteroidDetails != null)
		{
			if (!asteroidDetails.ReadyForCleanUp)
			{
				// Deactivate, flag for clean up and split into two smaller asteroids
				asteroidDetails.ReadyForCleanUp = true;
				SplitAsteroid(asteroidDetails);
				EmitSignal(SignalName.Collision, asteroid, (int)asteroidDetails.AsteroidSize, collidedWith.GetParent());
			}
		}
		else
		{
			// TODO ERROR
		}
	}

	// Delete flagged entries from _activeAsteroids and destroy the associated GameObject for each.
	// Asteroids are not deleted immediately on collision as more than one collision may occur in a
	// a single frame and the associated AsteroidDetails may be required to handle each collision.
	private void CleanUpFlaggedAsteroids()
	{
		// Remove all flagged entries from _activeAsteroids and Destroy their associated GameObjects 
		for (var asteroidIndex = _activeAsteroids.Count - 1; asteroidIndex >= 0; asteroidIndex--)
		{
			var asteroidDetails = _activeAsteroids[asteroidIndex];
			if (asteroidDetails.ReadyForCleanUp)
			{
				RemoveChild(asteroidDetails.Asteroid);
				asteroidDetails.Asteroid.QueueFree();
				_activeAsteroids.RemoveAt(asteroidIndex);

			}
		}

		// If all asteroids have been destroyed then raise the associated event
		if (_activeAsteroids.Count <= 0)
		{
			EmitSignal(SignalName.FieldCleared);
		}
	}
}
