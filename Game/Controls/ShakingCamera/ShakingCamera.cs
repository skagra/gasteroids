using Godot;

namespace Asteroids;

public partial class ShakingCamera : Camera2D
{
    [Export]
    public float MaxOffsetX { get; set; } = 10f;
    [Export]
    public float MaxOffsetY { get; set; } = 10f;
    [Export]
    public float MaxRotation { get; set; } = 1f;

    private bool _isActive = false;

    public override void _Ready()
    {
        IgnoreRotation = false;
        Deactivate();
        if (GetParent() is Window)
        {
            var panel = new PanelContainer();
            AddChild(panel);
            panel.AddChild(new Label
            {
                Text = "Testing",
                HorizontalAlignment = HorizontalAlignment.Center,
            });
            Activate();
        }
    }

    public void Activate()
    {
        this.Enable(true);
        SetPhysicsProcess(true);
        _isActive = true;
    }

    public void Deactivate()
    {
        this.Enable(false);
        Offset = Vector2.Zero;
        Rotation = 0f;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_isActive)
        {
            Rotation = MaxRotation * (float)GD.RandRange(-1.0f, 1.0f);
            Offset = new Vector2(MaxOffsetX * (float)GD.RandRange(-1.0f, 1.0f), MaxOffsetY * (float)GD.RandRange(-1.0f, 1.0f));
        }
    }
}
