using Godot;

namespace Asteroids;

public partial class Screen : Node
{
    private static Screen _I;

    private static Rect2 _viewportRect;

    public override void _Ready()
    {
        _I = this;
        _viewportRect = GetTree().Root.GetViewport().GetVisibleRect();
    }

    public static Vector2 ClampToViewport(Vector2 position)
    {
        if (position.X > _viewportRect.End.X)
        {
            position.X = _viewportRect.Position.X;
        }
        else if (position.X < _viewportRect.Position.X)
        {
            position.X = _viewportRect.End.X;
        }

        if (position.Y > _viewportRect.End.Y)
        {
            position.Y = _viewportRect.Position.Y;
        }
        else if (position.Y < _viewportRect.Position.Y)
        {
            position.Y = _viewportRect.End.Y;
        }

        return position;
    }

    public static Vector2 Centre
    {
        get => _viewportRect.Size * 0.5f;
    }

    public static float Left { get => _viewportRect.Position.X; }

    public static float Right { get => _viewportRect.End.X; }

    public static float Top { get => _viewportRect.Position.Y; }

    public static float Bottom { get => _viewportRect.End.Y; }

}
