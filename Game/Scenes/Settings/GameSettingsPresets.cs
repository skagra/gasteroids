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

        PowerUpsEnabled = false,
        PlayerInfiniteLives = false,
        PlayerStartingLives = 3,
        PlayerMaxLives = 6,
        PlayerExtraLifeScoreThreshold = 10000,

        ShipAcceleration = 550f,
        ShipTurnSpeed = 5.5f,
        ShipLinearDampening = 0.9f,

        PlayerMaxMissiles = 8,
        PlayerMissilesSpeed = 550,
        PlayerMissilesLifespan = 200f,

        AsteroidsRotationEnabled = false,
        AsteroidsInitialQuantity = 4,
        AsteroidsMaxQuantity = 20,
        AsteroidsMinSpeed = 200,
        AsteroidsMaxSpeed = 400,
        AsteroidsGravityEnabled = false,
        AsteroidsGravitationalConstant = 1200,

        LargeSaucerEnabled = true,
        LargeSaucerSpeed = 330,
        LargeSaucerSpawnFrequency = 5f,
        LargeSaucerMaxMissiles = 6,
        LargeSaucerMissilesSpeed = 450,
        LargeSaucerMissilesLifespan = 1.8f,

        SmallSaucerEnabled = true,
        SmallSaucerSpeed = 400,
        SmallSaucerSpawnFrequency = 1.5f,
        SmallSaucerMaxMissiles = 6,
        SmallSaucerMissilesSpeed = 450,
        SmallSaucerMissilesLifespan = 2f
    };

    private static readonly GameSettings EasyGameSettings = new()
    {
        ConfigurationSet = SettingsPresets.Easy,

        Theme = Resources.Themes.Modern,

        SoundEnabled = true,

        PowerUpsEnabled = true,
        PlayerInfiniteLives = false,
        PlayerStartingLives = 3,
        PlayerMaxLives = 6,
        PlayerExtraLifeScoreThreshold = 1000,

        ShipAcceleration = 400f,
        ShipTurnSpeed = 4f,
        ShipLinearDampening = 350f,

        PlayerMaxMissiles = 8,
        PlayerMissilesSpeed = 550,
        PlayerMissilesLifespan = 2f,

        AsteroidsRotationEnabled = true,
        AsteroidsInitialQuantity = 4,
        AsteroidsMaxQuantity = 10,
        AsteroidsMinSpeed = 100,
        AsteroidsMaxSpeed = 250,
        AsteroidsGravityEnabled = false,
        AsteroidsGravitationalConstant = 1200,

        LargeSaucerEnabled = true,
        LargeSaucerSpeed = 200,
        LargeSaucerSpawnFrequency = 3f,
        LargeSaucerMaxMissiles = 4,
        LargeSaucerMissilesSpeed = 200,
        LargeSaucerMissilesLifespan = 1f,

        SmallSaucerEnabled = true,
        SmallSaucerSpeed = 300,
        SmallSaucerSpawnFrequency = 1f,
        SmallSaucerMaxMissiles = 4,
        SmallSaucerMissilesSpeed = 300,
        SmallSaucerMissilesLifespan = 1.5f
    };

    private static readonly GameSettings NormalGameSettings = new()
    {
        ConfigurationSet = SettingsPresets.Normal,

        Theme = Resources.Themes.Modern,

        SoundEnabled = true,

        PowerUpsEnabled = true,
        PlayerInfiniteLives = false,
        PlayerStartingLives = 3,
        PlayerMaxLives = 6,
        PlayerExtraLifeScoreThreshold = 10000,

        ShipAcceleration = 550f,
        ShipTurnSpeed = 5f,
        ShipLinearDampening = 150f,

        PlayerMaxMissiles = 8,
        PlayerMissilesSpeed = 550,
        PlayerMissilesLifespan = 2f,

        AsteroidsRotationEnabled = true,
        AsteroidsInitialQuantity = 4,
        AsteroidsMaxQuantity = 20,
        AsteroidsMinSpeed = 200,
        AsteroidsMaxSpeed = 400,
        AsteroidsGravityEnabled = true,
        AsteroidsGravitationalConstant = 1200,

        LargeSaucerEnabled = true,
        LargeSaucerSpeed = 330,
        LargeSaucerSpawnFrequency = 5f,
        LargeSaucerMaxMissiles = 6,
        LargeSaucerMissilesSpeed = 450,
        LargeSaucerMissilesLifespan = 1.8f,

        SmallSaucerEnabled = true,
        SmallSaucerSpeed = 400,
        SmallSaucerSpawnFrequency = 1.5f,
        SmallSaucerMaxMissiles = 6,
        SmallSaucerMissilesSpeed = 450,
        SmallSaucerMissilesLifespan = 2f
    };

    private static readonly GameSettings HardGameSettings = new()
    {
        ConfigurationSet = SettingsPresets.Hard,

        Theme = Resources.Themes.Modern,

        SoundEnabled = true,

        PowerUpsEnabled = true,
        PlayerInfiniteLives = false,
        PlayerStartingLives = 3,
        PlayerMaxLives = 6,
        PlayerExtraLifeScoreThreshold = 10000,

        ShipAcceleration = 700f,
        ShipTurnSpeed = 7f,
        ShipLinearDampening = 0.5f,

        PlayerMaxMissiles = 8,
        PlayerMissilesSpeed = 550,
        PlayerMissilesLifespan = 100f,

        AsteroidsRotationEnabled = true,
        AsteroidsInitialQuantity = 6,
        AsteroidsMaxQuantity = 20,
        AsteroidsMinSpeed = 300,
        AsteroidsMaxSpeed = 500,
        AsteroidsGravityEnabled = true,
        AsteroidsGravitationalConstant = 1500,

        LargeSaucerEnabled = true,
        LargeSaucerSpeed = 450,
        LargeSaucerSpawnFrequency = 7f,
        LargeSaucerMaxMissiles = 8,
        LargeSaucerMissilesSpeed = 600,
        LargeSaucerMissilesLifespan = 2f,

        SmallSaucerEnabled = true,
        SmallSaucerSpeed = 550,
        SmallSaucerSpawnFrequency = 5f,
        SmallSaucerMaxMissiles = 8,
        SmallSaucerMissilesSpeed = 700,
        SmallSaucerMissilesLifespan = 2f
    };

    private static readonly GameSettings CrazyGameSettings = new()
    {
        ConfigurationSet = SettingsPresets.Crazy,

        Theme = Resources.Themes.Modern,

        SoundEnabled = true,

        PowerUpsEnabled = true,
        PlayerInfiniteLives = false,
        PlayerStartingLives = 3,
        PlayerMaxLives = 6,
        PlayerExtraLifeScoreThreshold = 10000,

        ShipAcceleration = 900,
        ShipTurnSpeed = 8f,
        ShipLinearDampening = 50f,

        PlayerMaxMissiles = 8,
        PlayerMissilesSpeed = 550,
        PlayerMissilesLifespan = 2f,

        AsteroidsRotationEnabled = true,
        AsteroidsInitialQuantity = 6,
        AsteroidsMaxQuantity = 20,
        AsteroidsMinSpeed = 400,
        AsteroidsMaxSpeed = 650,
        AsteroidsGravityEnabled = true,
        AsteroidsGravitationalConstant = 2500,

        LargeSaucerEnabled = true,
        LargeSaucerSpeed = 600,
        LargeSaucerSpawnFrequency = 8f,
        LargeSaucerMaxMissiles = 8,
        LargeSaucerMissilesSpeed = 700,
        LargeSaucerMissilesLifespan = 2f,

        SmallSaucerEnabled = true,
        SmallSaucerSpeed = 750,
        SmallSaucerSpawnFrequency = 6f,
        SmallSaucerMaxMissiles = 8,
        SmallSaucerMissilesSpeed = 800,
        SmallSaucerMissilesLifespan = 2f
    };

    private static readonly GameSettings DemoGameSettings = new()
    {
        ConfigurationSet = SettingsPresets.Classic,

        Theme = Resources.Themes.Classic,

        SoundEnabled = false,

        PowerUpsEnabled = false,
        PlayerInfiniteLives = false,
        PlayerStartingLives = 3,
        PlayerMaxLives = 6,
        PlayerExtraLifeScoreThreshold = 10000,

        ShipAcceleration = 550f,
        ShipTurnSpeed = 5.5f,
        ShipLinearDampening = 0.9f,

        PlayerMaxMissiles = 4,
        PlayerMissilesSpeed = 550,
        PlayerMissilesLifespan = 2f,

        AsteroidsRotationEnabled = true,
        AsteroidsInitialQuantity = 10,
        AsteroidsMaxQuantity = 20,
        AsteroidsMinSpeed = 100,
        AsteroidsMaxSpeed = 400,
        AsteroidsGravityEnabled = true,
        AsteroidsGravitationalConstant = 1200,

        LargeSaucerEnabled = true,
        LargeSaucerSpeed = 330,
        LargeSaucerSpawnFrequency = 7f,
        LargeSaucerMaxMissiles = 6,
        LargeSaucerMissilesSpeed = 450,
        LargeSaucerMissilesLifespan = 1.8f,

        SmallSaucerEnabled = false,
        SmallSaucerSpeed = 400,
        SmallSaucerSpawnFrequency = 4f,
        SmallSaucerMaxMissiles = 6,
        SmallSaucerMissilesSpeed = 450,
        SmallSaucerMissilesLifespan = 2f
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