using System.Reflection.Metadata.Ecma335;
using Godot;

namespace Asteroids;

public partial class Screen : Node
{
    public static Screen Instance { get; private set; }

    private Rect2 _viewportRect;

    public override void _Ready()
    {
        Instance = this;
        _viewportRect = GetTree().Root.GetViewport().GetVisibleRect();
    }

    public Vector2 ClampToViewport(Vector2 position)
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

    public Vector2 Centre
    {
        get => _viewportRect.Size * 0.5f;
    }

    public float Left { get => _viewportRect.Position.X; }

    public float Right { get => _viewportRect.End.X; }

    public float Top { get => _viewportRect.Position.Y; }

    public float Bottom { get => _viewportRect.End.Y; }

}
