using Godot;
using System;

namespace Asteroids;

public partial class Screen : Node
{
    public static Screen Instance { get; private set; }

    public override void _Ready()
    {
        Instance = this;
    }

    public Vector2 ClampToViewport(Vector2 position, Viewport viewport)
    {
        var viewportRect = viewport.GetVisibleRect();

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
}
