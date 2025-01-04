using Godot;

namespace Asteroids;

public partial class PushStart : Label
{
    [Export]
    public float FlashPeriod { get; set; } = 0.5f;

    private float _flashTimer = 0;
    private bool _visible = false;

    public override void _Ready()
    {
        Hide();
    }

    public new void Show()
    {
        _flashTimer = 0;
        _visible = true;
        base.Show();
        this.Enable(true);
    }

    public new void Hide()
    {
        base.Hide();
        this.Enable(false);
    }

    public override void _Process(double delta)
    {
        _flashTimer += (float)delta;
        if (_flashTimer > FlashPeriod)
        {
            _flashTimer = 0;
            if (_visible)
            {
                base.Hide();
            }
            else
            {
                base.Show();
            }
            _visible = !_visible;
        }
    }
}
