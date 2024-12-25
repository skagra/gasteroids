using static Asteroids.Logger;

namespace Asteroids;

public interface ILogConsumer
{
    void Log(LogLevel level, string message);
}