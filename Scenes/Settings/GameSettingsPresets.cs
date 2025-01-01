using System.Collections.Generic;

namespace Asteroids;

public static class GameSettingsPresets
{
    public enum SettingsPresets
    {
        Classic = 0,
        Easy = 1,
        Normal = 2,
        Hard = 3,
        Crazy = 4,
        Demo = 5
    }

    private static readonly GameSettings ClassicGameSettings = new()
    {
        ConfigurationSet = SettingsPresets.Classic,

        Theme = Resources.Themes.Classic,

        SoundEnabled = true,

        PlayerInfiniteLives = false,
        PlayerStartingLives = 1,
        PlayerMaxLives = 6,
        PlayerExtraLifeScoreThreshold = 10000,

        ShipAcceleration = 300f,
        ShipTurnSpeed = 5f,
        ShipLinearDampening = 2f,

        PlayerMaxMissiles = 8,
        PlayerMissilesSpeed = 200,
        PlayerMissilesLifespan = 1.5f,

        AsteroidsRotationEnabled = true,
        AsteroidsInitialQuantity = 4,
        AsteroidsMaxQuantity = 20,
        AsteroidsMinSpeed = 100,
        AsteroidsMaxSpeed = 150,
        AsteroidsGravityEnabled = true,
        AsteroidsGravitationalConstant = 1500,

        LargeSaucerEnabled = true,
        LargeSaucerSpeed = 100,
        LargeSaucerSpawnFrequency = 20,
        LargeSaucerMaxMissiles = 6,
        LargeSaucerMissilesSpeed = 100,
        LargeSaucerMissilesLifespan = 5,

        SmallSaucerEnabled = true,
        SmallSaucerSpeed = 150,
        SmallSaucerSpawnFrequency = 10,
        SmallSaucerMaxMissiles = 6,
        SmallSaucerMissilesSpeed = 100,
        SmallSaucerMissilesLifespan = 5
    };

    private static readonly GameSettings EasyGameSettings = new()
    {
        ConfigurationSet = SettingsPresets.Easy,

        Theme = Resources.Themes.Modern,

        SoundEnabled = true,

        PlayerInfiniteLives = false,
        PlayerStartingLives = 2,
        PlayerMaxLives = 6,
        PlayerExtraLifeScoreThreshold = 10000,

        ShipAcceleration = 300f,
        ShipTurnSpeed = 5f,
        ShipLinearDampening = 2f,

        PlayerMaxMissiles = 8,
        PlayerMissilesSpeed = 200,
        PlayerMissilesLifespan = 1.5f,

        AsteroidsRotationEnabled = true,
        AsteroidsInitialQuantity = 4,
        AsteroidsMaxQuantity = 20,
        AsteroidsMinSpeed = 100,
        AsteroidsMaxSpeed = 150,
        AsteroidsGravityEnabled = true,
        AsteroidsGravitationalConstant = 1500,

        LargeSaucerEnabled = true,
        LargeSaucerSpeed = 100,
        LargeSaucerSpawnFrequency = 20,
        LargeSaucerMaxMissiles = 6,
        LargeSaucerMissilesSpeed = 100,
        LargeSaucerMissilesLifespan = 5,

        SmallSaucerEnabled = true,
        SmallSaucerSpeed = 150,
        SmallSaucerSpawnFrequency = 10,
        SmallSaucerMaxMissiles = 6,
        SmallSaucerMissilesSpeed = 100,
        SmallSaucerMissilesLifespan = 5
    };

    private static readonly GameSettings NormalGameSettings = new()
    {
        ConfigurationSet = SettingsPresets.Normal,

        Theme = Resources.Themes.Modern,

        SoundEnabled = true,

        PlayerInfiniteLives = false,
        PlayerStartingLives = 3,
        PlayerMaxLives = 6,
        PlayerExtraLifeScoreThreshold = 10000,

        ShipAcceleration = 300f,
        ShipTurnSpeed = 5f,
        ShipLinearDampening = 2f,

        PlayerMaxMissiles = 8,
        PlayerMissilesSpeed = 200,
        PlayerMissilesLifespan = 1.5f,

        AsteroidsRotationEnabled = true,
        AsteroidsInitialQuantity = 4,
        AsteroidsMaxQuantity = 20,
        AsteroidsMinSpeed = 100,
        AsteroidsMaxSpeed = 150,
        AsteroidsGravityEnabled = true,
        AsteroidsGravitationalConstant = 1500,

        LargeSaucerEnabled = true,
        LargeSaucerSpeed = 100,
        LargeSaucerSpawnFrequency = 20,
        LargeSaucerMaxMissiles = 6,
        LargeSaucerMissilesSpeed = 100,
        LargeSaucerMissilesLifespan = 5,

        SmallSaucerEnabled = true,
        SmallSaucerSpeed = 150,
        SmallSaucerSpawnFrequency = 10,
        SmallSaucerMaxMissiles = 6,
        SmallSaucerMissilesSpeed = 100,
        SmallSaucerMissilesLifespan = 5
    };

