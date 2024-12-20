using System.Collections.Generic;
using Godot;
namespace Asteroids;

public partial class Main : Node
{
    [ExportCategory("Scores")]
    [Export]
    private int _asteroidScoreLarge = 20;
    [Export]
    private int _asteroidScoreMedium = 50;
    [Export]
    private int _asteroidScoreSmall = 100;
    [Export]
    private int _saucerScoreLarge = 200;
    [Export]
    private int _saucerScoreLargeSmall = 1000;

    [ExportCategory("Lives")]
    [Export]
    private int _startLives = 3;
    [Export]
    private int _maxLives = 6;
    [Export]
    private int _extraLifeThreshold = 1000;

    [ExportCategory("Asteroids")]
    [ExportGroup("Start Count")]
    [Export]
    private int _startAsteroidInitialCount = 4;
    [Export]
    private int _startAsteroidCountDelta = 1;
    [Export]
    private int _startAsteroidCountMax = 18;
    [Export]
    [ExportGroup("Speed")]
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

    [ExportCategory("Sound")]
    [Export]
    public bool _isSoundEnabled = true;

    [ExportCategory("Misc")]
    [Export]
    private int _safeZoneRadius = 200;

    [ExportCategory("Testing")]
    [Export]
    private bool _infiniteLives = false;

    private readonly Dictionary<AsteroidSize, int> _asteroidScores = new();
    private AsteroidField _asteroidField;
    private BulletController _bulletController;
    private Player _player;
    private Score _score;
    private Lives _lives;
    private Label _gameOverLabel;
    private Area2D _exclusionZone;
    private CollisionShape2D _exclusionZoneCollisionShape;
    private int _startAsteroidsCount;
    private int _nextExtraLifeThreshold;
    private bool _gameOver = false;
    private bool _spawnNewPlayer = false;
    private Vector2 _shipSpawnPosition;

    public override void _EnterTree()
    {
        _asteroidScores[AsteroidSize.Large] = _asteroidScoreLarge;
        _asteroidScores[AsteroidSize.Medium] = _asteroidScoreMedium;
        _asteroidScores[AsteroidSize.Small] = _asteroidScoreSmall;
    }

    public override void _Ready()
    {
        // Get references too all required scenes
        _player = GetNode<Player>("Player");
        _bulletController = GetNode<BulletController>("BulletController");
        _asteroidField = GetNode<AsteroidField>("AsteroidField");
        _score = GetNode<Score>("Ui/Score");
        _lives = GetNode<Lives>("Ui/Lives");
        _gameOverLabel = GetNode<Label>("Ui/Game Over");
        _exclusionZone = GetNode<Area2D>("ExclusionZone");
        _exclusionZoneCollisionShape = _exclusionZone.GetNode<CollisionShape2D>("CollisionShape2D");

        // Missiles
        _bulletController.Collided += OnBulletSignalCollided;

        // Player ship
        _player.Exploding += OnPlayerSignalExploding;
        _player.Exploded += OnPlayerSignalExploded;
        _player.Shoot += OnPlayerSignalShoot;
        _shipSpawnPosition = Screen.Instance.GetCentre();

        // Asteroids
        _startAsteroidsCount = _startAsteroidInitialCount;
        _asteroidField.MinSpeed = MinSpeed;
        _asteroidField.MaxSpeed = MaxSpeed;
        _asteroidField.IsRotationEnabled = IsRotationEnabled;
        _asteroidField.MinRadiansPerSecond = MinRadiansPerSecond;
        _asteroidField.MaxRadiansPerSecond = MaxRadiansPerSecond;
        _asteroidField.Collision += OnAsteroidSignalCollided;
        _asteroidField.FieldCleared += OnFieldCleared;

        // Asteroid exclusion zone - ensures ship spawns are safe
        var circleShape = (CircleShape2D)_exclusionZoneCollisionShape.Shape;
        circleShape.Radius = _safeZoneRadius;
        _exclusionZone.Position = _shipSpawnPosition;

        // Sound
        if (!_isSoundEnabled)
        {
            AudioServer.SetBusMute(AudioServer.GetBusIndex("Master"), true);
        }

        // Start a new game
        NewGame();
    }

    private void NewGame()
    {
        _lives.SetLives(_startLives);
        _nextExtraLifeThreshold = _extraLifeThreshold;
        _exclusionZoneCollisionShape.Disabled = true;
        _spawnNewPlayer = true;
        _asteroidField.CreateField(_startAsteroidsCount,
           new Rect2(_shipSpawnPosition.X - _safeZoneRadius, _shipSpawnPosition.Y - _safeZoneRadius,
                     _safeZoneRadius * 2, _safeZoneRadius * 2),
           true);
    }

    public override void _Process(double delta)
    {
        if (_spawnNewPlayer)
        {
            if (!_exclusionZone.HasOverlappingAreas())
            {
                _spawnNewPlayer = false;
                _exclusionZoneCollisionShape.Disabled = true;
                _player.Activate(_shipSpawnPosition);
            }
        }
    }

    private void OnFieldCleared()
    {
        _startAsteroidsCount += _startAsteroidCountDelta;
        _startAsteroidsCount = Mathf.Clamp(_startAsteroidsCount, 0, _startAsteroidCountMax);
        _asteroidField.CreateField(_startAsteroidsCount,
            new Rect2(_player.Position.X - _safeZoneRadius, _player.Position.Y - _safeZoneRadius,
                      _safeZoneRadius * 2, _safeZoneRadius * 2),
                      true);
    }

    private void OnPlayerSignalExploded()
    {
        GD.Print("In OnExploded in Main");
        _player.Deactivate();
        if (!_gameOver)
        {
            // This is safe wrt signal delivery order
            // as there is a gap between "Exploding" and "Exploded"
            _spawnNewPlayer = true;
        }
    }

    private void OnPlayerSignalExploding()
    {
        if (_lives.Value > 0)
        {
            if (!_infiniteLives)
            {
                _lives.RemoveLife();
            }

            if (_lives.Value == 0)
            {
                GameOver();
            }
            else
            {
                _exclusionZoneCollisionShape.SetDeferred(CollisionShape2D.PropertyName.Disabled, false);
            }
        }
    }

    private void GameOver()
    {
        GD.Print("GAME OVER");
        _gameOver = true;
        _gameOverLabel.Show();
    }

    private void OnPlayerSignalShoot(Vector2 position, Vector2 shipLinearVelocity, float shipRotation)
    {
        GD.Print($"Player collision detected in '{this.Name}'");
        _bulletController.SpawnBullet(position, shipLinearVelocity, shipRotation);
    }

    private void OnBulletSignalCollided(Bullet bullet, Node collidedWith)
    {
        GD.Print($"Bullet collision detected in '{this.Name}' with '{collidedWith.Name}'");
        _bulletController.KillBullet(bullet);
    }

    private void IncreaseScore(int increase)
    {
        _score.Increase(increase);
        if (_score.Value > _nextExtraLifeThreshold && _lives.Value < _maxLives)
        {
            _lives.AddLife();
            _nextExtraLifeThreshold += _extraLifeThreshold;
        }
    }

    private void OnAsteroidSignalCollided(Asteroid asteroid, AsteroidSize size, Node collidedWith)
    {
        GD.Print($"Asteroid collision detected in '{this.Name}' with '{collidedWith.Name}'");
        if (collidedWith is Bullet)
        {
            IncreaseScore(_asteroidScores[size]);
        }
    }
}
