using System.Collections.Generic;
using Godot;

namespace Asteroids;

public partial class Main : Node
{
    private const string _SETTINGS_SAVE_PATH = "user://settings.json";
    private const string _HIGH_SCORE_SAVE_PATH = "user://highscores.json";

    private const float _GAME_OVER_SHOW_TIME = 5;
    private const float _HIGH_SCORE_SHOW_TIME = 10;

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

    [ExportCategory("Saucers")]
    [Export]
    private float _largeSaucerStartMinSpawnTimer = 10;
    [Export]
    private float _largeSaucerStartMaxSpawnTimer = 20;
    [Export]
    private float _largeSaucerSpawnTimerFloor = 5;
    [Export]
    private float _largeSaucerSpawnTimerDeltaProportion = 0.5f;
    [Export]
    private float _smallSaucerStartMinSpawnTimer = 60;
    [Export]
    private float _smallSaucerStartMaxSpawnTimer = 120;
    [Export]
    private float _smallSaucerSpawnTimerFloor = 5;
    [Export]
    private float _smallSaucerSpawnTimerDeltaProportion = 0.5f;

    [ExportCategory("Misc")]
    [Export]
    private int _safeZoneRadius = 200;
    [Export]
    private int _startAsteroidsOnDemoScreen = 15;
    [Export]
    private int _minAsteroidsOnDemoScreen = 10;

    [ExportCategory("Testing")]
    [Export]
    private bool _infiniteLives = false;

    // Score table
    private readonly Dictionary<AsteroidSize, int> _asteroidScores = new();

    // Scene references
    private AsteroidFieldController _asteroidFieldController;
    private PlayerController _playerController;
    private Score _score;
    private Lives _lives;
    private Label _gameOverLabel;
    private Label _helpLabel;
    private PushStart _pushStartLabel;
    private Area2D _exclusionZone;
    private Beats _beats;
    private CollisionShape2D _exclusionZoneCollisionShape;
    private SettingsDialog _settingsDialog;
    private HelpDialog _helpDialog;
    private Panel _background;
    private SaucerController _largeSaucerController;
    private SaucerController _smallSaucerController;
    private HighScoreTable _highScoreTable;
    private Label _highScoreLabel;

    // State
    private int _asteroidsCurrentNewGameStart;
    private int _extraLifeThresholdNext;
    private Vector2 _shipSpawnPosition;

    // Game state machine
    private enum GameState { WaitingToPlay, AwaitingNewShip, Playing, ShowingConfigDialog, ShowingHelpDialog };
    private GameState _gameState;

    // Timers
    private Timer _showingGameOver = new();
    private Timer _showingHighScores = new();

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
        try
        {
            var settings = SettingsPersistence.Load(_SETTINGS_SAVE_PATH);
            if (settings != null)
            {
                _settingsDialog.ActiveSettings = settings;
            }
        }
        catch
        {
        }

        ApplyConfiguration(_settingsDialog.ActiveSettings);

        var highScores = HighScorePersistence.Load(_HIGH_SCORE_SAVE_PATH);
        if (highScores != null)
        {
            _highScoreTable.SetHighScores(highScores);
        }
        _highScoreLabel.Text = _highScoreTable.HighScore.ToString();

        CreateDemoScreen();

        _smallSaucerController.TargetCallback = () => _playerController.PlayerPosition;

        // Waiting to start
        WaitingToPlay();

        _showingGameOver.OneShot = true;
        _showingGameOver.Autostart = false;
        _showingGameOver.Timeout += ShowingGameOverOnTimeout;
        AddChild(_showingGameOver);

