using System;
using Godot;

namespace Asteroids;

using static Asteroids.Missile;
using static Asteroids.PowerUpController;
using SettingsPresets = GameSettingsPresets.SettingsPresets;

public partial class GamePlayController : Node
{
    [Signal]
    public delegate void GameOverEventHandler();

    [Signal]
    public delegate void NewGameStartedEventHandler();

    [ExportCategory("Asteroids")]
    [Export]
    private int _asteroidsNewSheetDelta = 2;
    [Export]
    private float _asteroidsSpeedDelta = 1.2f;

    [ExportCategory("Saucers")]
    [Export]
    private float _largeSaucerSpawnTimerFloor = 5;
    [Export]
    private float _largeSaucerSpawnTimerDeltaProportion = 0.75f;

    [Export]
    private float _smallSaucerSpawnTimerFloor = 5;
    [Export]
    private float _smallSaucerSpawnTimerDeltaProportion = 0.75f;

    [ExportCategory("Demo Asteroid Field")]
    [Export]
    private int _minAsteroidsOnDemoScreen = 4;

    [ExportCategory("Misc")]
    [Export]
    private int _safeZoneRadius = 200;

    [Export]
    private EventHub _eventHub;

    public int Score { get => _scoreController.Score; }

    // Scene references
    private AsteroidFieldController _asteroidFieldController;
    private PlayerController _playerController;
    private Area2D _exclusionZone;
    private Beats _beats;
    private CollisionShape2D _exclusionZoneCollisionShape;
    private SaucerController _largeSaucerController;
    private SaucerController _smallSaucerController;
    private ScoreController _scoreController;
    private LivesController _livesController;
    private PowerUpController _powerUpController;

    // State
    private int _asteroidsCurrentInitialQuantity;
    private int _extraLifeThresholdNext;
    private Vector2 _shipSpawnPosition;

    // Game state machine
    private enum GamePlayState { NotPlaying, AwaitingNewShip, Playing };
    private GamePlayState _gamePlayState;

    // Game settings
    private GameSettingsBridge _settingsBridge;

    private int _maxAsteroids;

    public override void _Ready()
    {
        Logger.I.Debug("Main scene ready");

        // Scene references
        SetupSceneReferences();

        // Signals
        SetupSceneSignals();

        // Asteroids count callback - used in hyperspace accident calculation
        _playerController.GetAsteroidsCount = () => _asteroidFieldController.AsteroidCount;

        // New ship spawn location
        _shipSpawnPosition = Screen.Centre;

        // Asteroid exclusion zone - ensures ship spawns are safe
        var circleShape = (CircleShape2D)_exclusionZoneCollisionShape.Shape;
        circleShape.Radius = _safeZoneRadius;
        _exclusionZone.Position = _shipSpawnPosition;

        // Create settings bridge
        _settingsBridge = new GameSettingsBridge(_playerController, _asteroidFieldController,
                                                 _largeSaucerController, _smallSaucerController);

        // Not playing yet
        SetGamePlayState(GamePlayState.NotPlaying);

        // Background asteroid field
        CreateDemoScreen();

        // Running scene stand alone
        if (GetParent() is Window)
        {
            NewGame(GameSettingsPresets.GetSettings(SettingsPresets.Normal));
        }
    }

