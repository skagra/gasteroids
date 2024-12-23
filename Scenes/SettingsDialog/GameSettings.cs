namespace Asteroids;

public class GameSettings
{
    public GameSettings() { }

    public GameSettings(GameSettings configSettings)
    {
        BackgroundEnabled = configSettings.BackgroundEnabled;
        SoundEnabled = configSettings.SoundEnabled;
        ShipInfiniteLives = configSettings.ShipInfiniteLives;
        ShipStartingCount = configSettings.ShipStartingCount;
        ShipMax = configSettings.ShipMax;
        ShipExtraThreshold = configSettings.ShipExtraThreshold;
        ShipAcceleration = configSettings.ShipAcceleration;
        ShipRotationSpeed = configSettings.ShipRotationSpeed;
        AsteroidsRotationEnabled = configSettings.AsteroidsRotationEnabled;
        AsteroidsStartingQuantity = configSettings.AsteroidsStartingQuantity;
        AsteroidsMaxStartingQuantity = configSettings.AsteroidsMaxStartingQuantity;
        AsteroidsMinSpeed = configSettings.AsteroidsMinSpeed;
        AsteroidsMaxSpeed = configSettings.AsteroidsMaxSpeed;
        MissilesMax = configSettings.MissilesMax;
        MissilesSpeed = configSettings.MissilesSpeed;
        MissilesLifespan = configSettings.MissilesLifespan;
    }

    // Graphics
    public bool BackgroundEnabled { get; set; }

    // Sound
    public bool SoundEnabled { get; set; }

    // Player ships
    public bool ShipInfiniteLives { get; set; }
    public int ShipStartingCount { get; set; }
    public int ShipMax { get; set; }
    public int ShipExtraThreshold { get; set; }
    public float ShipAcceleration { get; set; }
    public float ShipRotationSpeed { get; set; }

    // Asteroids
    public bool AsteroidsRotationEnabled { get; set; }
    public int AsteroidsStartingQuantity { get; set; }
    public int AsteroidsMaxStartingQuantity { get; set; }
    public float AsteroidsMinSpeed { get; set; }
    public float AsteroidsMaxSpeed { get; set; }

    // Player missiles
    public int MissilesMax { get; set; }
    public float MissilesSpeed { get; set; }
    public float MissilesLifespan { get; set; }
}
