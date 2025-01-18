using System;
using Godot;

namespace Asteroids;

public partial class PowerUp : Area2D
{
    [Signal]
    public delegate void CollectedEventHandler(PowerUp powerUp, Area2D collidedWith);

    [Signal]
    public delegate void DoneEventHandler(PowerUp powerUp);

    [Export]
    private float _powerUpDuration = 5f;

    public float _timer;

    private AnimationPlayer _animationPlayer;

    public override void _Ready()
    {
        _animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer") ?? throw new NullReferenceException("AnimationPlayer not found");

        _timer = 0f;

        AreaEntered += Area2DOnEntered;

        _animationPlayer.Play("Pulse");

        if (GetParent() is Window)
        {
            Position = Screen.Centre;
        }
    }

    private void StartDone()
    {
        _animationPlayer.Play("Pop");
    }

    private void Area2DOnEntered(Area2D area)
    {
        if (Identities.IsPlayer(area))
        {
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
