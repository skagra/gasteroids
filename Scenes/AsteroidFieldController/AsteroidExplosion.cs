using Godot;

namespace Asteroids;

public partial class AsteroidExplosion : AnimatedSprite2D
{
    [Signal]
    public delegate void ExplosionCompletedEventHandler(AsteroidExplosion explosion);

    public Vector2 LinearVelocity { get; set; }
    public float AngularVelocity { get; set; }

    public override void _Ready()
    {
        if (GetParent() is Window)
        {
            Position = Screen.Instance.Centre;
        }

        AnimationFinished += AnimationCompleted;

        Play();
    }

    public override void _Process(double delta)
    {
        Position += LinearVelocity * (float)delta;
        Rotation += AngularVelocity * (float)delta;
    }

    // Called back from animation
    private void AnimationCompleted()
    {
        EmitSignal(SignalName.ExplosionCompleted, this);
    }
}
