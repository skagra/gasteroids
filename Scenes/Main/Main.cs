using System.Collections.Generic;
using Godot;

namespace Asteroids;

public partial class Main : Node
{
    #region Inspector configuration values

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

    [ExportCategory("Sound")]
    [Export]
    private bool _soundIsEnabled = true;

    [ExportCategory("Misc")]
    [Export]
    private int _safeZoneRadius = 200;

    [ExportCategory("Testing")]
    [Export]
    private bool _infiniteLives = false;

    #endregion Inspector configuration values

    // Score table
    private readonly Dictionary<AsteroidSize, int> _asteroidScores = new();

    #region Scene references

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
    private Config _config;
    private ControlsDialog _controlsDialog;
    private Panel _background;

    #endregion Scene references

    #region State

    private int _asteroidsCurrentNewGameStart;
    private int _extraLifeThresholdNext;
    private Vector2 _shipSpawnPosition;

    #endregion State

    #region Game state machine

    // Game state machine
    private enum GameState { WaitingToPlay, AwaitingNewShip, Playing, /* GameOver */ ShowingConfig, ShowingControls };
    private GameState _gameState;

    #endregion Game state machine

    #region Set up

    public override void _EnterTree()
    {
        // Set up score table
        _asteroidScores[AsteroidSize.Large] = _scoreAsteroidLarge;
        _asteroidScores[AsteroidSize.Medium] = _scoreAsteroidMedium;
        _asteroidScores[AsteroidSize.Small] = _scoreAsteroidSmall;
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
        _config = GetNode<Config>("Config");
        _controlsDialog = GetNode<ControlsDialog>("Controls Dialog");
        _background = GetNode<Panel>("Background");
    }

    private void SetupSceneSignals()
    {
        // Missiles
        _missileController.Collided += OnMissileSignalCollided;

        // Player ship
        _player.Exploding += OnPlayerSignalExploding;
        _player.Exploded += OnPlayerSignalExploded;
        _player.Shoot += OnPlayerSignalShoot;

        // Asteroids
        _asteroidFieldController.Collision += OnAsteroidSignalCollided;
        _asteroidFieldController.FieldCleared += OnFieldCleared;

        // Configuration settings
        _config.OkPressed += OnConfigOkPressed;
        _config.Cancel += OnCancelPressed;

        // Controls
        _controlsDialog.OkPressed += OnControlsOkPressed;
        // Window resize
        GetTree().GetRoot().SizeChanged += OnResized;
    }

    public override void _Ready()
    {
        SetupSceneReferences();
        GD.Print($"In main READY missiles count = {_missileController.MissileCount}");
        SetupSceneSignals();

        // New ship spawn location
        _shipSpawnPosition = Screen.Instance.GetCentre();

        // Asteroid exclusion zone - ensures ship spawns are safe
        var circleShape = (CircleShape2D)_exclusionZoneCollisionShape.Shape;
        circleShape.Radius = _safeZoneRadius;
        _exclusionZone.Position = _shipSpawnPosition;

        ApplyConfiguration(_config.Settings);

        // // Sound
        // SetSoundEnabled(_soundIsEnabled);

        // Waiting to start
        WaitingToPlay();
    }

    #endregion Set up

    #region Game execution

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
    private void OnPlayerSignalExploded()
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

    private void OnPlayerSignalExploding()
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

    private void OnPlayerSignalShoot(Vector2 position, Vector2 shipLinearVelocity, float shipRotation)
    {
        GD.Print($"Player collision detected in '{this.Name}'");
        _missileController.SpawnMissile(position, shipLinearVelocity, shipRotation);
    }

    private void OnMissileSignalCollided(Missile missile, Node collidedWith)
    {
        GD.Print($"Missile collision detected in '{this.Name}' with '{collidedWith.Name}'");
        _missileController.KillMissile(missile);
    }

    private void OnAsteroidSignalCollided(Asteroid asteroid, AsteroidSize size, Node collidedWith)
    {
        GD.Print($"Asteroid collision detected in '{this.Name}' with '{collidedWith.Name}'");
        if (collidedWith is Missile)
        {
            IncreaseScore(_asteroidScores[size]);
        }
    }

    #endregion Game execution

    #region Configuration

    private bool _backgroundEnabled = true; // TODO

    private Config.ConfigSettings CollectConfiguration()
    {
        return new Config.ConfigSettings
        {
            BackgroundEnabled = _backgroundEnabled,
            SoundEnabled = _soundIsEnabled,

            ShipInvulnerable = _infiniteLives,
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

    private void ShowConfig()
    {
        _pushStartLabel.Hide();
        _oneCoinLabel.Hide();

        _config.Show(CollectConfiguration());

        GD.Print($"In main missiles count = {_missileController.MissileCount}");

        _gameState = GameState.ShowingConfig;
    }

    private void ApplyConfiguration(Config.ConfigSettings config)
    {
        _backgroundEnabled = config.BackgroundEnabled;
        _background.Visible = _backgroundEnabled;

        SetSoundEnabled(config.SoundEnabled);  // TODO

        _infiniteLives = config.ShipInvulnerable;
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
        _missileController.MissileDuration = config.MissilesLifespan;
    }

    private void OnConfigOkPressed()
    {
        GD.Print("Showing push to start");

        _pushStartLabel.Show();
        _oneCoinLabel.Show();

        var config = _config.Settings;
        ApplyConfiguration(config);

        _gameState = GameState.WaitingToPlay;

        GD.Print($"Main OK Pressed has AsteroidsRotationEnabled={config.AsteroidsRotationEnabled} _asteroidFieldController.IsRotationEnabled={_asteroidFieldController.IsRotationEnabled}");
    }

    private void OnCancelPressed()
    {
        _pushStartLabel.Show();
        _oneCoinLabel.Show();
        _gameState = GameState.WaitingToPlay;
    }

    #endregion Configuration

    #region Waiting to play

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
                ShowConfig();
            }
            else if (inputEvent.IsActionPressed("Help"))
            {
                ShowControls();
            }
        }
    }

    private void ShowControls()
    {
        _oneCoinLabel.Hide();
        _pushStartLabel.Hide();

        _controlsDialog.Show();
        _gameState = GameState.ShowingControls;
    }

    private void OnControlsOkPressed()
    {
        _oneCoinLabel.Show();
        _pushStartLabel.Show();

        _gameState = GameState.WaitingToPlay;
    }

    #endregion Waiting to play

    #region New game

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
        SetSoundEnabled(_soundIsEnabled);
        _beats.Reset();
        _beats.Start();

        _gameState = GameState.AwaitingNewShip;
    }

    #endregion New game

    #region Game over

    private void GameOver()
    {
        GD.Print("Game Over");

        //_gameState = GameState.GameOver;
        _beats.Stop();
        _gameOverLabel.Show();

        WaitingToPlay();
    }

    #endregion Game over

    private void OnResized()
    {
        _shipSpawnPosition = Screen.Instance.GetCentre();
        _exclusionZone.Position = _shipSpawnPosition;
    }

    #region Utility methods
    private void SetSoundEnabled(bool enabled)
    {
        GD.Print($"Setting sound enabled = {enabled}");
        _soundIsEnabled = enabled;
        AudioServer.SetBusMute(AudioServer.GetBusIndex("Master"), !enabled);
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

    #endregion Utility methods
}
