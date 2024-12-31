using Godot;

namespace Asteroids;

public partial class Explosion : AnimatedSprite2D
{
    [Signal]
    public delegate void ExplosionCompletedEventHandler(Explosion explosion);

    public Vector2 LinearVelocity { get; set; }
    public float AngularVelocity { get; set; }

    public override void _Ready()
    {
        if (GetParent() is Window)
        {
            Position = Screen.Centre;
        }

        AnimationFinished += AnimatedSprite2DOnAnimationFinished;

        Play();
    }

    public override void _Process(double delta)
    {
        Position += LinearVelocity * (float)delta;
        Rotation += AngularVelocity * (float)delta;
    }

    private void AnimatedSprite2DOnAnimationFinished()
    {
        Logger.I.SignalSent(this, SignalName.ExplosionCompleted);
        EmitSignal(SignalName.ExplosionCompleted, this);
    }
}