        _showingHighScores.OneShot = true;
        _showingHighScores.Autostart = false;
        _showingHighScores.Timeout += ShowingHighScoresTimeout;
        AddChild(_showingHighScores);
    }

    private void ShowingGameOverOnTimeout()
    {
        _gameOverLabel.Hide();
        _helpLabel.Hide();
        _highScoreTable.Show();
        _showingHighScores.Start(_HIGH_SCORE_SHOW_TIME);
    }

    private void ShowingHighScoresTimeout()
    {
        _highScoreTable.Hide();
        _helpLabel.Show();
    }

    private void EnableFx(bool enable)
    {
        _asteroidFieldController.EnableFx(enable);
        _playerController.EnableFx(enable);
        _smallSaucerController.EnableFx(enable);
        _largeSaucerController.EnableFx(enable);
    }

    private void CreateDemoScreen()
    {
        EnableFx(false);

        _smallSaucerController.SpawnTimerMax = _smallSaucerStartMinSpawnTimer;
        _smallSaucerController.SpawnTimerMin = _smallSaucerStartMaxSpawnTimer;
        _largeSaucerController.SpawnTimerMax = _largeSaucerStartMinSpawnTimer;
        _largeSaucerController.SpawnTimerMin = _largeSaucerStartMaxSpawnTimer;

        _largeSaucerController.Activate();

        _asteroidFieldController.SpawnField(_startAsteroidsOnDemoScreen, new Rect2(), false);
    }

    private void SetupSceneReferences()
    {
        // Get references too all required scenes
        _playerController = GetNode<PlayerController>("PlayerController");
        _asteroidFieldController = GetNode<AsteroidFieldController>("AsteroidFieldController");
        _score = (Score)FindChild("Score");
        _lives = (Lives)FindChild("Lives");
        _gameOverLabel = (Label)FindChild("Game Over");
        _pushStartLabel = (PushStart)FindChild("Push Start");
        _helpLabel = (Label)FindChild("Help");
        _exclusionZone = GetNode<Area2D>("ExclusionZone");
        _exclusionZoneCollisionShape = _exclusionZone.GetNode<CollisionShape2D>("CollisionShape2D");
        _beats = GetNode<Beats>("Beats");
        _settingsDialog = GetNode<SettingsDialog>("Settings Dialog");
        _helpDialog = GetNode<HelpDialog>("Help Dialog");
        _background = GetNode<Panel>("Background");
        _largeSaucerController = GetNode<SaucerController>("LargeSaucerController");
        _smallSaucerController = GetNode<SaucerController>("SmallSaucerController");
        _highScoreTable = GetNode<HighScoreTable>("HighScoreTable");
        _highScoreLabel = (Label)FindChild("High Score");
    }

    private void SetupSceneSignals()
    {
        // Player ship
        _playerController.Exploding += PlayerOnExploding;
        _playerController.Exploded += PlayerOnExploded;

        // Asteroids
        _asteroidFieldController.Collided += AsteroidFieldControllerOnCollided;
        _asteroidFieldController.FieldCleared += AsteroidFieldControllerOnFieldCleared;

        // Saucers
        _smallSaucerController.Collided += SmallSaucerOnCollided;
        _largeSaucerController.Collided += LargeSaucerOnCollided;

        // Configuration settings
        _settingsDialog.OkPressed += SettingsDialogOnOkPressed;
        _settingsDialog.Cancel += SettingsDialogOnCancel;

        // Controls
        _helpDialog.OkPressed += HelpDialogOnOkPressed;

        // Window resize
        GetTree().GetRoot().SizeChanged += WindowOnSizeChanged;
    }

    public override void _Process(double delta)
    {
        // Ready to spawn new ship
        if (_gameState == GameState.AwaitingNewShip)
        {
            // But we need to wait until the area around the spawn
            // point is free from asteroids
            if (!_exclusionZone.HasOverlappingAreas())
            {
                _gameState = GameState.Playing;
                _exclusionZoneCollisionShape.Disabled = true;
                _playerController.Activate(_shipSpawnPosition);
            }
        }
        else if (_gameState == GameState.WaitingToPlay)
        {
            if (_smallSaucerController.IsActive && !_smallSaucerController.IsSaucerActive &&
                !_largeSaucerController.IsActive)
            {
                _largeSaucerController.Activate();
                _smallSaucerController.Deactivate();
            }

            if (_asteroidFieldController.AsteroidCount < _minAsteroidsOnDemoScreen)
            {
                CreateDemoScreen();
            }

        }
    }

    private void SmallSaucerOnCollided(Saucer saucer, Node collidedWith)
    {
        Logger.I.SignalReceived(this, saucer, Saucer.SignalName.Collided, collidedWith, "SMALL");
        if (collidedWith is Missile)
        {
            IncreaseScore(_scoreSaucerSmall);
        }
    }

    private void LargeSaucerOnCollided(Saucer saucer, Node collidedWith)
    {
        Logger.I.SignalReceived(this, saucer, Saucer.SignalName.Collided, collidedWith, "LARGE");
        if (collidedWith is Missile)
        {
            IncreaseScore(_scoreSaucerLarge);
        }
    }

    private void AsteroidFieldControllerOnCollided(Asteroid asteroid, AsteroidSize size, Node collidedWith)
    {
        Logger.I.SignalReceived(this, asteroid, AsteroidFieldController.SignalName.Collided, size, collidedWith);
        if (collidedWith is Missile missile)
        {
            if (missile.GetParent()?.GetParent() is PlayerController)
            {
                IncreaseScore(_asteroidScores[size]);
            }
        }
    }

    private void AsteroidFieldControllerOnFieldCleared()
    {
        // Increment number of asteroids to spawn and keep it within permitted range
        _asteroidsCurrentNewGameStart += _asteroidsNewSheetDelta;
        _asteroidsCurrentNewGameStart = Mathf.Clamp(_asteroidsCurrentNewGameStart, 0, _asteroidsNewGameMax);

        // Spawn the new field of asteroids
        _asteroidFieldController.SpawnField(_asteroidsCurrentNewGameStart,
            new Rect2(_playerController.PlayerPosition.X - _safeZoneRadius, _playerController.PlayerPosition.Y - _safeZoneRadius,
                      _safeZoneRadius * 2, _safeZoneRadius * 2),
                      true);

        _largeSaucerController.SpawnTimerMax = Mathf.Max(_largeSaucerController.SpawnTimerMax * _largeSaucerSpawnTimerDeltaProportion, _largeSaucerSpawnTimerFloor);
        _largeSaucerController.SpawnTimerMin = Mathf.Max(_largeSaucerController.SpawnTimerMin * _largeSaucerSpawnTimerDeltaProportion, _largeSaucerSpawnTimerFloor); ;

        _smallSaucerController.SpawnTimerMax = Mathf.Max(_smallSaucerController.SpawnTimerMax * _smallSaucerSpawnTimerDeltaProportion, _smallSaucerSpawnTimerFloor);
        _smallSaucerController.SpawnTimerMin = Mathf.Max(_smallSaucerController.SpawnTimerMin * _smallSaucerSpawnTimerDeltaProportion, _smallSaucerSpawnTimerFloor);

        // Reset beats sounds to slowest pace
        _beats.Reset();
    }

    private void PlayerOnExploded()
    {
        if (_gameState == GameState.Playing)
        {
            // Hide the player/stop processing
            _playerController.Deactivate();

            // This is safe wrt signal delivery order
            // as there is a gap between "Exploding" and "Exploded"
            _gameState = GameState.AwaitingNewShip;
            _beats.Start();
        }
    }

    private void PlayerOnExploding()
    {
        // Just a safety check!
        if (_lives.Value > 0)
        {
            // Stop playing the beats sounds
            _beats.Stop();

            // If we don't have infinite lives selected then decrement remaining lives
            if (!_infiniteLives)
            {
                _lives.RemoveLife();
            }

            // If we have no lives left then flag it
            if (_lives.Value == 0)
            {
                GameOver();
            }
            else
            {
                // Enable the exclusion zone to ensure the new player spawn will be safe
                _exclusionZoneCollisionShape.SetDeferred(CollisionShape2D.PropertyName.Disabled, false);
            }
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
            ShipAcceleration = _playerController.PlayerThrustForce,
            ShipRotationSpeed = _playerController.PlayerRotationSpeed,

            AsteroidsRotationEnabled = _asteroidFieldController.IsRotationEnabled,
            AsteroidsStartingQuantity = _asteroidsNewGameStart,
            AsteroidsMaxStartingQuantity = _asteroidsNewGameMax,
            AsteroidsMinSpeed = _asteroidFieldController.MinSpeed,
            AsteroidsMaxSpeed = _asteroidFieldController.MaxSpeed,
            AsteroidsGravityEnabled = _playerController.PlayerGravitationalPullCallback != null,
            AsteroidsGravitationalConstant = _asteroidFieldController.GravitationalMultiplier,
            MissilesMax = _playerController.MissileCount,
            MissilesSpeed = _playerController.MissileSpeed,
            MissilesLifespan = _playerController.MissileDuration
        };
    }

    private void ShowConfigDialog()
    {
        _pushStartLabel.Hide();
        _helpLabel.Hide();
        _showingGameOver.Stop();
        _showingHighScores.Stop();
        _highScoreTable.Hide();
        _gameOverLabel.Hide();

        _settingsDialog.ActiveSettings = CollectConfiguration();
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
        _playerController.PlayerThrustForce = config.ShipAcceleration;
        _playerController.PlayerRotationSpeed = config.ShipRotationSpeed;

        _asteroidFieldController.IsRotationEnabled = config.AsteroidsRotationEnabled;
        _asteroidsNewGameStart = config.AsteroidsStartingQuantity;
        _asteroidsNewGameMax = config.AsteroidsMaxStartingQuantity;
        _asteroidFieldController.MinSpeed = config.AsteroidsMinSpeed;
        _asteroidFieldController.MaxSpeed = config.AsteroidsMaxSpeed;
        _playerController.PlayerGravitationalPullCallback = config.AsteroidsGravityEnabled ? _asteroidFieldController.GetGravitationalVector : null;
        _asteroidFieldController.GravitationalMultiplier = config.AsteroidsGravitationalConstant;
        _playerController.MissileCount = config.MissilesMax;
        _playerController.MissileSpeed = config.MissilesSpeed;
        _playerController.MissileDuration = config.MissilesLifespan;
    }

    private void SettingsDialogOnOkPressed()
    {
        _pushStartLabel.Show();
        _helpLabel.Show();

        ApplyConfiguration(_settingsDialog.ActiveSettings);

        SettingsPersistence.Save(_settingsDialog.ActiveSettings, _SETTINGS_SAVE_PATH);

        _gameState = GameState.WaitingToPlay;
    }

    private void SettingsDialogOnCancel()
    {
        _pushStartLabel.Show();
        _helpLabel.Show();
        _gameState = GameState.WaitingToPlay;
    }

    private void WaitingToPlay()
    {
        _pushStartLabel.Show();
        _helpLabel.Show();

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
        _helpLabel.Hide();
        _pushStartLabel.Hide();
        _gameOverLabel.Hide();

        _showingGameOver.Stop();
        _showingHighScores.Stop();
        _highScoreTable.Hide();

        _helpDialog.Show();
        _gameState = GameState.ShowingHelpDialog;
    }

    private void HelpDialogOnOkPressed()
    {
        _helpLabel.Show();
        _pushStartLabel.Show();

        _gameState = GameState.WaitingToPlay;
    }

    private void NewGame()
    {
        // Starting lives
        _asteroidsCurrentNewGameStart = _asteroidsNewGameStart;
        _lives.SetLives(_livesNewGame);

        // Reset score to 0
        _score.Value = 0;

        // Threshold for first extra life
        _extraLifeThresholdNext = _lifeExtraThreshold;

        // Don't need the exclusion zone as we are creating new asteroids
        _exclusionZoneCollisionShape.Disabled = true;

        // Stop timers
        _showingGameOver.Stop();
        _showingHighScores.Stop();

        // Hide UI labels
        _pushStartLabel.Hide();
        _gameOverLabel.Hide();
        _helpLabel.Hide();

        // Hide high score tables
        _highScoreTable.Hide();

        // Create the new asteroid fields
        _asteroidFieldController.SpawnField(_asteroidsCurrentNewGameStart,
           new Rect2(_shipSpawnPosition.X - _safeZoneRadius, _shipSpawnPosition.Y - _safeZoneRadius,
                     _safeZoneRadius * 2, _safeZoneRadius * 2),
           true);

        // Set up saucers
        _smallSaucerController.SpawnTimerMax = _smallSaucerStartMinSpawnTimer;
        _smallSaucerController.SpawnTimerMin = _smallSaucerStartMaxSpawnTimer;
        _smallSaucerController.Deactivate(true);
        _smallSaucerController.Activate();
        _largeSaucerController.SpawnTimerMax = _largeSaucerStartMinSpawnTimer;
        _largeSaucerController.SpawnTimerMin = _largeSaucerStartMaxSpawnTimer;
        _largeSaucerController.Deactivate(true);
        _largeSaucerController.Activate();

        // Enable FX audio
        //SetFxAudioEnabled(true);
        EnableFx(true);

        // Start playing the beats sounds
        _beats.Reset();
        _beats.Start();

        // Flag we are ready to spawn a new ship
        _gameState = GameState.AwaitingNewShip;
    }


    private void GameOver()
    {
        _beats.Stop();
        _gameOverLabel.Show();

        _showingGameOver.Start(_GAME_OVER_SHOW_TIME);

        WaitingToPlay();
    }

    private void WindowOnSizeChanged()
    {
        _shipSpawnPosition = Screen.Instance.Centre;
        _exclusionZone.Position = _shipSpawnPosition;
    }

    private static void SetSoundEnabled(bool enabled)
    {
        AudioServer.SetBusMute(AudioServer.GetBusIndex(Constants.AUDIO_BUS_NAME_MASTER), !enabled);
    }

    private static bool GetSoundEnabled()
    {
        return !AudioServer.IsBusMute(AudioServer.GetBusIndex(Constants.AUDIO_BUS_NAME_MASTER));
    }

    private static void SetFxAudioEnabled(bool enabled)
    {
        AudioServer.SetBusMute(AudioServer.GetBusIndex(Constants.AUDIO_BUS_NAME_FX), !enabled);
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
