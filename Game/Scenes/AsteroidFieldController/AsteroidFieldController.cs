using System;
using System.Collections.Generic;
using Godot;

namespace Asteroids;

public partial class AsteroidFieldController : Node
{
    private class AsteroidDetails
    {
        // The asteroid
        public Asteroid Asteroid { get; set; }

        // Size of the asteroid
        public AsteroidSize AsteroidSize { get; set; }

        // Flag the asteroid for destruction on the next cycle
        public bool ReadyForCleanUp { get; set; } = false;
    }

    // Signals
    [Signal]
    public delegate void CollidedEventHandler(Asteroid asteroid, AsteroidSize size, Node collidedWith);

    [Signal]
    public delegate void FieldClearedEventHandler(AsteroidFieldController sender);

    // Values configurable via the inspector
    [Export]
    [ExportCategory("References")]
    private PackedScene _asteroidExplosion;

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
    public float RotationMaxRadiansPerSecond { get; set; } = 1.0f;

    [ExportGroup("Gravity")]
    [Export]
    public float GravitationalMultiplier { get; set; } = 1000;
    [Export]
    private float _massLargeAsteroid = 5000;
    [Export]
    private float _massMediumAsteroid = 500;
    [Export]
    private float _massSmallAsteroid = 50;

    [ExportGroup("Sounds")]
    [Export]
    private AudioStream _bangLarge;
    [Export]
    private AudioStream _bangMedium;
    [Export]
    private AudioStream _bangSmall;


    [ExportCategory("Testing")]
    [Export]
    private int _testingNumAsteroids = 10;

    public int AsteroidCount { get => _activeAsteroids.Count; }

    // Asteroid scenes
    private readonly List<PackedScene> _largeAsteroidPrefabs = new() {
        Resources.AsteroidType1Large,
        Resources.AsteroidType1Large,
        Resources.AsteroidType1Large
    };

    private readonly List<PackedScene> _mediumAsteroidPrefabs = new() {
        Resources.AsteroidType1Medium,
        Resources.AsteroidType1Medium,
        Resources.AsteroidType1Medium
    };

    private readonly List<PackedScene> _smallAsteroidPrefabs = new() {
        Resources.AsteroidType1Small,
        Resources.AsteroidType1Small,
        Resources.AsteroidType1Small
    };

    // List of all active asteroids
    private readonly List<AsteroidDetails> _activeAsteroids = new();

    private readonly AudioStreamPlayer2D _audioStreamPlayerBangLarge = new();
    private readonly AudioStreamPlayer2D _audioStreamPlayerBangMedium = new();
    private readonly AudioStreamPlayer2D _audioStreamPlayerBangSmall = new();

    // Asteroid id used to give spawned asteroids friendly names
    private int _asteroidId = 1;

    private bool _fxEnabled = true;

    public override void _Ready()
    {
        // Set up audio streams
        _audioStreamPlayerBangLarge.Bus = Resources.AUDIO_BUS_NAME_FX;
        _audioStreamPlayerBangLarge.Stream = _bangLarge ?? throw new NullReferenceException("Bang Large not set");
        AddChild(_audioStreamPlayerBangLarge);
        _audioStreamPlayerBangMedium.Bus = Resources.AUDIO_BUS_NAME_FX;
        _audioStreamPlayerBangMedium.Stream = _bangMedium ?? throw new NullReferenceException("Bang Medium not set");
        AddChild(_audioStreamPlayerBangMedium);
        _audioStreamPlayerBangSmall.Bus = Resources.AUDIO_BUS_NAME_FX;
        _audioStreamPlayerBangSmall.Stream = _bangSmall ?? throw new NullReferenceException("Bang Small not set");
        AddChild(_audioStreamPlayerBangSmall);

        // Test mode
        if (GetParent() is Window)
        {
            SpawnField(_testingNumAsteroids, new Rect2(), false);
        }
    }

    public Vector2 GetGravitationalVector(Vector2 location)
    {
        var result = new Vector2();

        foreach (var asteroid in _activeAsteroids)
        {
            if (!asteroid.ReadyForCleanUp)
            {
                var locus = asteroid.Asteroid.Position - location;
                var unitForce = locus.Normalized() / locus.LengthSquared();
                result += unitForce * asteroid.AsteroidSize switch
                {
                    AsteroidSize.Large => _massLargeAsteroid,
                    AsteroidSize.Medium => _massMediumAsteroid,
                    AsteroidSize.Small => _massSmallAsteroid,
                    _ => throw new NotImplementedException()
                };
            }
        }
        return result * GravitationalMultiplier;
    }


    public void EnableFx(bool enable)
    {
        _fxEnabled = enable;
    }

    public override void _Process(double delta)
    {
        CleanUpFlaggedAsteroids();
    }

    // Clear active asteroid list and destroy all associated GameObjects
    public void DeSpawnField()
    {
        foreach (var asteroid in _activeAsteroids)
        {
            asteroid.ReadyForCleanUp = true;
        }
    }

