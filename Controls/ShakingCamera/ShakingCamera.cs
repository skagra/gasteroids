using Godot;

public partial class ShakingCamera : Camera2D
{
    [Export]
    public float Decay { get; set; }
    [Export]
    public float MaxOffsetX { get; set; }
    [Export]
    public float MaxOffsetY { get; set; }
    [Export]
    public float MaxRotation { get; set; }

    private bool _isActive = false;

    public override void _Ready()
    {
        IgnoreRotation = false;
    }

    public void Activate()
    {
        _isActive = true;
    }

    public void Deactivate()
    {
        _isActive = false;
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
