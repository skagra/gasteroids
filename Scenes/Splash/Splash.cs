using Godot;

public partial class Splash : CanvasLayer
{
    [Signal]
    public delegate void SplashDoneEventHandler();

    private AnimationPlayer _animationPlayer;

    public override void _Ready()
    {
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
        _animationPlayer.Play("FadeOut");
    }
}
