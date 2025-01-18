using System;
using Godot;

namespace Asteroids;

public partial class Splash : CanvasLayer
{
    [Export]
    private AudioStream _splashSound;

    private AudioStreamPlayer _audioStreamPlayer = new();

    private Control _control;

    [Export]
    public Color Modulate
    {
        get => _control.Modulate;
        set => _control.Modulate = value;
    }

    public override void _Ready()
    {
        _audioStreamPlayer.Bus = Resources.AUDIO_BUS_NAME_FX;
        _audioStreamPlayer.Stream = _splashSound ?? throw new NullReferenceException("Splash sound not set");
        AddChild(_audioStreamPlayer);

        _control = (Control)FindChild("Control");
    }

    public void PlaySound()
    {
        _audioStreamPlayer.Play();
    }
}
