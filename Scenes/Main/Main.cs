using System.Collections.Generic;
using Godot;

namespace Asteroids;

public partial class Main : Node
{
    // Inspector configuration values
    [ExportCategory("Scores")]
    [Export]
    private int _scoreAsteroidLarge = 20;
    [Export]
    private int _scoreAsteroidMedium = 50;
    [Export]
    private int _scoreAsteroidSmall = 100;
    [Export]
    private int _scoreSaucerLarge = 200;
    [Export]
    private int _scoreSaucerSmall = 1000;

    [ExportCategory("Lives")]
    [Export]
    private int _livesNewGame = 3;
    [Export]
    private int _livesMax = 6;
    [Export]
    private int _lifeExtraThreshold = 1000;

    [ExportCategory("Asteroids")]
    [Export]
    private int _asteroidsNewGameStart = 4;
    [Export]
    private int _asteroidsNewGameMax = 18;
    [Export]
    private int _asteroidsNewSheetDelta = 1;

    // [ExportCategory("Sound")]
    // [Export]
    // private bool _soundIsEnabled = true;

    [ExportCategory("Misc")]
    [Export]
    private int _safeZoneRadius = 200;

    [ExportCategory("Testing")]
    [Export]
    private bool _infiniteLives = false;

    // Score table
    private readonly Dictionary<AsteroidSize, int> _asteroidScores = new();

    // Scene references
    private AsteroidFieldController _asteroidFieldController;
    private MissileController _missileController;
    private Player _player;
    private Score _score;
    private Lives _lives;
    private Label _gameOverLabel;
    private Label _oneCoinLabel;
    private PushStart _pushStartLabel;
    private Area2D _exclusionZone;
    private Beats _beats;
    private CollisionShape2D _exclusionZoneCollisionShape;
    private SettingsDialog _settingsDialog;
    private HelpDialog _helpDialog;
    private Panel _background;

    // State
    private int _asteroidsCurrentNewGameStart;
    private int _extraLifeThresholdNext;
    private Vector2 _shipSpawnPosition;


    // Game state machine
    private enum GameState { WaitingToPlay, AwaitingNewShip, Playing, ShowingConfigDialog, ShowingHelpDialog };
    private GameState _gameState;

    public override void _EnterTree()
    {
        // Set up score table
        _asteroidScores[AsteroidSize.Large] = _scoreAsteroidLarge;
        _asteroidScores[AsteroidSize.Medium] = _scoreAsteroidMedium;
        _asteroidScores[AsteroidSize.Small] = _scoreAsteroidSmall;
    }

    public override void _Ready()
    {
        // Scene references
        SetupSceneReferences();

        // Scenes
        SetupSceneSignals();

        // New ship spawn location
        _shipSpawnPosition = Screen.Instance.Centre;

        // Asteroid exclusion zone - ensures ship spawns are safe
        var circleShape = (CircleShape2D)_exclusionZoneCollisionShape.Shape;
        circleShape.Radius = _safeZoneRadius;
        _exclusionZone.Position = _shipSpawnPosition;

        // Apply the default configuration
        ApplyConfiguration(_settingsDialog.ActiveSettings);

        // Waiting to start
        WaitingToPlay();
    }

    private void SetupSceneReferences()
    {
        // Get references too all required scenes
        _player = GetNode<Player>("Player");
        _missileController = GetNode<MissileController>("MissileController");
        _asteroidFieldController = GetNode<AsteroidFieldController>("AsteroidFieldController");
        _score = (Score)FindChild("Score");
        _lives = (Lives)FindChild("Lives");
        _gameOverLabel = (Label)FindChild("Game Over");
        _pushStartLabel = (PushStart)FindChild("Push Start");
        _oneCoinLabel = (Label)FindChild("Help");
        _exclusionZone = GetNode<Area2D>("ExclusionZone");
        _exclusionZoneCollisionShape = _exclusionZone.GetNode<CollisionShape2D>("CollisionShape2D");
        _beats = GetNode<Beats>("Beats");
        _settingsDialog = GetNode<SettingsDialog>("Settings Dialog");
        _helpDialog = GetNode<HelpDialog>("Help Dialog");
        _background = GetNode<Panel>("Background");
    }

