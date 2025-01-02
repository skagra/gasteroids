using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Godot;

namespace Asteroids;

public static class HighScorePersistence
{
    private static readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

    public static List<ScoreDetails> Load(string userPath)
    {
        List<ScoreDetails> result = null;

        var globalizedPath = ProjectSettings.GlobalizePath(userPath);
        if (File.Exists(globalizedPath))
        {
            try
            {
                var jsonString = File.ReadAllText(globalizedPath);
                result = JsonSerializer.Deserialize<List<ScoreDetails>>(jsonString);
            }
            catch (Exception e)
            {
                Logger.I.Error("Failed to load save file '{0}'", e.ToString());
            }
        }

        return result;
    }

    public static void Save(List<ScoreDetails> scores, string userPath)
    {
        try
        {
            var json = JsonSerializer.Serialize(scores, _jsonOptions);
            File.WriteAllText(ProjectSettings.GlobalizePath(userPath), json);
        }
        catch (Exception e)
        {
            Logger.I.Error("Failed to load save file '{0}'", e.ToString());
        }
    }
}