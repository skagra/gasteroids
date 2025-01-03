using System;
using Godot;

namespace Asteroids;

using static Asteroids.UiUtils;
using SettingsPresets = GameSettingsPresets.SettingsPresets;

public partial class Main : Node
{
    private const string _SETTINGS_SAVE_PATH = "user://settings.json";
    private const string _HIGH_SCORE_SAVE_PATH = "user://highscores.json";

    [ExportCategory("Asteroids")]

    [Export]
    private int _asteroidsNewSheetDelta = 2;
    [Export]
    private float _asteroidsSpeedDelta = 1.2f;

    [ExportCategory("Saucers")]
    [Export]
    private float _largeSaucerSpawnTimerFloor = 5;
    [Export]
    private float _largeSaucerSpawnTimerDeltaProportion = 0.5f;

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
    private int _minAsteroidsOnDemoScreen = 4;

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
    private MainAnimationPlayer _mainAnimationPlayer;
    private Ui _ui;
    private EnterHighScore _enterHighScore;
    private Scores _scores;

    // State
    private int _asteroidsCurrentInitialQuantity;
    private int _extraLifeThresholdNext;
    private Vector2 _shipSpawnPosition;

    // Game state machine
    private enum GameState { WaitingToPlay, AwaitingNewShip, Playing, ShowingConfigDialog, ShowingHelpDialog, EnteringHighScore };
    private GameState _gameState;

    // Game settings
    private GameSettingsBridge _settingsBridge;
    private GameSettings _gameSettings;

    // UI hide and show utils
    UiUtils _uiUtils;

    public override void _Ready()
    {
        Logger.I.Debug("Main scene ready");

        // Scene references
        SetupSceneReferences();

        // Signals
        SetupSceneSignals();

        // Set up UI utils
        _uiUtils = new UiUtils(_splashScreen, _ui, _fadingOverlay, _highScoreTable, _helpDialog, _settingsDialog);

        // New ship spawn location
        _shipSpawnPosition = Screen.Centre;

        // Asteroid exclusion zone - ensures ship spawns are safe
        var circleShape = (CircleShape2D)_exclusionZoneCollisionShape.Shape;
        circleShape.Radius = _safeZoneRadius;
        _exclusionZone.Position = _shipSpawnPosition;

        // Load and apply configuration
        _settingsBridge = new GameSettingsBridge(_playerController, _asteroidFieldController,
                                          _largeSaucerController, _smallSaucerController, _beats);
        _gameSettings = GameSettingsPersistence.Load(_SETTINGS_SAVE_PATH) ?? GameSettingsPresets.GetSettings(SettingsPresets.Normal);
        _settingsBridge.Apply(_gameSettings);

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

        // Splash screen followed by main animation loop
        _mainAnimationPlayer.PlaySplash();
    }

    private void ActivateSplashScreen()
    {
        _uiUtils.ShowAndHide(ViewableElements.SplashScreen | ViewableElements.FadingOverlay, ViewableElements.FadingOverlay);
        _splashScreen.Activate();
    }