    private void SetupSceneSignals()
    {
        // Missiles
        _missileController.Collided += OnMissileCollided;

        // Player ship
        _player.Exploding += OnPlayerExploding;
        _player.Exploded += OnPlayerExploded;
        _player.Shoot += OnPlayerShoot;

        // Asteroids
        _asteroidFieldController.Collision += OnAsteroidCollided;
        _asteroidFieldController.FieldCleared += OnFieldCleared;

        // Configuration settings
        _settingsDialog.OkPressed += OnConfigDialogOkPressed;
        _settingsDialog.Cancel += OnConfigDialogCancelPressed;

        // Controls
        _helpDialog.OkPressed += OnHelpDialogOkPressed;
        // Window resize
        GetTree().GetRoot().SizeChanged += OnResized;
    }

    public override void _Process(double delta)
    {
        if (_gameState == GameState.AwaitingNewShip)
        {
            if (!_exclusionZone.HasOverlappingAreas())
            {
                _gameState = GameState.Playing;
                _exclusionZoneCollisionShape.Disabled = true;
                _player.Activate(_shipSpawnPosition);
            }
        }
    }

    private void OnFieldCleared()
    {
        _asteroidsCurrentNewGameStart += _asteroidsNewSheetDelta;
        _asteroidsCurrentNewGameStart = Mathf.Clamp(_asteroidsCurrentNewGameStart, 0, _asteroidsNewGameMax);
        _asteroidFieldController.CreateField(_asteroidsCurrentNewGameStart,
            new Rect2(_player.Position.X - _safeZoneRadius, _player.Position.Y - _safeZoneRadius,
                      _safeZoneRadius * 2, _safeZoneRadius * 2),
                      true);
        _beats.Reset();
    }
    private void OnPlayerExploded()
    {
        GD.Print("In OnExploded in Main");
        _player.Deactivate();

        if (_gameState == GameState.Playing)
        {
            // This is safe wrt signal delivery order
            // as there is a gap between "Exploding" and "Exploded"
            _gameState = GameState.AwaitingNewShip;
            _beats.Start();
        }
    }

