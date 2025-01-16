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
    private Lives _lives;
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

    public int Lives
    {
        get => _lives.Value;
        set => _lives.Value = value;
    }

    private bool _fxEnabled = false;
    public void EnableFx(bool enable)
    {
        _fxEnabled = enable;
    }

    public override void _Ready()
    {
        Debug.Assert(_lives != null);
        Debug.Assert(_eventHub != null);

        _extraLifeSoundPlayer.Bus = Resources.AUDIO_BUS_NAME_FX;
        _extraLifeSoundPlayer.Stream = _extraLifeSound ?? throw new NullReferenceException("Extra life sound not set");
        AddChild(_extraLifeSoundPlayer);

        _eventHub.ScoreIncreased += OnScoreIncreased;
        _eventHub.PlayerExploding += OnPlayerExploding;

        Reset();
    }

    private void Reset()
    {
        _lives.Value = _newGameLives;
        _nextExtraLifeThreshold = ExtraLifeThreshold;
    }

    private void OnPlayerExploding(PlayerController playerController)
    {
        if (!_infiniteLives)
        {
            _lives.RemoveLife();
            EmitSignal(SignalName.LivesDecreased, _lives.Value);
        }
    }

    public void AddLife(bool mute = false)
    {
        if (!mute && _fxEnabled)
        {
            _extraLifeSoundPlayer.Play();
        }

        _lives.AddLife();
        EmitSignal(SignalName.LivesIncreased, _lives.Value);
    }

    private void OnScoreIncreased(int newScore)
    {
        if (newScore > _nextExtraLifeThreshold && _lives.Value < _maxLives)
        {
            AddLife();
            _nextExtraLifeThreshold += ExtraLifeThreshold;
        }
    }
}