    public override void _Process(double delta)
    {
        // Ready to spawn new ship
        if (_gamePlayState == GamePlayState.AwaitingNewShip)
        {
            // But we need to wait until the area around the spawn
            // point is free from asteroids, saucers and saucer missiles
            if (!_exclusionZone.HasOverlappingAreas())
            {
                SetGamePlayState(GamePlayState.Playing);
                _exclusionZoneCollisionShape.Disabled = true;
                _playerController.Activate(_shipSpawnPosition);
            }
        }
        else if (_gamePlayState == GamePlayState.NotPlaying)
        {
            // If we are not actively playing then disable the small saucer once it is off screen
            if (_smallSaucerController.IsActive && !_smallSaucerController.IsSaucerActive)
            {
                _smallSaucerController.Deactivate();
            }

            // Create a new demo asteroid field if the current one has become too sparse
            if (_asteroidFieldController.AsteroidCount < _minAsteroidsOnDemoScreen)
            {
                CreateDemoScreen();
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
        _largeSaucerController = GetNode<SaucerController>("LargeSaucerController") ?? throw new NullReferenceException("LargeSaucerController not found");
        _smallSaucerController = GetNode<SaucerController>("SmallSaucerController") ?? throw new NullReferenceException("SmallSaucerController not found");
        _eventHub = (EventHub)FindChild("EventHub") ?? throw new NullReferenceException("EventHub not found");
        _livesController = (LivesController)FindChild("LivesController") ?? throw new NullReferenceException("LivesController not found");
        _scoreController = (ScoreController)FindChild("ScoreController") ?? throw new NullReferenceException("ScoreController not found");
        _powerUpController = (PowerUpController)FindChild("PowerUpController") ?? throw new NullReferenceException("PowerUpController not found");
    }

    private void SetupSceneSignals()
    {
        // Player ship
        _eventHub.LivesDecreased += OnLivesDecreased;
        _eventHub.PlayerExploded += PlayerOnExploded;

        // Asteroids
        _eventHub.AsteroidFieldCleared += AsteroidFieldControllerOnFieldCleared;

        // Power Ups
        _eventHub.PowerUpCollected += OnPowerUpCollected;

        // Window resize
        GetTree().GetRoot().SizeChanged += WindowOnSizeChanged;

        // Callback to allow small saucer to target the player
        _smallSaucerController.TargetCallback = () => _playerController.PlayerPosition;
    }

    private void OnPowerUpCollected(PowerUpType powerUpType)
    {
        _playerController.MultiShot = false;
        _playerController.ShotMode = ShotModeType.Wrap;
        _playerController.PoweredUp = false;

        switch (powerUpType)
        {
            case PowerUpType.MultiShot:
                _playerController.MultiShot = true;
                _playerController.PoweredUp = true;
                break;
            case PowerUpType.ReflectiveShot:
                _playerController.ShotMode = ShotModeType.Reflect;
                _playerController.PoweredUp = true;
                break;
            case PowerUpType.ExtraLife:
                _livesController.AddLife();
                break;
            default:
                break;
        }
    }

    private void CreateDemoScreen()
    {
        var demoSettings = GameSettingsPresets.GetSettings(SettingsPresets.Demo);
        _settingsBridge.Apply(demoSettings, GameSettingsBridge.Fields.Theme);
        Resources.EnableNewSoundFx(false);

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

    private void SetGamePlayState(GamePlayState gamePlayState)
    {
        Logger.I.Debug("Game play state changed to {0}", gamePlayState);
        _gamePlayState = gamePlayState;
    }

    public void NewGame(GameSettings gameSettings)
    {
        // Apply the current configuration
        _settingsBridge.Apply(gameSettings);
        _maxAsteroids = gameSettings.AsteroidsMaxQuantity;

        // Starting lives
        _livesController.Lives = gameSettings.PlayerStartingLives;

        // Reset score to 0
        _scoreController.Score = 0;

        // Threshold for first extra life
        _livesController.ExtraLifeThreshold = gameSettings.PlayerExtraLifeScoreThreshold;

        // Starting quantity of asteroids
        _asteroidsCurrentInitialQuantity = gameSettings.AsteroidsInitialQuantity;

        // Don't need the exclusion zone as we are creating new asteroids
        _exclusionZoneCollisionShape.Disabled = true;

        // Create the new asteroid fields
        _asteroidFieldController.SpawnField(_asteroidsCurrentInitialQuantity,
           new Rect2(_shipSpawnPosition.X - _safeZoneRadius, _shipSpawnPosition.Y - _safeZoneRadius,
                     _safeZoneRadius * 2, _safeZoneRadius * 2),
           true);

        // Saucers
        _largeSaucerController.Deactivate(true);
        if (gameSettings.LargeSaucerEnabled)
        {
            _largeSaucerController.Activate();
        }

        _smallSaucerController.Deactivate(true);
        if (gameSettings.SmallSaucerEnabled)
        {
            _smallSaucerController.Activate();
        }

        // Enable FX audio according to configuration
        Resources.EnableNewSoundFx(gameSettings.SoundEnabled);

        // Start playing the beats sounds
        _beats.Reset();
        _beats.Start();

        // Flag we are ready to spawn a new ship
        SetGamePlayState(GamePlayState.AwaitingNewShip);

        // Flag new game
        EmitSignal(SignalName.NewGameStarted);
    }

    private void SetGameOver()
    {
        // Stop playing the background beats
        _beats.Stop();

        // Not playing anymore
        SetGamePlayState(GamePlayState.NotPlaying);

        // Flag game over
        EmitSignal(SignalName.GameOver);
    }

    // <-- Game state control

    // --> Game events

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
        // Increase the number of asteroids but keep it within permitted range
        _asteroidsCurrentInitialQuantity += _asteroidsNewSheetDelta;
        _asteroidsCurrentInitialQuantity = Mathf.Min(_asteroidsCurrentInitialQuantity, _maxAsteroids);

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
        if (_gamePlayState == GamePlayState.Playing)
        {
            // Hide the player/stop processing
            _playerController.Deactivate();

            // This is safe wrt signal delivery order as there is a gap between "Exploding" and "Exploded"
            SetGamePlayState(GamePlayState.AwaitingNewShip);
            _beats.Start();
        }
    }

    private void OnLivesDecreased(int newLives)
    {
        Logger.I.SignalReceived(this, _livesController, LivesController.SignalName.LivesDecreased);

        // Stop playing the beats sounds
        _beats.Stop();

        // Lose power ups if any
        _playerController.PoweredUp = false;
        _playerController.MultiShot = false;
        _playerController.ShotMode = ShotModeType.Wrap;

        // If we have no lives left then flag it
        if (newLives == 0)
        {
            SetGameOver();
        }
        else
        {
            // Enable the exclusion zone to ensure the new player spawn will be safe
            _exclusionZoneCollisionShape.SetDeferred(CollisionShape2D.PropertyName.Disabled, false);
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
}
