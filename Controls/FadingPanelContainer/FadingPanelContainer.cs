using Godot;

namespace Asteroids;

public partial class FadingPanelContainer : PanelContainer
{
    private readonly Color _opaqueColor = new(1, 1, 1, 0.75f);

    private AnimationPlayer _animationPlayer;

    public override void _Ready()
    {
        _animationPlayer = (AnimationPlayer)FindChild("AnimationPlayer");
    }

    public void Show(bool instant = false)
    {
        if (!instant)
        {
            _animationPlayer.Play("FadeIn");
        }
        else
        {
            Modulate = _opaqueColor;
        }
        base.Show();
    }

    public void Hide(bool instant = false)
    {
        if (!instant)
        {
            _animationPlayer.Play("FadeOut");
        }
        else
        {
            base.Hide();
        }
    }
}
