using System;
using System.IO;
using System.Text.Json;
using Godot;

namespace Asteroids;

public static class GameSettingsPersistence
{
    private static JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

    public static GameSettings Load(string userPath)
    {
        GameSettings result = null;

        try
        {
            var jsonString = File.ReadAllText(ProjectSettings.GlobalizePath(userPath));
            result = JsonSerializer.Deserialize<GameSettings>(jsonString);
        }
        catch (Exception e)
        {
            Logger.I.Error("Failed to load save file '{0}'", e.ToString());
        }

        return result;
    }

    public static void Save(GameSettings settings, string userPath)
    {
        try
        {
            var json = JsonSerializer.Serialize(settings, _jsonOptions);
            File.WriteAllText(ProjectSettings.GlobalizePath(userPath), json);
        }
        catch (Exception e)
        {
            Logger.I.Error("Failed to load save file '{0}'", e.ToString());
        }
    }
}