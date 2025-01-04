using System;
using Godot;

namespace Asteroids;

public partial class FlashingLabel : Label
{
    private AnimationPlayer _animationPlayer;

    public override void _Ready()
    {
        _animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer") ?? throw new NullReferenceException("AnimationPlayer not found");
        Hide();

        if (GetParent() is Window)
        {
            Show();
        }
    }

    public new void Show()
    {
        _animationPlayer.Play("FlashIt");
        base.Show();
    }

    public new void Hide()
    {
        _animationPlayer.Stop();
        base.Hide();
    }
}
