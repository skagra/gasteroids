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

    private bool _terminating = false;

    public override void _Ready()
    {
        _terminating = false;
        _audioStreamPlayer.Bus = Resources.AUDIO_BUS_NAME_FX;
        _audioStreamPlayer.Stream = _splashSound;
        AddChild(_audioStreamPlayer);

        _animationPlayer = (AnimationPlayer)FindChild("AnimationPlayer");
        _animationPlayer.AnimationFinished += OnAnimationFinished;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!_terminating && Input.IsAnythingPressed())
        {
            _terminating = true;
            _animationPlayer.SpeedScale = 10f;
        }
    }

    private void OnAnimationFinished(StringName animName)
    {
        if (Visible)
        {
            EmitSignal(SignalName.SplashDone);
        }
    }

    public void Activate(bool mute = false)
    {
        _animationPlayer.SpeedScale = 1f;
        if (!mute)
        {
            _audioStreamPlayer.Play();
        }
        _animationPlayer.Play("FadeOut");
    }
}
