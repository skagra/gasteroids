using Godot;

namespace Asteroids;

public partial class Config : CanvasLayer
{
    [Signal]
    public delegate void OkPressedEventHandler();

    [Signal]
    public delegate void CancelEventHandler();

    private Button _cancelButton;
    private Button _OKButton;

    private CheckBox _soundEnabled;

    private CheckBox _backgroundEnabled;

    CheckBox _shipInvulnerable;
    SpinBox _shipStartingCount;
    SpinBox _shipMax;
    SpinBox _shipExtraThreshold;
    HSlider _shipAcceleration;
    HSlider _shipRotationSpeed;

    private CheckBox _asteroidsRotationEnabled;
    private SpinBox _asteroidsStartingQuantity;
    private SpinBox _asteroidsMaxStartingQuantity;
    private HSlider _asteroidsMinSpeed;
    private HSlider _asteroidsMaxSpeed;

    private SpinBox _missilesMax;
    private HSlider _missilesLifespan;

    public class ConfigSettings
    {
        public bool BackgroundEnabled { get; set; }

        public bool SoundEnabled { get; set; }

        public bool ShipInvulnerable { get; set; }
        public int ShipStartingCount { get; set; }
        public int ShipMax { get; set; }
        public int ShipExtraThreshold { get; set; }
        public float ShipAcceleration { get; set; }
        public float ShipRotationSpeed { get; set; }

        public bool AsteroidsRotationEnabled { get; set; }
        public int AsteroidsStartingQuantity { get; set; }
        public int AsteroidsMaxStartingQuantity { get; set; }
        public float AsteroidsMinSpeed { get; set; }
        public float AsteroidsMaxSpeed { get; set; }

        public int MissilesMax { get; set; }
        public float MissilesLifespan { get; set; }
    }

    public ConfigSettings Settings { get; private set; }

    public override void _Ready()
    {
        _backgroundEnabled = (CheckBox)FindChild("Background Enabled");

        _soundEnabled = (CheckBox)FindChild("Sound Enabled");

        _shipInvulnerable = (CheckBox)FindChild("Ship Invulnerable");
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

        _missilesMax = (SpinBox)FindChild("Missiles Maximum");
        _missilesLifespan = (HSlider)FindChild("Missiles Lifespan");

        _OKButton = (Button)FindChild("OK Button");
        _cancelButton = (Button)FindChild("Cancel Button");

        _OKButton.Pressed += OnOkButtonPressed;
        _cancelButton.Pressed += OnCancelButtonPressed;

        CollectConfigurationFromControls();

        if (GetParent() is Window)
        {
            Show();
        }
    }

    private void CollectConfigurationFromControls()
    {
        Settings = new ConfigSettings
        {
            BackgroundEnabled = _backgroundEnabled.ButtonPressed,

            SoundEnabled = _soundEnabled.ButtonPressed,

            ShipInvulnerable = _shipInvulnerable.ButtonPressed,
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

            MissilesMax = (int)_missilesMax.Value,
            MissilesLifespan = (float)_missilesLifespan.Value
        };
    }

    public void Show(ConfigSettings configSettings)
    {
        _backgroundEnabled.ButtonPressed = configSettings.BackgroundEnabled;

        _soundEnabled.ButtonPressed = configSettings.SoundEnabled;

        _shipInvulnerable.ButtonPressed = configSettings.ShipInvulnerable;
        _shipStartingCount.Value = configSettings.ShipStartingCount;
        _shipMax.Value = configSettings.ShipMax;
        _shipExtraThreshold.Value = configSettings.ShipExtraThreshold;
        _shipAcceleration.Value = configSettings.ShipAcceleration;
        _shipRotationSpeed.Value = configSettings.ShipRotationSpeed;

        _asteroidsRotationEnabled.ButtonPressed = configSettings.AsteroidsRotationEnabled;
        _asteroidsStartingQuantity.Value = configSettings.AsteroidsStartingQuantity;

        _asteroidsMaxStartingQuantity.Value = configSettings.AsteroidsMaxStartingQuantity;
        _asteroidsMinSpeed.Value = configSettings.AsteroidsMinSpeed;
        _asteroidsMaxSpeed.Value = configSettings.AsteroidsMaxSpeed;

        _missilesMax.Value = configSettings.MissilesMax;
        _missilesLifespan.Value = configSettings.MissilesLifespan;

        Show();
    }

    private void OnCancelButtonPressed()
    {
        Hide();

        EmitSignal(SignalName.Cancel);
    }

    private void OnOkButtonPressed()
    {
        CollectConfigurationFromControls();
        Hide();

        EmitSignal(SignalName.OkPressed);
    }
}