    // Create a sheet of large asteroids with random position, velocity and angular velocity,
    // while avoiding the exclusionZone Rect
    public void SpawnField(int numAsteroids, Rect2 exclusionZone, bool onlyLarge = true)
    {
        _asteroidId = 1;
        DeSpawnField();

        for (var i = 0; i < numAsteroids; i++)
        {
            if (onlyLarge)
            {
                SpawnRandomAsteroid(AsteroidSize.Large, exclusionZone);
            }
            else
            {
                var values = Enum.GetValues(typeof(AsteroidSize));
                SpawnRandomAsteroid((AsteroidSize)values.GetValue(GD.RandRange(0, values.Length - 1)), exclusionZone);
            }
        }
    }

    // Create an asteroid of the given size, with random position, velocity and angular velocity,
    // while avoiding the exclusionZone Rect.   
    private AsteroidDetails SpawnRandomAsteroid(AsteroidSize size, Rect2 exclusionZone)
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
        asteroid.AngularVelocity = IsRotationEnabled ? (float)GD.RandRange(0, RotationMaxRadiansPerSecond * 2.0f) - RotationMaxRadiansPerSecond : 0f;

        asteroid.Collided += AsteroidOnCollided;

        // Return the created asteroid and associated information
        var asteroidDetails = new AsteroidDetails { Asteroid = asteroid, AsteroidSize = size, ReadyForCleanUp = false };
        _activeAsteroids.Add(asteroidDetails);

        CallDeferred("AddChildAndName", asteroid);
        return asteroidDetails;
    }

    private void AddChildAndName(Asteroid asteroid)
    {
        AddChild(asteroid);
        asteroid.Name = $"Asteroid #{_asteroidId}";

        _asteroidId++;
    }

    // Create a random asteroid at the position of the given asteroid
    // at one size smaller than the given asteroid
    private AsteroidDetails SpawnSplitAsteroid(AsteroidDetails existingAsteroid)
    {
        var newAsteroidSize = existingAsteroid.AsteroidSize switch
        {
            AsteroidSize.Large => AsteroidSize.Medium,
            AsteroidSize.Medium => AsteroidSize.Small,
            _ => throw new NotImplementedException()
        };

        var asteroidDetails = SpawnRandomAsteroid(newAsteroidSize, new Rect2(0, 0, 0, 0));
        asteroidDetails.Asteroid.Position = existingAsteroid.Asteroid.Position;

        return asteroidDetails;
    }

    // Split the given asteroid into two smaller asteroids if the asteroid is Large or Medium,
    // and play the associated explosion audio
    private void SplitAsteroid(AsteroidDetails asteroid)
    {
        if (_fxEnabled)
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
        }

        if (asteroid.AsteroidSize != AsteroidSize.Small)
        {
            SpawnSplitAsteroid(asteroid);
            SpawnSplitAsteroid(asteroid);
        }
        else
        {
            var asteroidExplosion = _asteroidExplosion.Instantiate<Explosion>();
            asteroidExplosion.Name = "Asteroid Explosion";
            asteroidExplosion.Position = asteroid.Asteroid.Position;
            asteroidExplosion.AngularVelocity = asteroid.Asteroid.AngularVelocity;
            asteroidExplosion.LinearVelocity = asteroid.Asteroid.LinearVelocity;
            CallDeferred(MethodName.AddChild, asteroidExplosion);
            asteroidExplosion.ExplosionCompleted += ExplosionOnExplosionCompleted;
        }
    }

    private void ExplosionOnExplosionCompleted(Explosion asteroidExplosion)
    {
        Logger.I.SignalReceived(this, asteroidExplosion, Explosion.SignalName.ExplosionCompleted);

        RemoveChild(asteroidExplosion);
        asteroidExplosion.QueueFree();
    }

    private void AsteroidOnCollided(Asteroid asteroid, Node2D collidedWith)
    {
        Logger.I.SignalReceived(this, asteroid, Asteroid.SignalName.Collided, collidedWith);

        // This could either be a bullet or an asteroid that's been collided with
        var asteroidDetails = _activeAsteroids.Find(ad => ad.Asteroid == asteroid);

        if (asteroidDetails != null)
        {
            if (!asteroidDetails.ReadyForCleanUp)
            {
                // Deactivate, flag for clean up and split into two smaller asteroids
                asteroidDetails.ReadyForCleanUp = true;

                SplitAsteroid(asteroidDetails);
                Logger.I.SignalSent(this, SignalName.Collided, asteroidDetails.AsteroidSize, collidedWith);
                EmitSignal(SignalName.Collided, asteroid, (int)asteroidDetails.AsteroidSize, collidedWith);
            }
        }
        else
        {
            throw new InvalidOperationException($"Can't find asteroid details for asteroid with name='{asteroid.Name}'");
        }
    }

    // Delete flagged entries from _activeAsteroids and destroy the associated GameObject for each.
    // Asteroids are not deleted immediately on collision as more than one collision may occur in a
    // a single frame and the associated AsteroidDetails may be required to handle each collision.
    private void CleanUpFlaggedAsteroids()
    {
        // Remove all flagged entries from _activeAsteroids and Destroy their associated GameObjects 
        var asteroidCount = _activeAsteroids.Count;
        for (var asteroidIndex = asteroidCount - 1; asteroidIndex >= 0; asteroidIndex--)
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
            Logger.I.SignalSent(this, SignalName.FieldCleared);
            EmitSignal(SignalName.FieldCleared, this);
        }
    }
}
