
using System;
using System.Collections.Generic;
using Godot;

namespace Asteroids;

public class Logger
{
    private readonly List<ILogConsumer> _logConsumers = new();

    public enum LogLevel { None, Debug, Info, Warning, Error };

    public LogLevel Level { get; set; }

    public static readonly Logger I = new();

    public void AddConsumer(ILogConsumer logConsumer)
    {
        _logConsumers.Add(logConsumer);
    }

    public bool LoggingAt(LogLevel level)
    {
        return level >= Level && Level != LogLevel.None;
    }

    private void Log(LogLevel level, string template, params object[] args)
    {
        if (LoggingAt(level))
        {
            var message = $"{DateTime.Now.ToString("h:mm:ss fff")}:{level}:{string.Format(template, args)}";
            foreach (var consumer in _logConsumers)
            {
                consumer.Log(level, message);
            }
        }
    }

    public void Debug(string template, params object[] args)
    {
        Log(LogLevel.Debug, template, args);
    }

    public void Info(string template, params object[] args)
    {
        Log(LogLevel.Info, template, args);
    }

    public void Warning(string template, params object[] args)
    {
        Log(LogLevel.Warning, template, args);
    }

    public void Error(string template, params object[] args)
    {
        Log(LogLevel.Error, template, args);
    }
}
