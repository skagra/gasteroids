using Godot;

namespace Asteroids;

public partial class Splash : CanvasLayer
{
    [Signal]
    public delegate void SplashDoneEventHandler();

    [Export]
    private AudioStream _splashSound;

    private AudioStreamPlayer2D _audioStreamPlayer = new();

    private AnimationPlayer _animationPlayer;

    public override void _Ready()
    {
        _audioStreamPlayer.Bus = Resources.AUDIO_BUS_NAME_FX;
        _audioStreamPlayer.Stream = _splashSound;
        AddChild(_audioStreamPlayer);

        _animationPlayer = (AnimationPlayer)FindChild("AnimationPlayer");
        _animationPlayer.AnimationFinished += OnAnimationFinished;
    }

    private void OnAnimationFinished(StringName animName)
    {
        if (Visible)
        {
            EmitSignal(SignalName.SplashDone);
        }
    }

    public void Activate()
    {
        _audioStreamPlayer.Play();
        _animationPlayer.Play("FadeOut");
    }
}
