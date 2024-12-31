using Godot;

namespace Asteroids;

public class GameSettingsBridge
{
    private readonly PlayerController _playerController;
    private readonly AsteroidFieldController _asteroidFieldController;
    private readonly SaucerController _largeSaucerController;
    private readonly SaucerController _smallSaucerController;

    public GameSettingsBridge(PlayerController playerController,
                     AsteroidFieldController asteroidFieldController,
                     SaucerController largeSaucerController,
                     SaucerController smallSaucerController)
    {
        _playerController = playerController;
        _asteroidFieldController = asteroidFieldController;
        _largeSaucerController = largeSaucerController;
        _smallSaucerController = smallSaucerController;
    }

    private GameSettingsPresets.SettingsPresets _configurationSet;
    public GameSettingsPresets.SettingsPresets ConfigurationSet
    {
        get => _configurationSet;
        set
        {
            _configurationSet = value;
        }
    }

    private Resources.Themes _theme;

    private Resources.Themes Theme
    {
        get => _theme;
        set
        {
            _theme = value;
            Resources.SwitchTheme(value);
        }
    }

    private bool _soundEnabled = true;
    public bool SoundEnabled
    {
        set
        {
            // Prevent playing any new fx sounds
            _asteroidFieldController.EnableFx(value);
            _playerController.EnableFx(value);
            _smallSaucerController.EnableFx(value);
            _largeSaucerController.EnableFx(value);
            // Mute the audio bus - stopping current sounds
            AudioServer.SetBusMute(AudioServer.GetBusIndex(Resources.AUDIO_BUS_NAME_FX),
                                   !value);
            _soundEnabled = value;

            GD.Print($"Setting _soundEnabled={_soundEnabled}");
        }
        get => _soundEnabled;
    }

    public bool PlayerInfiniteLives { get; set; }

    public int PlayerStartingLives { get; set; }

    public int PlayerMaxLives { get; set; }

    public int PlayerExtraLifeScoreThreshold { get; set; }

    public float ShipAcceleration
    {
        get => ShipAcceleration = _playerController.PlayerThrustForce;
        set => _playerController.PlayerThrustForce = value;
    }
    public float ShipTurnSpeed
    {
        get => ShipTurnSpeed = _playerController.PlayerRotationSpeed;
        set => _playerController.PlayerRotationSpeed = value;
    }

    public float ShipLinearDampening
    {
        get => _playerController.LinearDampening;
        set => _playerController.LinearDampening = value;
    }

    public int PlayerMissilesMax
    {
        get => _playerController.MissileCount;
        set => _playerController.MissileCount = value;
    }

    public float PlayerMissilesSpeed
    {
        get => _playerController.MissileSpeed;
        set => _playerController.MissileSpeed = value;
    }

    public float PlayerMissilesLifespan
    {
        get => _playerController.MissileDuration;
        set => _playerController.MissileDuration = value;
    }

    public bool AsteroidsRotationEnabled
    {
        get => _asteroidFieldController.IsRotationEnabled;
        set => _asteroidFieldController.IsRotationEnabled = value;
    }

    public int AsteroidsInitialQuantity { get; set; }

    public int AsteroidsMaxQuantity { get; set; }

    public float AsteroidsMinSpeed
    {
        get => _asteroidFieldController.MinSpeed;
        set => _asteroidFieldController.MinSpeed = value;
    }

    public float AsteroidsMaxSpeed
    {
        get => _asteroidFieldController.MaxSpeed;
        set => _asteroidFieldController.MaxSpeed = value;
    }

    public bool AsteroidsGravityEnabled
    {
        get => _playerController.PlayerGravitationalPullCallback != null;
        set => _playerController.PlayerGravitationalPullCallback = value ? _asteroidFieldController.GetGravitationalVector : null;
    }

    public float AsteroidsGravitationalConstant
    {
        get => _asteroidFieldController.GravitationalMultiplier;
        set => _asteroidFieldController.GravitationalMultiplier = value;
    }

