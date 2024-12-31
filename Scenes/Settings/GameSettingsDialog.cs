using Godot;
using static Asteroids.Resources;

namespace Asteroids;

public partial class GameSettingsDialog : CanvasLayer
{
    // Signals
    [Signal]
    public delegate void OkPressedEventHandler();

    [Signal]
    public delegate void CancelEventHandler();

    private FadingPanelContainer _fadingPanelContainer;

    // Pre-canned configurations
    OptionButton _configurationSet;

    // Theme
    OptionButton _theme;

    // Sound
    private CheckBox _soundEnabled;

    // Player ships
    CheckBox _playerInfiniteLives;
    SpinBox _playerStartingLives;
    SpinBox _playerMaxLives;
    SpinBox _playerExtraLifeScoreThreshold;

    HSlider _shipAcceleration;
    HSlider _shipTurnSpeed;
    HSlider _shipLinearDampening;

    // Asteroids
    private CheckBox _asteroidsRotationEnabled;
    private SpinBox _asteroidsInitialQuantity;
    private SpinBox _asteroidsMaxQuantity;
    private HSlider _asteroidsMinSpeed;
    private HSlider _asteroidsMaxSpeed;
    private CheckBox _asteroidsGravityEnabled;
    private HSlider _asteroidGravitationalConstant;

    // Player missiles
    private SpinBox _playerMaxMissiles;
    private HSlider _playerMissilesSpeed;
    private HSlider _playerMissilesLifespan;

    // Large saucer
    private CheckBox _largeSaucerEnabled;
    private HSlider _largeSaucerSpeed;
    private HSlider _largeSaucerSpawnFrequency;
    private SpinBox _largeSaucerMaxMissiles;
    private HSlider _largeSaucerMissilesSpeed;
    private HSlider _largeSaucerMissilesLifespan;

    // Small saucer
    private CheckBox _smallSaucerEnabled;
    private HSlider _smallSaucerSpeed;
    private HSlider _smallSaucerSpawnFrequency;
    private SpinBox _smallSaucerMaxMissiles;
    private HSlider _smallSaucerMissilesSpeed;
    private HSlider _smallSaucerMissilesLifespan;

    // Buttons
    private Button _cancelButton;
    private Button _okButton;

    // Currently active settings
    public GameSettings ActiveSettings { get; set; }

    public override void _Ready()
    {
        _fadingPanelContainer = (FadingPanelContainer)FindChild("FadingPanelContainer");

        // Get references to controls
        _configurationSet = (OptionButton)FindChild("Configuration Set");

        _theme = (OptionButton)FindChild("Theme");

        _soundEnabled = (CheckBox)FindChild("Sound Enabled");

        _playerInfiniteLives = (CheckBox)FindChild("Player Infinite Lives");
        _playerStartingLives = (SpinBox)FindChild("Player Starting Lives");
        _playerMaxLives = (SpinBox)FindChild("Player Max Lives");
        _playerExtraLifeScoreThreshold = (SpinBox)FindChild("Player Extra Life Score Threshold");

        _shipAcceleration = (HSlider)FindChild("Ship Acceleration");
        _shipTurnSpeed = (HSlider)FindChild("Ship Turn Speed");
        _shipLinearDampening = (HSlider)FindChild("Ship Linear Dampening");

        _playerMaxMissiles = (SpinBox)FindChild("Player Max Missiles");
        _playerMissilesSpeed = (HSlider)FindChild("Player Missiles Speed");
        _playerMissilesLifespan = (HSlider)FindChild("Player Missiles Lifespan");

        _asteroidsRotationEnabled = (CheckBox)FindChild("Asteroids Rotation Enabled");
        _asteroidsInitialQuantity = (SpinBox)FindChild("Asteroids Initial Quantity");
        _asteroidsMaxQuantity = (SpinBox)FindChild("Asteroids Max Quantity");
        _asteroidsMinSpeed = (HSlider)FindChild("Asteroids Min Speed");
        _asteroidsMaxSpeed = (HSlider)FindChild("Asteroids Max Speed");
        _asteroidsGravityEnabled = (CheckBox)FindChild("Asteroids Gravity Enabled");
        _asteroidGravitationalConstant = (HSlider)FindChild("Asteroids Gravitational Constant");

        _largeSaucerEnabled = (CheckBox)FindChild("Large Saucer Enabled");
        _largeSaucerSpeed = (HSlider)FindChild("Large Saucer Speed");
        _largeSaucerSpawnFrequency = (HSlider)FindChild("Large Saucer Spawn Frequency");
        _largeSaucerMaxMissiles = (SpinBox)FindChild("Large Saucer Max Missiles");
        _largeSaucerMissilesSpeed = (HSlider)FindChild("Large Saucer Missiles Speed");
        _largeSaucerMissilesLifespan = (HSlider)FindChild("Large Saucer Missiles Lifespan");

        _smallSaucerEnabled = (CheckBox)FindChild("Small Saucer Enabled");
        _smallSaucerSpeed = (HSlider)FindChild("Small Saucer Speed");
        _smallSaucerSpawnFrequency = (HSlider)FindChild("Small Saucer Spawn Frequency");
        _smallSaucerMaxMissiles = (SpinBox)FindChild("Small Saucer Max Missiles");
        _smallSaucerMissilesSpeed = (HSlider)FindChild("Small Saucer Missiles Speed");
        _smallSaucerMissilesLifespan = (HSlider)FindChild("Small Saucer Missiles Lifespan");

        _okButton = (Button)FindChild("OK Button");
        _cancelButton = (Button)FindChild("Cancel Button");

        // Set up signals
        _configurationSet.ItemSelected += ConfigurationSetOnItemSelected;
        _okButton.Pressed += OnOkButtonPressed;
        _cancelButton.Pressed += OnCancelButtonPressed;

        // Testing mode
        if (GetParent() is Window)
        {
            ActiveSettings = GameSettingsPresets.GetSettings(GameSettingsPresets.SettingsPresets.Normal);
            Show();
        }
    }

