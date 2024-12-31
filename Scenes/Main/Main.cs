using System;
using System.Collections.Generic;
using Godot;

namespace Asteroids;

using SettingsPresets = GameSettingsPresets.SettingsPresets;

public partial class Main : Node
{
    private const string _SETTINGS_SAVE_PATH = "user://settings.json";
    private const string _HIGH_SCORE_SAVE_PATH = "user://highscores.json";

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

    [ExportCategory("Asteroids")]

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

    // Score table
    private readonly Dictionary<AsteroidSize, int> _asteroidScores = new();

    // Scene references
    private AsteroidFieldController _asteroidFieldController;
    private PlayerController _playerController;
    private Area2D _exclusionZone;
    private Beats _beats;
    private CollisionShape2D _exclusionZoneCollisionShape;
    private GameSettingsDialog _settingsDialog;
    private HelpDialog _helpDialog;
    private SaucerController _largeSaucerController;
    private SaucerController _smallSaucerController;
    private HighScoreTable _highScoreTable;
    private FadingPanelContainer _fadingOverlay;
    private Splash _splashScreen;
    private AnimationPlayer _gameOverAnimationPlayer;
    private Ui _ui;

    // Timers
    private Timer _endOfGameGracePeriodExpiredTimer;
    private bool _endOfGameGracePeriodExpired = true;

    // State
    private int _asteroidsCurrentInitialQuantity;
    private int _extraLifeThresholdNext;
    private Vector2 _shipSpawnPosition;

    // Game state machine
    private enum GameState { WaitingToPlay, AwaitingNewShip, Playing, ShowingConfigDialog, ShowingHelpDialog };
    private GameState _gameState;

    // Configurable settings
    private GameSettingsBridge _settings;

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

        // Timers
        SetupTimers();

        // New ship spawn location
        _shipSpawnPosition = Screen.Centre;

        // Asteroid exclusion zone - ensures ship spawns are safe
        var circleShape = (CircleShape2D)_exclusionZoneCollisionShape.Shape;
        circleShape.Radius = _safeZoneRadius;
        _exclusionZone.Position = _shipSpawnPosition;

        // Load and apply configuration
        _settings = new GameSettingsBridge(_playerController, _asteroidFieldController,
                                          _largeSaucerController, _smallSaucerController);
        var settings = GameSettingsPersistence.Load(_SETTINGS_SAVE_PATH);
        _settings.GameSettings = settings ?? GameSettingsPresets.GetSettings(SettingsPresets.Normal);

        // Load and apply high scores
        var highScores = HighScorePersistence.Load(_HIGH_SCORE_SAVE_PATH);
        if (highScores != null)
        {
            _highScoreTable.SetHighScores(highScores);
        }
        _ui.HighScore = _highScoreTable.HighScore;

        // Screen overlay to dim background when not playing
        _fadingOverlay.SpeedScale = 0.1f;

        // Waiting to start
        WaitingToPlay();

        // Background asteroid field
        CreateDemoScreen();