    private void SplashOnSplashDone()
    {
        _uiUtils.ShowAndHide(ViewableElements.HelpLabel | ViewableElements.StartLabel | ViewableElements.FadingOverlay, ViewableElements.FadingOverlay);
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
            if (_asteroidFieldController.AsteroidCount < _minAsteroidsOnDemoScreen) // && _endOfGameGracePeriodExpired)
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
        _playerController = GetNode<PlayerController>("PlayerController") ?? throw new NullReferenceException("PlayerController not found");
        _asteroidFieldController = GetNode<AsteroidFieldController>("AsteroidFieldController") ?? throw new NullReferenceException("AsteroidFieldController not found");
        _exclusionZone = GetNode<Area2D>("ExclusionZone") ?? throw new NullReferenceException("ExclusionZone not found");
        _exclusionZoneCollisionShape = _exclusionZone.GetNode<CollisionShape2D>("CollisionShape2D") ?? throw new NullReferenceException("CollisionShape2D not found");
        _beats = GetNode<Beats>("Beats") ?? throw new NullReferenceException("Beats not found");
        _settingsDialog = GetNode<GameSettingsDialog>("Game Settings Dialog") ?? throw new NullReferenceException("Game Settings Dialog not found");
        _helpDialog = GetNode<HelpDialog>("Help Dialog") ?? throw new NullReferenceException("Help Dialog not found");
        _largeSaucerController = GetNode<SaucerController>("LargeSaucerController") ?? throw new NullReferenceException("LargeSaucerController not found");
        _smallSaucerController = GetNode<SaucerController>("SmallSaucerController") ?? throw new NullReferenceException("SmallSaucerController not found");
        _highScoreTable = GetNode<HighScoreTable>("HighScoreTable") ?? throw new NullReferenceException("HighScoreTable not found");
        _fadingOverlay = (FadingPanelContainer)FindChild("FadingOverlay") ?? throw new NullReferenceException("FadingOverlay not found");
        _splashScreen = (Splash)FindChild("Splash") ?? throw new NullReferenceException("Splash not found");
        _mainAnimationPlayer = (MainAnimationPlayer)FindChild("MainAnimationPlayer") ?? throw new NullReferenceException("MainAnimationPlayer not found");
        _ui = (Ui)FindChild("UI") ?? throw new NullReferenceException("UI not found");
        _enterHighScore = (EnterHighScore)FindChild("EnterHighScore") ?? throw new NullReferenceException("EnterHighScore not found");
        _scores = (Scores)FindChild("Scores") ?? throw new NullReferenceException("Scores not found");
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

        // High score name entered
        _enterHighScore.NameEntered += HighScoreOnNameEntered;
    }

    private void CreateDemoScreen()
    {
        var demoSettings = GameSettingsPresets.GetSettings(SettingsPresets.Demo);
        _settingsBridge.Apply(demoSettings, GameSettingsBridge.Fields.Theme | GameSettingsBridge.Fields.Sound);
        EnableNewSoundFx(false);

        if (demoSettings.LargeSaucerEnabled)
        {
            _largeSaucerController.Activate();
        }

        if (demoSettings.SmallSaucerEnabled)
        {
            _smallSaucerController.Activate();
        }

        _asteroidFieldController.SpawnField(demoSettings.AsteroidsInitialQuantity, new Rect2(), false);
    }

    // <-- Setup

    // Game state control ->

    private void WaitingToPlay()
    {
        _uiUtils.ShowAndHide(ViewableElements.FadingOverlay, ViewableElements.FadingOverlay);
        Logger.I.Debug("Setting game state to {0}", GameState.WaitingToPlay);
        _gameState = GameState.WaitingToPlay;
    }

    private void NewGame()
    {
        // Apply the current configuration
        _settingsBridge.Apply(_gameSettings);

        // Starting lives
        _ui.Lives = _gameSettings.PlayerStartingLives;

        // Reset score to 0
        _ui.Score = 0;

        // Threshold for first extra life
        _extraLifeThresholdNext = _gameSettings.PlayerExtraLifeScoreThreshold;

        // Starting quantity of asteroids
        _asteroidsCurrentInitialQuantity = _gameSettings.AsteroidsInitialQuantity;

        // Don't need the exclusion zone as we are creating new asteroids
        _exclusionZoneCollisionShape.Disabled = true;

        // Hide UI elements
        _mainAnimationPlayer.Stop();
        _uiUtils.ShowAndHide(ViewableElements.None);

        // Create the new asteroid fields
        _asteroidFieldController.SpawnField(_asteroidsCurrentInitialQuantity,
           new Rect2(_shipSpawnPosition.X - _safeZoneRadius, _shipSpawnPosition.Y - _safeZoneRadius,
                     _safeZoneRadius * 2, _safeZoneRadius * 2),
           true);

        // Saucers
        _largeSaucerController.Deactivate(true);
        if (_gameSettings.LargeSaucerEnabled)
        {
            _largeSaucerController.Activate();
        }

        _smallSaucerController.Deactivate(true);
        if (_gameSettings.SmallSaucerEnabled)
        {
            _smallSaucerController.Activate();
        }

        // Enable FX audio according to configuration
        EnableNewSoundFx(_gameSettings.SoundEnabled);

        // Start playing the beats sounds
        _beats.Reset();
        _beats.Start();

        // Flag we are ready to spawn a new ship
        Logger.I.Debug("Setting game state to {0}", GameState.AwaitingNewShip);
        _gameState = GameState.AwaitingNewShip;
    }

