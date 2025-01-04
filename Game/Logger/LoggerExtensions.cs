using Godot;

namespace Asteroids;

public static class LoggerExtensions
{
    public static void SignalSent(this Logger logger, Node origin, string signalName, params object[] args)
    {
        if (logger.LoggingAt(Logger.LogLevel.Debug))
        {
            TransformNodesToName(args);
            logger.Debug($"Signal Sent: {origin.Name}: ^@{signalName}({string.Join(", ", args)})");
        }
    }

    public static void SignalReceived(this Logger logger, Node recipient, Node sender, string signalName, params object[] args)
    {
        if (logger.LoggingAt(Logger.LogLevel.Debug))
        {
            TransformNodesToName(args);
            logger.Debug($"Signal Received: {recipient.Name}: <-{sender.Name}@{signalName}({string.Join(", ", args)})");
        }
    }

    private static void TransformNodesToName(object[] objects)
    {
        var len = objects.Length;
        for (var i = 0; i < len; i++)
        {
            if (objects[i] is Node node)
            {
                objects[i] = node.Name;
            }
        }
    }
}