        // Splash screen
        ActivateSplashScreen();
    }

    private void ActivateSplashScreen()
    {
        ShowAndHide(ViewableElements.SplashScreen | ViewableElements.FadingOverlay, ViewableElements.FadingOverlay);

        _splashScreen.Activate();
    }

    private void SplashOnSplashDone()
    {
        ShowAndHide(ViewableElements.HelpLabel | ViewableElements.StartLabel | ViewableElements.FadingOverlay, ViewableElements.FadingOverlay);
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
            // If we are not actively playing then disable the small saucer once it is off screen
            if (_smallSaucerController.IsActive && !_smallSaucerController.IsSaucerActive &&
                !_largeSaucerController.IsActive)
            {
                _largeSaucerController.Activate();
                _smallSaucerController.Deactivate();
            }

            // Create a new demo asteroid field if the current one has become too sparse
            // after a grace period
            if (_asteroidFieldController.AsteroidCount < _minAsteroidsOnDemoScreen && _endOfGameGracePeriodExpired)
            {
                CreateDemoScreen();
            }
        }
    }

    public override void _UnhandledInput(InputEvent inputEvent)
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

    // Setup -->

    private void SetupSceneReferences()
    {
        // Get references too all required scenes
        _playerController = GetNode<PlayerController>("PlayerController");
        _asteroidFieldController = GetNode<AsteroidFieldController>("AsteroidFieldController");
        _exclusionZone = GetNode<Area2D>("ExclusionZone");
        _exclusionZoneCollisionShape = _exclusionZone.GetNode<CollisionShape2D>("CollisionShape2D");
        _beats = GetNode<Beats>("Beats");
        _settingsDialog = GetNode<GameSettingsDialog>("Game Settings Dialog");
        _helpDialog = GetNode<HelpDialog>("Help Dialog");
        _largeSaucerController = GetNode<SaucerController>("LargeSaucerController");
        _smallSaucerController = GetNode<SaucerController>("SmallSaucerController");
        _highScoreTable = GetNode<HighScoreTable>("HighScoreTable");
        _fadingOverlay = (FadingPanelContainer)FindChild("FadingOverlay");
        _splashScreen = (Splash)FindChild("Splash");
        _gameOverAnimationPlayer = (AnimationPlayer)FindChild("GameOverAnimationPlayer");
        _ui = (Ui)FindChild("UI");
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

        // Callback to allow small saucer to target the player
        _smallSaucerController.TargetCallback = () => _playerController.PlayerPosition;

        // Splash screen fade completed
        _splashScreen.SplashDone += SplashOnSplashDone;
    }

    private void SetupTimers()
    {
        _endOfGameGracePeriodExpiredTimer = new Timer();
        _endOfGameGracePeriodExpiredTimer.OneShot = true;
        _endOfGameGracePeriodExpiredTimer.Timeout += EndOfGameGracePeriodExpiredOnTimeout;
        AddChild(_endOfGameGracePeriodExpiredTimer);
    }

    private void CreateDemoScreen()
    {
        EnableSoundFx(false);

        _smallSaucerController.SpawnTimerMax = _smallSaucerStartMinSpawnTimer;
        _smallSaucerController.SpawnTimerMin = _smallSaucerStartMaxSpawnTimer;
        _largeSaucerController.SpawnTimerMax = _largeSaucerStartMinSpawnTimer;
        _largeSaucerController.SpawnTimerMin = _largeSaucerStartMaxSpawnTimer;

        _largeSaucerController.Activate();

        _asteroidFieldController.SpawnField(_startAsteroidsOnDemoScreen, new Rect2(), false);
    }

    // <-- Setup

    // Game state control ->

    private void WaitingToPlay()
    {
        ShowAndHide(ViewableElements.StartLabel | ViewableElements.HelpLabel | ViewableElements.FadingOverlay, ViewableElements.FadingOverlay);
        _gameState = GameState.WaitingToPlay;
    }

    private void NewGame()
    {
        // Starting lives
        _ui.Lives = _settings.PlayerStartingLives;

        // Reset score to 0
        _ui.Score = 0;

        // Threshold for first extra life
        _extraLifeThresholdNext = _settings.PlayerExtraLifeScoreThreshold;

        // Starting quantity of asteroids
        _asteroidsCurrentInitialQuantity = _settings.AsteroidsInitialQuantity;

        // Don't need the exclusion zone as we are creating new asteroids
        _exclusionZoneCollisionShape.Disabled = true;

        // Hide UI elements
        _gameOverAnimationPlayer.Stop();
        ShowAndHide(ViewableElements.None);

        // Create the new asteroid fields
        _asteroidFieldController.SpawnField(_asteroidsCurrentInitialQuantity,
           new Rect2(_shipSpawnPosition.X - _safeZoneRadius, _shipSpawnPosition.Y - _safeZoneRadius,
                     _safeZoneRadius * 2, _safeZoneRadius * 2),
           true);

        // Set up saucers TODO - these will be pulled from config and current values will be increased per sheet
        _largeSaucerController.SpawnTimerMax = _largeSaucerStartMinSpawnTimer;
        _largeSaucerController.SpawnTimerMin = _largeSaucerStartMaxSpawnTimer;
        _largeSaucerController.Deactivate(true);
        if (_settings.LargeSaucerEnabled)
        {
            _largeSaucerController.Activate();
        }
        _smallSaucerController.SpawnTimerMax = _smallSaucerStartMinSpawnTimer;
        _smallSaucerController.SpawnTimerMin = _smallSaucerStartMaxSpawnTimer;
        _smallSaucerController.Deactivate(true);
        if (_settings.SmallSaucerEnabled)
        {
            _smallSaucerController.Activate();
        }

        // Enable FX audio according to configuration
        EnableSoundFx(_settings.SoundEnabled);

        // Start playing the beats sounds
        _beats.Reset();
        _beats.Start();

        // Flag we are ready to spawn a new ship
        _gameState = GameState.AwaitingNewShip;
    }

    private void GameOver()
    {
        _endOfGameGracePeriodExpired = false;
        _endOfGameGracePeriodExpiredTimer.Start(5); // TODO

        _beats.Stop();
        ShowAndHide(ViewableElements.FadingOverlay);
        _gameOverAnimationPlayer.Play("GameOver");

        WaitingToPlay();
    }

    private void EndOfGameGracePeriodExpiredOnTimeout()
    {
        _endOfGameGracePeriodExpired = true;
    }

    // <-- Game state control

    // --> Game events

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
    private void IncreaseScore(int increase)
    {
        _ui.Score += increase;
        if (_ui.Score > _extraLifeThresholdNext && _ui.Lives < _settings.PlayerMaxLives)
        {
            _ui.AddLife();
            _extraLifeThresholdNext += _settings.PlayerExtraLifeScoreThreshold;
        }
    }

    private void AsteroidFieldControllerOnFieldCleared()
    {
        IncreaseDifficulty();

        // Spawn the new field of asteroids
        _asteroidFieldController.SpawnField(_asteroidsCurrentInitialQuantity,
            new Rect2(_playerController.PlayerPosition.X - _safeZoneRadius, _playerController.PlayerPosition.Y - _safeZoneRadius,
                      _safeZoneRadius * 2, _safeZoneRadius * 2),
                      true);

        // Reset beats sounds to slowest pace
        _beats.Reset();
    }

    private void IncreaseDifficulty()
    {
        // Increment number of asteroids to spawn and keep it within permitted range
        _asteroidsCurrentInitialQuantity += _asteroidsNewSheetDelta;
        _asteroidsCurrentInitialQuantity = Mathf.Min(_asteroidsCurrentInitialQuantity, _settings.AsteroidsMaxQuantity);

        // Increase the frequency at which saucers spawn
        _largeSaucerController.SpawnTimerMax = Mathf.Max(_largeSaucerController.SpawnTimerMax * _largeSaucerSpawnTimerDeltaProportion, _largeSaucerSpawnTimerFloor);
        _largeSaucerController.SpawnTimerMin = Mathf.Max(_largeSaucerController.SpawnTimerMin * _largeSaucerSpawnTimerDeltaProportion, _largeSaucerSpawnTimerFloor);

        _smallSaucerController.SpawnTimerMax = Mathf.Max(_smallSaucerController.SpawnTimerMax * _smallSaucerSpawnTimerDeltaProportion, _smallSaucerSpawnTimerFloor);
        _smallSaucerController.SpawnTimerMin = Mathf.Max(_smallSaucerController.SpawnTimerMin * _smallSaucerSpawnTimerDeltaProportion, _smallSaucerSpawnTimerFloor);

        // TODO Increase asteroid speed
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
        if (_ui.Lives > 0)
        {
            // Stop playing the beats sounds
            _beats.Stop();

            // If we don't have infinite lives selected then decrement remaining lives
            if (!_settings.PlayerInfiniteLives)
            {
                _ui.RemoveLife();
            }

            // If we have no lives left then flag it
            if (_ui.Lives == 0)
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

    // <-- Game events

    // --> Other events

    private void WindowOnSizeChanged()
    {
        _shipSpawnPosition = Screen.Centre;
        _exclusionZone.Position = _shipSpawnPosition;
    }

    // <-- Other events

    // Configuration -->

    private void ShowConfigDialog()
    {
        _gameOverAnimationPlayer.Stop();
        _settingsDialog.ActiveSettings = _settings.GameSettings;
        ShowAndHide(ViewableElements.SettingsDialog | ViewableElements.FadingOverlay, ViewableElements.FadingOverlay);

        _gameState = GameState.ShowingConfigDialog;
    }

    private void SettingsDialogOnOkPressed()
    {
        ShowAndHide(ViewableElements.StartLabel | ViewableElements.HelpLabel | ViewableElements.FadingOverlay, ViewableElements.FadingOverlay);
        _settings.GameSettings = _settingsDialog.ActiveSettings;
        GameSettingsPersistence.Save(_settingsDialog.ActiveSettings, _SETTINGS_SAVE_PATH);

        _gameState = GameState.WaitingToPlay;
    }

    private void SettingsDialogOnCancel()
    {
        ShowAndHide(ViewableElements.StartLabel | ViewableElements.HelpLabel | ViewableElements.FadingOverlay, ViewableElements.FadingOverlay);

        _gameState = GameState.WaitingToPlay;
    }

    // <-- Configuration

    // Help dialog -->

    private void ShowHelpDialog()
    {
        _gameOverAnimationPlayer.Stop();
        ShowAndHide(ViewableElements.HelpDialog | ViewableElements.FadingOverlay, ViewableElements.FadingOverlay);

        _gameState = GameState.ShowingHelpDialog;
    }

    private void HelpDialogOnOkPressed()
    {
        ShowAndHide(ViewableElements.StartLabel | ViewableElements.HelpLabel | ViewableElements.FadingOverlay, ViewableElements.FadingOverlay);

        _gameState = GameState.WaitingToPlay;
    }

    // <-- Help dialog

    // UI utilities -->

    [Flags]
    private enum ViewableElements
    {
        None = 0b_0000_0000,
        SplashScreen = 0b_0000_0001,
        HelpLabel = 0b_0000_0010,
        StartLabel = 0b_0000_0100,
        GameOverLabel = 0b0000_1000,
        HighScoreTable = 0b0001_0000,
        HelpDialog = 0b0010_0000,
        SettingsDialog = 0b0100_0000,
        FadingOverlay = 0b1000_0000
    }

    private void ShowAndHide(ViewableElements flags, ViewableElements immediate = ViewableElements.None)
    {
        GD.Print(flags);

        if ((flags & ViewableElements.SplashScreen) != 0)
        {
            _splashScreen.Show();
        }
        else
        {
            _splashScreen.Hide();
        }

        if ((flags & ViewableElements.HelpLabel) != 0)
        {
            _ui.ShowHelpLabel();
        }
        else
        {
            _ui.HideHelpLabel();
        }

        if ((flags & ViewableElements.StartLabel) != 0)
        {
            _ui.ShowStartLabel();
        }
        else
        {
            _ui.HideStartLabel();
        }

        if ((flags & ViewableElements.GameOverLabel) != 0)
        {
            _ui.ShowGameOverLabel();
        }
        else
        {
            _ui.HideGameOverLabel();
        }

        if ((flags & ViewableElements.HighScoreTable) != 0)
        {
            _highScoreTable.Show((immediate & ViewableElements.HighScoreTable) != 0);
        }
        else
        {
            _highScoreTable.Hide((immediate & ViewableElements.HighScoreTable) != 0);
        }

        if ((flags & ViewableElements.HelpDialog) != 0)
        {
            _helpDialog.Show((immediate & ViewableElements.HelpDialog) != 0);
        }
        else
        {
            _helpDialog.Hide((immediate & ViewableElements.HelpDialog) != 0);
        }

        if ((flags & ViewableElements.SettingsDialog) != 0)
        {
            _settingsDialog.Show((immediate & ViewableElements.SettingsDialog) != 0);
        }
        else
        {
            _settingsDialog.Hide((immediate & ViewableElements.SettingsDialog) != 0);
        }

        if ((flags & ViewableElements.FadingOverlay) != 0)
        {
            _fadingOverlay.Show((immediate & ViewableElements.FadingOverlay) != 0);
        }
        else
        {
            _fadingOverlay.Hide((immediate & ViewableElements.FadingOverlay) != 0);
        }
    }

    // <-- UI utilities

    // Audio -->

    private void EnableSoundFx(bool enable)
    {
        _asteroidFieldController.EnableFx(enable);
        _playerController.EnableFx(enable);
        _smallSaucerController.EnableFx(enable);
        _largeSaucerController.EnableFx(enable);
        AudioServer.SetBusMute(AudioServer.GetBusIndex(Resources.AUDIO_BUS_NAME_FX), !enable);
    }

    // <-- Audio
}