    private void GameOver()
    {
        // Stop playing the background beats
        _beats.Stop();

        // Show the fading overlay
        _uiUtils.ShowAndHide(ViewableElements.FadingOverlay);

        // Disable all new sound fxs
        EnableNewSoundFx(false);

        // Check if the score is high enough to be included in the high score table
        if (_highScoreTable.IsEligibleForInclusion(_ui.Score))
        {
            _ui.ShowGameOverLabel();
            _enterHighScore.Show();
            Logger.I.Debug("Setting game state to {0}", GameState.EnteringHighScore);
            _gameState = GameState.EnteringHighScore;
        }
        else
        {
            _mainAnimationPlayer.PlayGameOver();
            WaitingToPlay();
        }
    }

    private void HighScoreOnNameEntered(string name)
    {
        _highScoreTable.AddScore(name, _ui.Score);
        HighScorePersistence.Save(_highScoreTable.GetScores(), _HIGH_SCORE_SAVE_PATH);
        _enterHighScore.Hide();

        _uiUtils.ShowAndHide(ViewableElements.FadingOverlay, ViewableElements.FadingOverlay);
        _mainAnimationPlayer.PlayMainLoop();

        WaitingToPlay();
    }

    // <-- Game state control

    // --> Game events

    private void SmallSaucerOnCollided(Saucer saucer, Node collidedWith)
    {
        Logger.I.SignalReceived(this, saucer, Saucer.SignalName.Collided, collidedWith, "SMALL");
        if (collidedWith is Missile)
        {
            IncreaseScore(_scores.SaucerSmall);
        }
    }

    private void LargeSaucerOnCollided(Saucer saucer, Node collidedWith)
    {
        Logger.I.SignalReceived(this, saucer, Saucer.SignalName.Collided, collidedWith, "LARGE");
        if (collidedWith is Missile)
        {
            IncreaseScore(_scores.SaucerLarge);
        }
    }

    private void AsteroidFieldControllerOnCollided(Asteroid asteroid, AsteroidSize size, Node collidedWith)
    {
        Logger.I.SignalReceived(this, asteroid, AsteroidFieldController.SignalName.Collided, size, collidedWith);
        if (collidedWith is Missile missile)
        {
            if (missile.GetParent()?.GetParent() is PlayerController)
            {
                IncreaseScore(_scores.AsteroidScore(size));
            }
        }
    }
    private void IncreaseScore(int increase)
    {
        _ui.Score += increase;
        if (_ui.Score > _extraLifeThresholdNext)
        {
            _extraLifeThresholdNext += _gameSettings.PlayerExtraLifeScoreThreshold;
            if (_ui.Lives < _gameSettings.PlayerMaxLives)
            {
                _ui.AddLife();
            }
        }
    }

    private void AsteroidFieldControllerOnFieldCleared(AsteroidFieldController asteroidFieldController)
    {
        Logger.I.SignalReceived(this, asteroidFieldController, AsteroidFieldController.SignalName.FieldCleared);

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
        // Increase the number of asteroids to spawn and keep it within permitted range
        _asteroidsCurrentInitialQuantity += _asteroidsNewSheetDelta;
        _asteroidsCurrentInitialQuantity = Mathf.Min(_asteroidsCurrentInitialQuantity, _gameSettings.AsteroidsMaxQuantity);

        // Increase asteroid speed
        _asteroidFieldController.MinSpeed *= _asteroidsSpeedDelta;
        _asteroidFieldController.MaxSpeed *= _asteroidsSpeedDelta;

        // Increase the frequency at which saucers spawn
        _largeSaucerController.SpawnTimerMax = Mathf.Max(_largeSaucerController.SpawnTimerMax * _largeSaucerSpawnTimerDeltaProportion, _largeSaucerSpawnTimerFloor);
        _largeSaucerController.SpawnTimerMin = Mathf.Max(_largeSaucerController.SpawnTimerMin * _largeSaucerSpawnTimerDeltaProportion, _largeSaucerSpawnTimerFloor);

        _smallSaucerController.SpawnTimerMax = Mathf.Max(_smallSaucerController.SpawnTimerMax * _smallSaucerSpawnTimerDeltaProportion, _smallSaucerSpawnTimerFloor);
        _smallSaucerController.SpawnTimerMin = Mathf.Max(_smallSaucerController.SpawnTimerMin * _smallSaucerSpawnTimerDeltaProportion, _smallSaucerSpawnTimerFloor);
    }

