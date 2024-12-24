using Godot;

namespace Asteroids;

public partial class SettingsDialog : CanvasLayer
{
    // Signals
    [Signal]
    public delegate void OkPressedEventHandler();

    [Signal]
    public delegate void CancelEventHandler();

    // Graphics
    private CheckBox _backgroundEnabled;

    // Sound
    private CheckBox _soundEnabled;

    // Player ships
    CheckBox _shipInfiniteLives;
    SpinBox _shipStartingCount;
    SpinBox _shipMax;
    SpinBox _shipExtraThreshold;
    HSlider _shipAcceleration;
    HSlider _shipRotationSpeed;

    // Asteroids
    private CheckBox _asteroidsRotationEnabled;
    private SpinBox _asteroidsStartingQuantity;
    private SpinBox _asteroidsMaxStartingQuantity;
    private HSlider _asteroidsMinSpeed;
    private HSlider _asteroidsMaxSpeed;
    private CheckBox _asteroidsGravityEnabled;
    private HSlider _asteroidGravitationalConstant;

    // Missiles
    private SpinBox _missilesMax;
    private HSlider _missilesSpeed;
    private HSlider _missilesLifespan;

    // Missiles
    private Button _cancelButton;
    private Button _resetButton;
    private Button _okButton;

    // Currently active settings
    public GameSettings ActiveSettings { get; set; }

    // Default settings values
    private readonly GameSettings _defaultSettings = new()
    {
        BackgroundEnabled = true,
        SoundEnabled = true,
        ShipInfiniteLives = false,
        ShipStartingCount = 3,
        ShipMax = 6,
        ShipExtraThreshold = 10000,
        ShipAcceleration = 300,
        ShipRotationSpeed = 5,
        AsteroidsRotationEnabled = true,
        AsteroidsStartingQuantity = 4,
        AsteroidsMaxStartingQuantity = 20,
        AsteroidsMinSpeed = 100,
        AsteroidsMaxSpeed = 150,
        AsteroidsGravityEnabled = true,
        AsteroidsGravitationalConstant = 1500,
        MissilesMax = 8,
        MissilesSpeed = 200,
        MissilesLifespan = 1.5f
    };

    public override void _Ready()
    {
        // Get references to controls
        _backgroundEnabled = (CheckBox)FindChild("Background Enabled");

        _soundEnabled = (CheckBox)FindChild("Sound Enabled");

        _shipInfiniteLives = (CheckBox)FindChild("Ship Infinite Lives");
        _shipStartingCount = (SpinBox)FindChild("Ship Starting Count");
        _shipMax = (SpinBox)FindChild("Ship Max");
        _shipExtraThreshold = (SpinBox)FindChild("Ship Extra Threshold");
        _shipAcceleration = (HSlider)FindChild("Ship Acceleration");
        _shipRotationSpeed = (HSlider)FindChild("Ship Rotation Speed");

        _asteroidsRotationEnabled = (CheckBox)FindChild("Asteroids Rotation Enabled");
        _asteroidsStartingQuantity = (SpinBox)FindChild("Asteroids Starting Quantity");
        _asteroidsMaxStartingQuantity = (SpinBox)FindChild("Asteroids Max Starting Quantity");
        _asteroidsMinSpeed = (HSlider)FindChild("Asteroids Min Speed");
        _asteroidsMaxSpeed = (HSlider)FindChild("Asteroids Max Speed");
        _asteroidsGravityEnabled = (CheckBox)FindChild("Asteroids Gravity Enabled");
        _asteroidGravitationalConstant = (HSlider)FindChild("Asteroids Gravitational Constant");

        _missilesSpeed = (HSlider)FindChild("Missiles Speed");
        _missilesMax = (SpinBox)FindChild("Missiles Maximum");
        _missilesLifespan = (HSlider)FindChild("Missiles Lifespan");

        _okButton = (Button)FindChild("OK Button");
        _resetButton = (Button)FindChild("Reset Button");
        _cancelButton = (Button)FindChild("Cancel Button");

        // Set up signals
        _okButton.Pressed += OnOkButtonPressed;
        _resetButton.Pressed += OnResetPressed;
        _cancelButton.Pressed += OnCancelButtonPressed;

        // Start with default settings
        ApplyDefaultSettings();

        // Testing mode
        if (GetParent() is Window)
        {
            Show();
        }
    }