    private static readonly GameSettings HardGameSettings = new()
    {
        ConfigurationSet = SettingsPresets.Hard,

        Theme = Resources.Themes.Modern,

        SoundEnabled = true,

        PlayerInfiniteLives = false,
        PlayerStartingLives = 4,
        PlayerMaxLives = 6,
        PlayerExtraLifeScoreThreshold = 10000,

        ShipAcceleration = 300f,
        ShipTurnSpeed = 5f,
        ShipLinearDampening = 2f,

        PlayerMaxMissiles = 8,
        PlayerMissilesSpeed = 200,
        PlayerMissilesLifespan = 1.5f,

        AsteroidsRotationEnabled = true,
        AsteroidsInitialQuantity = 4,
        AsteroidsMaxQuantity = 20,
        AsteroidsMinSpeed = 100,
        AsteroidsMaxSpeed = 150,
        AsteroidsGravityEnabled = true,
        AsteroidsGravitationalConstant = 1500,

        LargeSaucerEnabled = true,
        LargeSaucerSpeed = 100,
        LargeSaucerSpawnFrequency = 20,
        LargeSaucerMaxMissiles = 6,
        LargeSaucerMissilesSpeed = 100,
        LargeSaucerMissilesLifespan = 5,

        SmallSaucerEnabled = true,
        SmallSaucerSpeed = 150,
        SmallSaucerSpawnFrequency = 10,
        SmallSaucerMaxMissiles = 6,
        SmallSaucerMissilesSpeed = 100,
        SmallSaucerMissilesLifespan = 5
    };

    private static readonly GameSettings CrazyGameSettings = new()
    {
        ConfigurationSet = SettingsPresets.Crazy,

        Theme = Resources.Themes.Modern,

        SoundEnabled = true,

        PlayerInfiniteLives = false,
        PlayerStartingLives = 5,
        PlayerMaxLives = 6,
        PlayerExtraLifeScoreThreshold = 10000,

        ShipAcceleration = 300f,
        ShipTurnSpeed = 5f,
        ShipLinearDampening = 2f,

        PlayerMaxMissiles = 8,
        PlayerMissilesSpeed = 200,
        PlayerMissilesLifespan = 1.5f,

        AsteroidsRotationEnabled = true,
        AsteroidsInitialQuantity = 4,
        AsteroidsMaxQuantity = 20,
        AsteroidsMinSpeed = 100,
        AsteroidsMaxSpeed = 150,
        AsteroidsGravityEnabled = true,
        AsteroidsGravitationalConstant = 1500,

        LargeSaucerEnabled = true,
        LargeSaucerSpeed = 100,
        LargeSaucerSpawnFrequency = 20,
        LargeSaucerMaxMissiles = 6,
        LargeSaucerMissilesSpeed = 100,
        LargeSaucerMissilesLifespan = 5,

        SmallSaucerEnabled = true,
        SmallSaucerSpeed = 150,
        SmallSaucerSpawnFrequency = 10,
        SmallSaucerMaxMissiles = 6,
        SmallSaucerMissilesSpeed = 100,
        SmallSaucerMissilesLifespan = 5
    };

    private static readonly GameSettings DemoGameSettings = new()
    {
        ConfigurationSet = SettingsPresets.Classic,

        Theme = Resources.Themes.Classic,

        SoundEnabled = false,

        PlayerInfiniteLives = false,
        PlayerStartingLives = 1,
        PlayerMaxLives = 6,
        PlayerExtraLifeScoreThreshold = 10000,

        ShipAcceleration = 300f,
        ShipTurnSpeed = 5f,
        ShipLinearDampening = 2f,

        PlayerMaxMissiles = 8,
        PlayerMissilesSpeed = 200,
        PlayerMissilesLifespan = 1.5f,

        AsteroidsRotationEnabled = true,
        AsteroidsInitialQuantity = 10,
        AsteroidsMaxQuantity = 20,
        AsteroidsMinSpeed = 150,
        AsteroidsMaxSpeed = 250,
        AsteroidsGravityEnabled = true,
        AsteroidsGravitationalConstant = 1500,

        LargeSaucerEnabled = true,
        LargeSaucerSpeed = 300,
        LargeSaucerSpawnFrequency = 10,
        LargeSaucerMaxMissiles = 6,
        LargeSaucerMissilesSpeed = 300,
        LargeSaucerMissilesLifespan = 3,

        SmallSaucerEnabled = true,
        SmallSaucerSpeed = 500,
        SmallSaucerSpawnFrequency = 5,
        SmallSaucerMaxMissiles = 6,
        SmallSaucerMissilesSpeed = 400,
        SmallSaucerMissilesLifespan = 3
    };

    private static readonly Dictionary<SettingsPresets, GameSettings> _presets = new()
    {
        { SettingsPresets.Classic, ClassicGameSettings },
        { SettingsPresets.Easy, EasyGameSettings },
        { SettingsPresets.Normal, NormalGameSettings },
        { SettingsPresets.Hard, HardGameSettings },
        { SettingsPresets.Crazy, CrazyGameSettings },
        { SettingsPresets.Demo, DemoGameSettings }
    };

    public static GameSettings GetSettings(SettingsPresets presets)
    {
        return _presets[presets];
    }
}