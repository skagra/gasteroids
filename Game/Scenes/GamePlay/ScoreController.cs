using Godot;
using System.Collections.Generic;
using System.Diagnostics;

namespace Asteroids;

public partial class ScoreController : Node
{
    [Signal]
    public delegate void ScoreIncreasedEventHandler(int newScore);

    // Inspector configuration values
    [Export]
    public int AsteroidLarge { get; private set; } = 20;
    [Export]
    public int AsteroidMedium { get; private set; } = 50;
    [Export]
    public int AsteroidSmall { get; private set; } = 100;
    [Export]
    public int SaucerLarge { get; private set; } = 200;
    [Export]
    public int SaucerSmall { get; private set; } = 1000;

    public Ui UI { get; set; }

    [Export]
    private EventHub _eventHub;

    public int Score
    {
        get => UI.Score;
        set => UI.Score = value;
    }

    // Score table
    private readonly Dictionary<AsteroidSize, int> _asteroidScores = new();

    public int AsteroidScore(AsteroidSize size) => _asteroidScores[size];

    public override void _EnterTree()
    {
        // Set up score table
        _asteroidScores[AsteroidSize.Large] = AsteroidLarge;
        _asteroidScores[AsteroidSize.Medium] = AsteroidMedium;
        _asteroidScores[AsteroidSize.Small] = AsteroidSmall;
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Debug.Assert(_eventHub != null);

        _eventHub.AsteroidCollided += OnAsteroidCollided;
        _eventHub.SmallSaucerCollided += OnSmallSaucerCollided;
        _eventHub.LargeSaucerCollided += OnLargeSaucerCollided;
    }

    private void OnLargeSaucerCollided(Saucer saucer, Node collidedWith)
    {
        Logger.I.SignalReceived(this, saucer, Saucer.SignalName.Collided, collidedWith, "LARGE");

        if (Identities.IsPlayerMissile(collidedWith))
        {
            IncreaseScore(SaucerLarge);
        }
    }

    private void OnSmallSaucerCollided(Saucer saucer, Node collidedWith)
    {
        Logger.I.SignalReceived(this, saucer, Saucer.SignalName.Collided, collidedWith, "SMALL");

        if (Identities.IsPlayerMissile(collidedWith))
        {
            IncreaseScore(SaucerSmall);
        }
    }

    private void OnAsteroidCollided(Asteroid asteroid, AsteroidSize size, Node collidedWith)
    {
        Logger.I.SignalReceived(this, asteroid, AsteroidFieldController.SignalName.Collided, size, collidedWith);

        if (Identities.IsPlayerMissile(collidedWith))
        {
            IncreaseScore(AsteroidScore(size));
        }
    }

    private void IncreaseScore(int increase)
    {
        UI.Score += increase;
        EmitSignal(SignalName.ScoreIncreased, UI.Score);
    }
}