    private void PlayerOnExploded(PlayerController playerController)
    {
        Logger.I.SignalReceived(this, playerController, PlayerController.SignalName.Exploded);

        if (_gameState == GameState.Playing)
        {
            // Hide the player/stop processing
            _playerController.Deactivate();

            // This is safe wrt signal delivery order
            // as there is a gap between "Exploding" and "Exploded"
            Logger.I.Debug("Setting game state to {0}", GameState.AwaitingNewShip);
            _gameState = GameState.AwaitingNewShip;
            _beats.Start();
        }
    }

    private void PlayerOnExploding(PlayerController playerController)
    {
        Logger.I.SignalReceived(this, playerController, PlayerController.SignalName.Exploding);

        // Just a safety check!
        if (_ui.Lives > 0)
        {
            // Stop playing the beats sounds
            _beats.Stop();

            // If we don't have infinite lives selected then decrement remaining lives
            if (!_gameSettings.PlayerInfiniteLives)
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
        _mainAnimationPlayer.Stop();
        _settingsDialog.ActiveSettings = _gameSettings;
        _uiUtils.ShowAndHide(ViewableElements.SettingsDialog | ViewableElements.FadingOverlay, ViewableElements.FadingOverlay);
        Logger.I.Debug("Setting game state to {0}", GameState.ShowingConfigDialog);
        _gameState = GameState.ShowingConfigDialog;
    }

    private void SettingsDialogOnOkPressed()
    {
        _uiUtils.ShowAndHide(ViewableElements.StartLabel | ViewableElements.HelpLabel | ViewableElements.FadingOverlay, ViewableElements.FadingOverlay);
        _gameSettings = new GameSettings(_settingsDialog.ActiveSettings); // TODO Or better dialog can give back a copy
        _settingsBridge.Apply(_gameSettings, GameSettingsBridge.Fields.Sound);
        GameSettingsPersistence.Save(_settingsDialog.ActiveSettings, _SETTINGS_SAVE_PATH);
        _mainAnimationPlayer.PlayDelayedMainLoop();
        Logger.I.Debug("Setting game state to {0}", GameState.WaitingToPlay);
        _gameState = GameState.WaitingToPlay;
    }

    private void SettingsDialogOnCancel()
    {
        _uiUtils.ShowAndHide(ViewableElements.StartLabel | ViewableElements.HelpLabel | ViewableElements.FadingOverlay, ViewableElements.FadingOverlay);
        _mainAnimationPlayer.PlayDelayedMainLoop();
        Logger.I.Debug("Setting game state to {0}", GameState.WaitingToPlay);
        _gameState = GameState.WaitingToPlay;
    }

    // <-- Configuration

    // Help dialog -->

    private void ShowHelpDialog()
    {
        _mainAnimationPlayer.Stop();
        _uiUtils.ShowAndHide(ViewableElements.HelpDialog | ViewableElements.FadingOverlay, ViewableElements.FadingOverlay);
        Logger.I.Debug("Setting game state to {0}", GameState.ShowingHelpDialog);
        _gameState = GameState.ShowingHelpDialog;
    }

    private void HelpDialogOnOkPressed()
    {
        _uiUtils.ShowAndHide(ViewableElements.StartLabel | ViewableElements.HelpLabel | ViewableElements.FadingOverlay, ViewableElements.FadingOverlay);
        _mainAnimationPlayer.PlayDelayedMainLoop();
        Logger.I.Debug("Setting game state to {0}", GameState.WaitingToPlay);
        _gameState = GameState.WaitingToPlay;
    }

    // <-- Help dialog

    // Audio -->

    private void EnableNewSoundFx(bool enable)
    {
        GetTree().CallGroup("SoundFx", "EnableFx", enable);  // TODO Pull out names into constants
    }

    // <-- Audio
}