    private void OnPlayerExploding()
    {
        if (_lives.Value > 0)
        {
            _beats.Stop();
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

    private void OnPlayerShoot(Vector2 position, Vector2 shipLinearVelocity, float shipRotation)
    {
        _missileController.SpawnMissile(position, shipLinearVelocity, shipRotation);
    }

    private void OnMissileCollided(Missile missile, Node collidedWith)
    {
        _missileController.KillMissile(missile);
    }

    private void OnAsteroidCollided(Asteroid asteroid, AsteroidSize size, Node collidedWith)
    {
        if (collidedWith is Missile)
        {
            IncreaseScore(_asteroidScores[size]);
        }
    }

    private GameSettings CollectConfiguration()
    {
        return new GameSettings
        {
            BackgroundEnabled = _background.Visible,
            SoundEnabled = GetSoundEnabled(),

            ShipInfiniteLives = _infiniteLives,
            ShipStartingCount = _livesNewGame,
            ShipMax = _livesMax,
            ShipExtraThreshold = _lifeExtraThreshold,
            ShipAcceleration = _player.ThrustForce,
            ShipRotationSpeed = _player.RotationSpeed,

            AsteroidsRotationEnabled = _asteroidFieldController.IsRotationEnabled,
            AsteroidsStartingQuantity = _asteroidsNewGameStart,
            AsteroidsMaxStartingQuantity = _asteroidsNewGameMax,
            AsteroidsMinSpeed = _asteroidFieldController.MinSpeed,
            AsteroidsMaxSpeed = _asteroidFieldController.MaxSpeed,

            MissilesMax = _missileController.MissileCount,
            MissilesLifespan = _missileController.MissileDuration
        };
    }

    private void ShowConfigDialog()
    {
        _pushStartLabel.Hide();
        _oneCoinLabel.Hide();

        _settingsDialog.Show();

        _gameState = GameState.ShowingConfigDialog;
    }

    private void ApplyConfiguration(GameSettings config)
    {
        _background.Visible = config.BackgroundEnabled;

        SetSoundEnabled(config.SoundEnabled);

        _infiniteLives = config.ShipInfiniteLives;
        _livesNewGame = config.ShipStartingCount;
        _livesMax = config.ShipMax;
        _lifeExtraThreshold = config.ShipExtraThreshold;
        _player.ThrustForce = config.ShipAcceleration;
        _player.RotationSpeed = config.ShipRotationSpeed;

        _asteroidFieldController.IsRotationEnabled = config.AsteroidsRotationEnabled;
        _asteroidsNewGameStart = config.AsteroidsStartingQuantity;
        _asteroidsNewGameMax = config.AsteroidsMaxStartingQuantity;
        _asteroidFieldController.MinSpeed = config.AsteroidsMinSpeed;
        _asteroidFieldController.MaxSpeed = config.AsteroidsMaxSpeed;

        _missileController.MissileCount = config.MissilesMax;
        _missileController.MissileSpeed = config.MissilesSpeed;
        _missileController.MissileDuration = config.MissilesLifespan;
    }

    private void OnConfigDialogOkPressed()
    {
        _pushStartLabel.Show();
        _oneCoinLabel.Show();

        var config = _settingsDialog.ActiveSettings;
        ApplyConfiguration(config);

        _gameState = GameState.WaitingToPlay;
    }

    private void OnConfigDialogCancelPressed()
    {
        _pushStartLabel.Show();
        _oneCoinLabel.Show();
        _gameState = GameState.WaitingToPlay;
    }

    private void WaitingToPlay()
    {
        _asteroidFieldController.CreateField(_asteroidsNewGameStart, new Rect2(), false);
        _pushStartLabel.Show();
        _oneCoinLabel.Show();

        _gameState = GameState.WaitingToPlay;
    }

    public override void _Input(InputEvent inputEvent)
    {
        if (inputEvent.IsActionPressed("Quit"))
        {
            GetTree().Quit();
        }

        if (_gameState == GameState.WaitingToPlay)
        {
            if (inputEvent.IsActionPressed("Start"))
            {
                NewGame();
            }
            else if (inputEvent.IsActionPressed("Config"))
            {
                ShowConfigDialog();
            }
            else if (inputEvent.IsActionPressed("Help"))
            {
                ShowHelpDialog();
            }
        }
    }

    private void ShowHelpDialog()
    {
        _oneCoinLabel.Hide();
        _pushStartLabel.Hide();

        _helpDialog.Show();
        _gameState = GameState.ShowingHelpDialog;
    }

    private void OnHelpDialogOkPressed()
    {
        _oneCoinLabel.Show();
        _pushStartLabel.Show();

        _gameState = GameState.WaitingToPlay;
    }

    private void NewGame()
    {
        _asteroidsCurrentNewGameStart = _asteroidsNewGameStart;
        _lives.SetLives(_livesNewGame);
        _score.Value = 0;
        _extraLifeThresholdNext = _lifeExtraThreshold;
        _exclusionZoneCollisionShape.Disabled = true;
        _pushStartLabel.Hide();
        _gameOverLabel.Hide();
        _oneCoinLabel.Hide();
        _asteroidFieldController.CreateField(_asteroidsCurrentNewGameStart,
           new Rect2(_shipSpawnPosition.X - _safeZoneRadius, _shipSpawnPosition.Y - _safeZoneRadius,
                     _safeZoneRadius * 2, _safeZoneRadius * 2),
           true);
        _beats.Reset();
        _beats.Start();

        _gameState = GameState.AwaitingNewShip;
    }

    private void GameOver()
    {
        _beats.Stop();
        _gameOverLabel.Show();

        WaitingToPlay();
    }

    private void OnResized()
    {
        _shipSpawnPosition = Screen.Instance.Centre;
        _exclusionZone.Position = _shipSpawnPosition;
    }

    private void SetSoundEnabled(bool enabled)
    {
        AudioServer.SetBusMute(AudioServer.GetBusIndex("Master"), !enabled);
    }

    private bool GetSoundEnabled()
    {
        return !AudioServer.IsBusMute(AudioServer.GetBusIndex("Master"));
    }

    private void IncreaseScore(int increase)
    {
        _score.Increase(increase);
        if (_score.Value > _extraLifeThresholdNext && _lives.Value < _livesMax)
        {
            _lives.AddLife();
            _extraLifeThresholdNext += _lifeExtraThreshold;
        }
    }
}