    private void ConfigurationSetOnItemSelected(long index)
    {
        GD.Print(index);
        ActiveSettings = GameSettingsPresets.GetSettings((GameSettingsPresets.SettingsPresets)index);
        CopyActiveSettingsToDialogControls();
    }

    // Copy control values into active settings
    private void CopyDialogValuesToActiveSettings()
    {
        ActiveSettings = new GameSettings
        {
            ConfigurationSet = (GameSettingsPresets.SettingsPresets)_configurationSet.GetSelectedId(),

            Theme = (Themes)_theme.GetSelectedId(),

            SoundEnabled = _soundEnabled.ButtonPressed,

            PlayerInfiniteLives = _playerInfiniteLives.ButtonPressed,
            PlayerStartingLives = (int)_playerStartingLives.Value,
            PlayerMaxLives = (int)_playerMaxLives.Value,
            PlayerExtraLifeScoreThreshold = (int)_playerExtraLifeScoreThreshold.Value,

            ShipAcceleration = (float)_shipAcceleration.Value,
            ShipTurnSpeed = (float)_shipTurnSpeed.Value,
            ShipLinearDampening = (float)_shipLinearDampening.Value,

            AsteroidsRotationEnabled = _asteroidsRotationEnabled.ButtonPressed,
            AsteroidsInitialQuantity = (int)_asteroidsInitialQuantity.Value,

            AsteroidsMaxQuantity = (int)_asteroidsMaxQuantity.Value,
            AsteroidsMinSpeed = (float)_asteroidsMinSpeed.Value,
            AsteroidsMaxSpeed = (float)_asteroidsMaxSpeed.Value,
            AsteroidsGravityEnabled = _asteroidsGravityEnabled.ButtonPressed,
            AsteroidsGravitationalConstant = (float)_asteroidGravitationalConstant.Value,

            PlayerMaxMissiles = (int)_playerMaxMissiles.Value,
            PlayerMissilesSpeed = (float)_playerMissilesSpeed.Value,
            PlayerMissilesLifespan = (float)_playerMissilesLifespan.Value,
            // TODO and in next also
        };
    }

    // Copy active settings into controls
    private void CopyActiveSettingsToDialogControls()
    {
        _configurationSet.Select((int)ActiveSettings.ConfigurationSet);

        _theme.Select((int)ActiveSettings.Theme);

        _soundEnabled.ButtonPressed = ActiveSettings.SoundEnabled;

        _playerInfiniteLives.ButtonPressed = ActiveSettings.PlayerInfiniteLives;
        _playerStartingLives.Value = ActiveSettings.PlayerStartingLives;
        _playerMaxLives.Value = ActiveSettings.PlayerMaxLives;
        _playerExtraLifeScoreThreshold.Value = ActiveSettings.PlayerExtraLifeScoreThreshold;

        _shipAcceleration.Value = ActiveSettings.ShipAcceleration;
        _shipTurnSpeed.Value = ActiveSettings.ShipTurnSpeed;
        _shipLinearDampening.Value = ActiveSettings.ShipLinearDampening;

        _asteroidsRotationEnabled.ButtonPressed = ActiveSettings.AsteroidsRotationEnabled;
        _asteroidsInitialQuantity.Value = ActiveSettings.AsteroidsInitialQuantity;
        _asteroidsMaxQuantity.Value = ActiveSettings.AsteroidsMaxQuantity;
        _asteroidsMinSpeed.Value = ActiveSettings.AsteroidsMinSpeed;
        _asteroidsMaxSpeed.Value = ActiveSettings.AsteroidsMaxSpeed;
        _asteroidsGravityEnabled.ButtonPressed = ActiveSettings.AsteroidsGravityEnabled;
        _asteroidGravitationalConstant.Value = ActiveSettings.AsteroidsGravitationalConstant;

        _playerMaxMissiles.Value = ActiveSettings.PlayerMaxMissiles;
        _playerMissilesSpeed.Value = ActiveSettings.PlayerMissilesSpeed;
        _playerMissilesLifespan.Value = ActiveSettings.PlayerMissilesLifespan;
    }

    // Show the settings dialog with currently active settings
    public void Show(bool immediate = false)
    {
        CopyActiveSettingsToDialogControls();
        _configurationSet.CallDeferred(CheckBox.MethodName.GrabFocus);
        _fadingPanelContainer.Show(immediate);
        base.Show();
    }

    // Signal handling
    private void OnCancelButtonPressed()
    {
        Hide();

        EmitSignal(SignalName.Cancel);
    }

    private void OnOkButtonPressed()
    {
        CopyDialogValuesToActiveSettings();
        Hide();

        EmitSignal(SignalName.OkPressed);
    }

    public void Hide(bool immediate = false)
    {
        _fadingPanelContainer.Hide(immediate);
    }
}
