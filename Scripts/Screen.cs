using Godot;

namespace Asteroids;

public partial class Screen : Node
{
    public static Screen Instance { get; private set; }
    public Viewport _viewport;

    public override void _Ready()
    {
        Instance = this;
        _viewport = GetViewport();
    }

    public Vector2 ClampToViewport(Vector2 position)
    {
        var viewportRect = _viewport.GetVisibleRect();

        if (position.X > viewportRect.End.X)
        {
            position.X = viewportRect.Position.X;
        }
        else if (position.X < viewportRect.Position.X)
        {
            position.X = viewportRect.End.X;
        }

        if (position.Y > viewportRect.End.Y)
        {
            position.Y = viewportRect.Position.Y;
        }
        else if (position.Y < viewportRect.Position.Y)
        {
            position.Y = viewportRect.End.Y;
        }

        return position;
    }

    public Vector2 GetCentre()
    {
        return _viewport.GetVisibleRect().Size * 0.5f;
    }
}
