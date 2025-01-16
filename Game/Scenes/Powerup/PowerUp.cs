using System;
using System.Diagnostics;
using Godot;

namespace Asteroids;

public partial class PowerUp : Area2D
{
    [Signal]
    public delegate void CollectedEventHandler(PowerUp powerUp, Area2D collidedWith);

    [Signal]
    public delegate void DoneEventHandler(PowerUp powerUp);

    [Export]
    private AudioStream _collectedSound;

    [Export]
    private float _powerUpDuration = 5f;

    public float _timer;

    private AnimationPlayer _animationPlayer;

    private AudioStreamPlayer _audioStreamPlayer;

    public override void _Ready()
    {
        Debug.Assert(_collectedSound != null);
        _animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer") ?? throw new NullReferenceException("AnimationPlayer not found");
        _audioStreamPlayer = new AudioStreamPlayer();
        AddChild(_audioStreamPlayer);

        _timer = 0f;

        AreaEntered += Area2DOnEntered;

        _animationPlayer.Play("Pulse");

        if (GetParent() is Window)
        {
            Position = Screen.Centre;
        }
    }

    private bool _fxEnabled = false;
    public void EnableFx(bool enable)
    {
        _fxEnabled = enable;
    }

    private void StartDone()
    {
        _animationPlayer.Play("Pop");
    }

    private void Area2DOnEntered(Area2D area)
    {
        if (Identities.IsPlayer(area))
        {
            _audioStreamPlayer.Stream = _collectedSound;
            if (_fxEnabled)
            {
                _audioStreamPlayer.Play();
            }
            StartDone();
            EmitSignal(SignalName.Collected, this, area);
        }
    }

    public void End()
    {
        EmitSignal(SignalName.Done, this);
    }

    public override void _Process(double delta)
    {
        _timer += (float)delta;
        if (!Screen.IsOnScreen(Position) || _timer > _powerUpDuration)
        {
            StartDone();
        }
    }
}
