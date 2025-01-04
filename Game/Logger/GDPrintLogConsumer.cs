using Godot;

namespace Asteroids;

public class GDPrintLogConsumer : ILogConsumer
{
    public void Log(Logger.LogLevel level, string message)
    {
        GD.Print(message);
    }
}