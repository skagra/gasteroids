using Godot;
using System.Diagnostics;

namespace Asteroids;

public partial class LivesController : Node
{
    [Signal]
    public delegate void LivesIncreasedEventHandler(int newLives);

    [Signal]
    public delegate void LivesDecreasedEventHandler(int newLives);

    [Export]
    private Ui _ui;

    [Export]
    private EventHub _eventHub;

    [Export]
    public int ExtraLifeThreshold { get; set; } = 1000;

    [Export]
    private int _maxLives = 6;

    [Export]
    private int _newGameLives = 3;

    [Export]
    private bool _infiniteLives = false;

    private int _nextExtraLifeThreshold;

    public int Lives
    {
        get => _ui.Lives;
        set => _ui.Lives = value;
    }

    public override void _Ready()
    {
        Debug.Assert(_ui != null);
        Debug.Assert(_eventHub != null);

        _eventHub.ScoreIncreased += OnScoreIncreased;
        _eventHub.PlayerExploding += OnPlayerExploding;

        Reset();
    }

    private void Reset()
    {
        _ui.Lives = _newGameLives;
        _nextExtraLifeThreshold = ExtraLifeThreshold;
    }

    private void OnPlayerExploding(PlayerController playerController)
    {
        if (!_infiniteLives)
        {
            _ui.RemoveLife();
            EmitSignal(SignalName.LivesDecreased, _ui.Lives);
        }
    }

    private void OnScoreIncreased(int newScore)
    {
        if (newScore > _nextExtraLifeThreshold && _ui.Lives < _maxLives)
        {
            _ui.AddLife();
            _nextExtraLifeThreshold += ExtraLifeThreshold;
            EmitSignal(SignalName.LivesIncreased, _ui.Lives);
        }
    }
}
