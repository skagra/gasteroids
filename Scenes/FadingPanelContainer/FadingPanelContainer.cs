using Godot;

namespace Asteroids;

public partial class FadingPanelContainer : PanelContainer
{
    private AnimationPlayer _animationPlayer;

    public override void _Ready()
    {
        _animationPlayer = (AnimationPlayer)FindChild("AnimationPlayer");
        base._Ready();
    }

    public new void Show()
    {
        base.Show();
        _animationPlayer.Play("FadeIn");
    }
}
