using System.Collections.Generic;
using Godot;

namespace Asteroids;

public partial class Scores : Node
{
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
}
