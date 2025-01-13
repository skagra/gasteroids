using System.Diagnostics;
using Godot;

namespace Asteroids;

public partial class EventHub : Node
{
    [Signal]
    public delegate void PlayerExplodingEventHandler(PlayerController playerController);
    [Signal]
    public delegate void PlayerExplodedEventHandler(PlayerController playerController);
    [Signal]
    public delegate void AsteroidCollidedEventHandler(Asteroid asteroid, AsteroidSize size, Node collidedWith);
    [Signal]
    public delegate void AsteroidFieldClearedEventHandler(AsteroidFieldController asteroidFieldController);
    [Signal]
    public delegate void PowerUpCollectedEventHandler(int powerUpType);
    [Signal]
    public delegate void LargeSaucerCollidedEventHandler(Saucer saucer, Node collidedWith);
    [Signal]
    public delegate void SmallSaucerCollidedEventHandler(Saucer saucer, Node collidedWith);
    [Signal]
    public delegate void ScoreIncreasedEventHandler(int newScore);
    [Signal]
    public delegate void LivesIncreasedEventHandler(int newLives);
    [Signal]
    public delegate void LivesDecreasedEventHandler(int newLives);

    [Export]
    private PlayerController _playerController;
    [Export]
    private AsteroidFieldController _asteroidFieldController;
    [Export]
    private SaucerController _largeSaucerController;
    [Export]
    private SaucerController _smallSaucerController;


    public override void _Ready()
    {
        Debug.Assert(_playerController != null);
        Debug.Assert(_asteroidFieldController != null);
        Debug.Assert(_largeSaucerController != null);
        Debug.Assert(_smallSaucerController != null);

        _playerController.Exploded += OnPlayerExploding;
        _playerController.Exploded += OnPlayerExploded;

        _asteroidFieldController.Collided += OnAsteroidCollided;
        _asteroidFieldController.FieldCleared += OnAsteroidFieldCleared;

        _largeSaucerController.Collided += OnLargeSaucerCollided;
        _smallSaucerController.Collided += OnSmallSaucerCollided;
    }

    private void OnSmallSaucerCollided(Saucer saucer, Node collidedWith)
    {
        Logger.I.SignalReceived(this, _smallSaucerController, SaucerController.SignalName.Collided, collidedWith);
        Logger.I.SignalSent(this, SignalName.SmallSaucerCollided, saucer, collidedWith);
        EmitSignal(SignalName.SmallSaucerCollided, saucer, collidedWith);
    }

    private void OnLargeSaucerCollided(Saucer saucer, Node collidedWith)
    {
        Logger.I.SignalReceived(this, _largeSaucerController, SaucerController.SignalName.Collided, collidedWith);
        Logger.I.SignalSent(this, SignalName.LargeSaucerCollided, saucer, collidedWith);
        EmitSignal(SignalName.LargeSaucerCollided, saucer, collidedWith);
    }

    private void OnAsteroidFieldCleared(AsteroidFieldController asteroidFieldController)
    {
        Logger.I.SignalReceived(this, asteroidFieldController, AsteroidFieldController.SignalName.FieldCleared);
        Logger.I.SignalSent(this, SignalName.AsteroidFieldCleared, asteroidFieldController);
        EmitSignal(SignalName.AsteroidFieldCleared, asteroidFieldController);
    }

    private void OnAsteroidCollided(Asteroid asteroid, AsteroidSize size, Node collidedWith)
    {
        Logger.I.SignalReceived(this, _asteroidFieldController, AsteroidFieldController.SignalName.Collided, asteroid, size, collidedWith);
        Logger.I.SignalSent(this, SignalName.AsteroidCollided, asteroid, size, collidedWith);
        EmitSignal(SignalName.AsteroidCollided, asteroid, (int)size, collidedWith);
    }

    private void OnPlayerExploded(PlayerController playerController)
    {
        Logger.I.SignalReceived(this, playerController, PlayerController.SignalName.Exploded);
        Logger.I.SignalSent(this, SignalName.PlayerExploded, playerController);
        EmitSignal(SignalName.PlayerExploded, playerController);
    }

    private void OnPlayerExploding(PlayerController playerController)
    {
        Logger.I.SignalReceived(this, playerController, PlayerController.SignalName.Exploding);
        Logger.I.SignalSent(this, SignalName.PlayerExploding, playerController);
        EmitSignal(SignalName.PlayerExploding, playerController);
    }
}
