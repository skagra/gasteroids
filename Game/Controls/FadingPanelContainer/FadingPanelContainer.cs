using System;
using Godot;

namespace Asteroids;

public partial class FadingPanelContainer : PanelContainer
{
    private readonly Color _opaqueColor = new(1, 1, 1, 0.75f);
    private readonly Color _translucentColor = new(1, 1, 1, 0);

    private AnimationPlayer _animationPlayer;

    public float SpeedScale
    {
        get => _animationPlayer.SpeedScale;
        set => _animationPlayer.SpeedScale = value;
    }

    public override void _Ready()
    {
        _animationPlayer = (AnimationPlayer)FindChild("AnimationPlayer") ?? throw new NullReferenceException("AnimationPlayer not found");

        if (GetParent() is Window)
        {
            AddChild(new Label
            {
                Text = "Testing",
                HorizontalAlignment = HorizontalAlignment.Center,
            });
            Show();
        }
    }

    public void Show(bool immediate = false)
    {
        if (!immediate)
        {
            Modulate = _translucentColor;
            _animationPlayer.Play("FadeIn");
        }
        else
        {
            Modulate = _opaqueColor;
        }
        base.Show();
    }

    public void Hide(bool immediate = false)
    {
        if (!immediate)
        {
            _animationPlayer.Play("FadeOut");
        }
        else
        {
            Modulate = _translucentColor;
            base.Hide();
        }
    }
}
