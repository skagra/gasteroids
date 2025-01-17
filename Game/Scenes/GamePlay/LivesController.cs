using Godot;
using System;
using System.Diagnostics;

namespace Asteroids;

public partial class LivesController : Node
{
    [Signal]
    public delegate void LivesIncreasedEventHandler(int newLives);
    [Signal]
    public delegate void LivesDecreasedEventHandler(int newLives);

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
    [Export]
    private AudioStream _extraLifeSound;

    private int _nextExtraLifeThreshold;
    private readonly AudioStreamPlayer2D _extraLifeSoundPlayer = new();

    public Ui UI { get; set; }

    public int Lives
    {
        get => UI.Lives;
        set => UI.Lives = value;
    }

    private bool _fxEnabled = false;
    public void EnableFx(bool enable)
    {
        _fxEnabled = enable;
    }

    public override void _Ready()
    {
        Debug.Assert(_eventHub != null);

        _extraLifeSoundPlayer.Bus = Resources.AUDIO_BUS_NAME_FX;
        _extraLifeSoundPlayer.Stream = _extraLifeSound ?? throw new NullReferenceException("Extra life sound not set");
        AddChild(_extraLifeSoundPlayer);

        _eventHub.ScoreIncreased += OnScoreIncreased;
        _eventHub.PlayerExploding += OnPlayerExploding;
    }

    public void Reset()
    {
        UI.Lives = _newGameLives;
        _nextExtraLifeThreshold = ExtraLifeThreshold;
    }

    private void OnPlayerExploding(PlayerController playerController)
    {
        if (!_infiniteLives)
        {
            UI.Lives--;
            EmitSignal(SignalName.LivesDecreased, UI.Lives);
        }
    }

    public void AddLife(bool mute = false)
    {
        if (!mute && _fxEnabled)
        {
            _extraLifeSoundPlayer.Play();
        }

        UI.AddLife();
        EmitSignal(SignalName.LivesIncreased, UI.Lives);
    }

    private void OnScoreIncreased(int newScore)
    {
        if (newScore > _nextExtraLifeThreshold && UI.Lives < _maxLives)
        {
            AddLife();
            _nextExtraLifeThreshold += ExtraLifeThreshold;
        }
    }
}
