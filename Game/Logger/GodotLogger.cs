using Godot;
using static Asteroids.Logger;

namespace Asteroids;

public partial class GodotLogger : Node
{
    [Export]
    public LogLevel Level
    {
        get => Logger.I.Level;
        set => Logger.I.Level = value;
    }

    public override void _EnterTree()
    {
        Logger.I.AddConsumer(new GDPrintLogConsumer());
    }
}