    public bool LargeSaucerEnabled { get; set; }

    public float LargeSaucerSpeed
    {
        get => _largeSaucerController.Speed;
        set => _largeSaucerController.Speed = value;
    }

    // TODO
    // LargeSaucerSpawnFrequency 
    // LargeSaucerMaximumMissiles 
    // LargeSaucerMissilesSpeed 
    // LargeSaucerMissilesLifespan

    public bool SmallSaucerEnabled { get; set; }

    public float SmallSaucerSpeed
    {
        get => _smallSaucerController.Speed;
        set => _smallSaucerController.Speed = value;
    }

    // TODO
    // SmallSaucerSpawnFrequency 
    // SmallSaucerMaxMissiles 
    // SmallSaucerMissilesSpeed 
    // SmallSaucerMissilesLifespan 

    public GameSettings GameSettings
    {
        get
        {
            return new GameSettings
            {
                ConfigurationSet = ConfigurationSet,

                Theme = Theme,

                SoundEnabled = SoundEnabled,

                PlayerInfiniteLives = PlayerInfiniteLives,
                PlayerStartingLives = PlayerStartingLives,
                PlayerMaxLives = PlayerMaxLives,
                PlayerExtraLifeScoreThreshold = PlayerExtraLifeScoreThreshold,

                ShipAcceleration = ShipAcceleration,
                ShipTurnSpeed = _playerController.PlayerRotationSpeed,
                ShipLinearDampening = _playerController.LinearDampening,

                AsteroidsRotationEnabled = _asteroidFieldController.IsRotationEnabled,
                AsteroidsInitialQuantity = AsteroidsInitialQuantity,
                AsteroidsMaxQuantity = AsteroidsMaxQuantity,
                AsteroidsMinSpeed = _asteroidFieldController.MinSpeed,
                AsteroidsMaxSpeed = _asteroidFieldController.MaxSpeed,
                AsteroidsGravityEnabled = _playerController.PlayerGravitationalPullCallback != null,
                AsteroidsGravitationalConstant = _asteroidFieldController.GravitationalMultiplier,

                PlayerMaxMissiles = _playerController.MissileCount,
                PlayerMissilesSpeed = _playerController.MissileSpeed,
                PlayerMissilesLifespan = _playerController.MissileDuration
            };
        }

        set
        {
            ConfigurationSet = value.ConfigurationSet;

            Theme = value.Theme;

            SoundEnabled = value.SoundEnabled;

            PlayerInfiniteLives = value.PlayerInfiniteLives;
            PlayerStartingLives = value.PlayerStartingLives;
            PlayerMaxLives = value.PlayerMaxLives;
            PlayerExtraLifeScoreThreshold = value.PlayerExtraLifeScoreThreshold;

            ShipAcceleration = value.ShipAcceleration;
            _playerController.PlayerRotationSpeed = value.ShipTurnSpeed;
            _playerController.LinearDampening = value.ShipLinearDampening;

            _asteroidFieldController.IsRotationEnabled = value.AsteroidsRotationEnabled;
            AsteroidsInitialQuantity = value.AsteroidsInitialQuantity;
            AsteroidsMaxQuantity = value.AsteroidsMaxQuantity;
            _asteroidFieldController.MinSpeed = value.AsteroidsMinSpeed;
            _asteroidFieldController.MaxSpeed = value.AsteroidsMaxSpeed;
            _playerController.PlayerGravitationalPullCallback = value.AsteroidsGravityEnabled ? _asteroidFieldController.GetGravitationalVector : null;
            _asteroidFieldController.GravitationalMultiplier = value.AsteroidsGravitationalConstant;

            _playerController.MissileCount = value.PlayerMaxMissiles;
            _playerController.MissileSpeed = value.PlayerMissilesSpeed;
            _playerController.MissileDuration = value.PlayerMissilesLifespan;
        }
    }
}
