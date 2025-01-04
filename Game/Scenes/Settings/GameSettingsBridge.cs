using System;
using Godot;

namespace Asteroids;

public class GameSettingsBridge
{
    private readonly PlayerController _playerController;
    private readonly AsteroidFieldController _asteroidFieldController;
    private readonly SaucerController _largeSaucerController;
    private readonly SaucerController _smallSaucerController;
    private readonly Beats _beats;

    public GameSettingsBridge(PlayerController playerController,
                              AsteroidFieldController asteroidFieldController,
                              SaucerController largeSaucerController,
                              SaucerController smallSaucerController,
                              Beats beats)
    {
        _playerController = playerController;
        _asteroidFieldController = asteroidFieldController;
        _largeSaucerController = largeSaucerController;
        _smallSaucerController = smallSaucerController;
        _beats = beats;
    }

    [Flags]
    public enum Fields
    {
        None = 0b0000_1000,
        All = 0b1111_1111,
        Theme = 0b0000_0001,
        Sound = 0b0000_0010
    }

    public void Apply(GameSettings value, Fields ignoreFields = Fields.None)
    {
        if ((ignoreFields & Fields.Theme) == 0)
        {
            Resources.SwitchTheme(value.Theme);
        }

        if ((ignoreFields & Fields.Sound) == 0)
        {
            Resources.EnableNewSoundFx(value.SoundEnabled);
        }

        _playerController.PlayerThrustForce = value.ShipAcceleration;
        _playerController.PlayerRotationSpeed = value.ShipTurnSpeed;
        _playerController.LinearDampening = value.ShipLinearDampening;

        _asteroidFieldController.IsRotationEnabled = value.AsteroidsRotationEnabled;
        _asteroidFieldController.MinSpeed = value.AsteroidsMinSpeed;
        _asteroidFieldController.MaxSpeed = value.AsteroidsMaxSpeed;
        _playerController.PlayerGravitationalPullCallback = value.AsteroidsGravityEnabled ? _asteroidFieldController.GetGravitationalVector : null;
        _asteroidFieldController.GravitationalMultiplier = value.AsteroidsGravitationalConstant;

        _playerController.MissileCount = value.PlayerMaxMissiles;
        _playerController.MissileSpeed = value.PlayerMissilesSpeed;
        _playerController.MissileDuration = value.PlayerMissilesLifespan;

        _largeSaucerController.Speed = value.LargeSaucerSpeed;
        _largeSaucerController.MaxMissiles = value.LargeSaucerMaxMissiles;
        _largeSaucerController.SpawnTimerMin = value.LargeSaucerMinSpawnTime;
        _largeSaucerController.SpawnTimerMax = value.LargeSaucerMaxSpawnTime;
        _largeSaucerController.MissileSpeed = value.LargeSaucerMissilesSpeed;
        _largeSaucerController.MissileLifespan = value.LargeSaucerMissilesLifespan;

        _smallSaucerController.Speed = value.SmallSaucerSpeed;
        _smallSaucerController.MaxMissiles = value.SmallSaucerMaxMissiles;
        _smallSaucerController.SpawnTimerMin = value.SmallSaucerMinSpawnTime;
        _smallSaucerController.SpawnTimerMax = value.SmallSaucerMaxSpawnTime;
        _smallSaucerController.MissileSpeed = value.SmallSaucerMissilesSpeed;
        _smallSaucerController.MissileLifespan = value.SmallSaucerMissilesLifespan;
    }
}
