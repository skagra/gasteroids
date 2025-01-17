namespace Asteroids;

public class GameSettings
{
    public GameSettings() { }

    public GameSettings(GameSettings configSettings)
    {
        ConfigurationSet = configSettings.ConfigurationSet;

        SoundEnabled = configSettings.SoundEnabled;

        Theme = configSettings.Theme;

        PowerUpsEnabled = configSettings.PowerUpsEnabled;
        PlayerInfiniteLives = configSettings.PlayerInfiniteLives;
        PlayerStartingLives = configSettings.PlayerStartingLives;
        PlayerMaxLives = configSettings.PlayerMaxLives;
        PlayerExtraLifeScoreThreshold = configSettings.PlayerExtraLifeScoreThreshold;

        ShipAcceleration = configSettings.ShipAcceleration;
        ShipTurnSpeed = configSettings.ShipTurnSpeed;
        ShipLinearDampening = configSettings.ShipLinearDampening;

        PlayerMaxMissiles = configSettings.PlayerMaxMissiles;
        PlayerMissilesSpeed = configSettings.PlayerMissilesSpeed;
        PlayerMissilesLifespan = configSettings.PlayerMissilesLifespan;

        AsteroidsRotationEnabled = configSettings.AsteroidsRotationEnabled;
        AsteroidsInitialQuantity = configSettings.AsteroidsInitialQuantity;
        AsteroidsMaxQuantity = configSettings.AsteroidsMaxQuantity;
        AsteroidsMinSpeed = configSettings.AsteroidsMinSpeed;
        AsteroidsMaxSpeed = configSettings.AsteroidsMaxSpeed;
        AsteroidsGravityEnabled = configSettings.AsteroidsGravityEnabled;
        AsteroidsGravitationalConstant = configSettings.AsteroidsGravitationalConstant;

        LargeSaucerEnabled = configSettings.LargeSaucerEnabled;
        LargeSaucerSpeed = configSettings.LargeSaucerSpeed;
        LargeSaucerSpawnFrequency = configSettings.LargeSaucerSpawnFrequency;
        LargeSaucerMaxMissiles = configSettings.LargeSaucerMaxMissiles;
        LargeSaucerMissilesSpeed = configSettings.LargeSaucerMissilesSpeed;
        LargeSaucerMissilesLifespan = configSettings.LargeSaucerMissilesLifespan;

        SmallSaucerEnabled = configSettings.SmallSaucerEnabled;
        SmallSaucerSpeed = configSettings.SmallSaucerSpeed;
        SmallSaucerSpawnFrequency = configSettings.SmallSaucerSpawnFrequency;
        SmallSaucerMaxMissiles = configSettings.SmallSaucerMaxMissiles;
        SmallSaucerMissilesSpeed = configSettings.SmallSaucerMissilesSpeed;
        SmallSaucerMissilesLifespan = configSettings.SmallSaucerMissilesLifespan;
    }

    // Configuration sets
    public GameSettingsPresets.SettingsPresets ConfigurationSet { get; set; }

    // Theme
    public Resources.Themes Theme { get; set; }

    // Sound
    public bool SoundEnabled { get; set; }

    // Player ships
    public bool PowerUpsEnabled { get; set; }
    public bool PlayerInfiniteLives { get; set; }
    public int PlayerStartingLives { get; set; }
    public int PlayerMaxLives { get; set; }
    public int PlayerExtraLifeScoreThreshold { get; set; }
    public float ShipAcceleration { get; set; }
    public float ShipTurnSpeed { get; set; }
    public float ShipLinearDampening { get; set; }

    // Player missiles
    public int PlayerMaxMissiles { get; set; }
    public float PlayerMissilesSpeed { get; set; }
    public float PlayerMissilesLifespan { get; set; }

    // Asteroids
    public bool AsteroidsRotationEnabled { get; set; }
    public int AsteroidsInitialQuantity { get; set; }
    public int AsteroidsMaxQuantity { get; set; }
    public float AsteroidsMinSpeed { get; set; }
    public float AsteroidsMaxSpeed { get; set; }
    public bool AsteroidsGravityEnabled { get; set; }
    public float AsteroidsGravitationalConstant { get; set; }

    // Saucers
    private static float SaucerSpawnFrequencyToMinTime(float frequency) => 60f / frequency;
    private static float SaucerSpawnFrequencyToMaxTime(float frequency) => 1.5f * 60f / frequency;

    // Large saucer
    public bool LargeSaucerEnabled { get; set; }
    public float LargeSaucerSpeed { get; set; }
    public float LargeSaucerMinSpawnTime { get => SaucerSpawnFrequencyToMinTime(LargeSaucerSpawnFrequency); }
    public float LargeSaucerMaxSpawnTime { get => SaucerSpawnFrequencyToMaxTime(LargeSaucerSpawnFrequency); }
    public float LargeSaucerSpawnFrequency { get; set; }
    public int LargeSaucerMaxMissiles { get; set; }
    public float LargeSaucerMissilesSpeed { get; set; }
    public float LargeSaucerMissilesLifespan { get; set; }

    // Small saucer
    public bool SmallSaucerEnabled { get; set; }
    public float SmallSaucerSpeed { get; set; }
    public float SmallSaucerMinSpawnTime { get => SaucerSpawnFrequencyToMinTime(SmallSaucerSpawnFrequency); }
    public float SmallSaucerMaxSpawnTime { get => SaucerSpawnFrequencyToMaxTime(SmallSaucerSpawnFrequency); }
    public float SmallSaucerSpawnFrequency { get; set; }
    public int SmallSaucerMaxMissiles { get; set; }
    public float SmallSaucerMissilesSpeed { get; set; }
    public float SmallSaucerMissilesLifespan { get; set; }
}