    // Copy default settings into active settings
    private void ApplyDefaultSettings()
    {
        ActiveSettings = new GameSettings(_defaultSettings);
        ApplyActiveSettings();
    }

    // Copy control values into active settings
    private void ApplyControlSettings()
    {
        ActiveSettings = new GameSettings
        {
            BackgroundEnabled = _backgroundEnabled.ButtonPressed,

            SoundEnabled = _soundEnabled.ButtonPressed,

            ShipInfiniteLives = _shipInfiniteLives.ButtonPressed,
            ShipStartingCount = (int)_shipStartingCount.Value,
            ShipMax = (int)_shipMax.Value,
            ShipExtraThreshold = (int)_shipExtraThreshold.Value,
            ShipAcceleration = (float)_shipAcceleration.Value,
            ShipRotationSpeed = (float)_shipRotationSpeed.Value,

            AsteroidsRotationEnabled = _asteroidsRotationEnabled.ButtonPressed,
            AsteroidsStartingQuantity = (int)_asteroidsStartingQuantity.Value,

            AsteroidsMaxStartingQuantity = (int)_asteroidsMaxStartingQuantity.Value,
            AsteroidsMinSpeed = (float)_asteroidsMinSpeed.Value,
            AsteroidsMaxSpeed = (float)_asteroidsMaxSpeed.Value,
            AsteroidsGravityEnabled = _asteroidsGravityEnabled.ButtonPressed,
            AsteroidsGravitationalConstant = (float)_asteroidGravitationalConstant.Value,

            MissilesMax = (int)_missilesMax.Value,
            MissilesSpeed = (float)_missilesSpeed.Value,
            MissilesLifespan = (float)_missilesLifespan.Value
        };
    }

    // Copy active settings into controls
    private void ApplyActiveSettings()
    {
        _backgroundEnabled.ButtonPressed = ActiveSettings.BackgroundEnabled;

        _soundEnabled.ButtonPressed = ActiveSettings.SoundEnabled;

        _shipInfiniteLives.ButtonPressed = ActiveSettings.ShipInfiniteLives;
        _shipStartingCount.Value = ActiveSettings.ShipStartingCount;
        _shipMax.Value = ActiveSettings.ShipMax;
        _shipExtraThreshold.Value = ActiveSettings.ShipExtraThreshold;
        _shipAcceleration.Value = ActiveSettings.ShipAcceleration;
        _shipRotationSpeed.Value = ActiveSettings.ShipRotationSpeed;

        _asteroidsRotationEnabled.ButtonPressed = ActiveSettings.AsteroidsRotationEnabled;
        _asteroidsStartingQuantity.Value = ActiveSettings.AsteroidsStartingQuantity;
        _asteroidsMaxStartingQuantity.Value = ActiveSettings.AsteroidsMaxStartingQuantity;
        _asteroidsMinSpeed.Value = ActiveSettings.AsteroidsMinSpeed;
        _asteroidsMaxSpeed.Value = ActiveSettings.AsteroidsMaxSpeed;
        _asteroidsGravityEnabled.ButtonPressed = ActiveSettings.AsteroidsGravityEnabled;
        _asteroidGravitationalConstant.Value = ActiveSettings.AsteroidsGravitationalConstant;

        _missilesMax.Value = ActiveSettings.MissilesMax;
        _missilesSpeed.Value = ActiveSettings.MissilesSpeed;
        _missilesLifespan.Value = ActiveSettings.MissilesLifespan;
    }

    // Show the settings dialog with currently active settings
    public new void Show()
    {
        ApplyActiveSettings();
        _backgroundEnabled.CallDeferred(CheckBox.MethodName.GrabFocus);
        base.Show();
    }

    // Signal handling
    private void OnCancelButtonPressed()
    {
        Hide();

        EmitSignal(SignalName.Cancel);
    }

    private void OnResetPressed()
    {
        ApplyDefaultSettings();
    }

    private void OnOkButtonPressed()
    {
        ApplyControlSettings();
        Hide();

        EmitSignal(SignalName.OkPressed);
    }
}
