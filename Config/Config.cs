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
    private CheckBox _soundEnabledCheckbox;
    private CheckBox _asteroidsRotationEnabledCheckbox;
    private SpinBox _asteroidsStartingQuantitySpinBox;

    public class ConfigSettings
    {
        public bool SoundEnabled { get; set; }
        public bool AsteroidsRotationEnabled { get; set; }
        public int AsteroidsStartingQuantity { get; set; }
    }

    public ConfigSettings Settings { get; private set; } = new();

    public override void _Ready()
    {
        Hide();

        _OKButton = (Button)FindChild("OK Button");
        _cancelButton = (Button)FindChild("Cancel Button");
        _soundEnabledCheckbox = (CheckBox)FindChild("Sound Enabled");
        _asteroidsRotationEnabledCheckbox = (CheckBox)FindChild("Asteroids Rotation Enabled");
        _asteroidsStartingQuantitySpinBox = (SpinBox)FindChild("Asteroids Starting Quantity");

        _OKButton.Pressed += OnOkButtonPressed;
        _cancelButton.Pressed += OnCancelButtonPressed;
    }

    public void Show(ConfigSettings configSettings)
    {
        _soundEnabledCheckbox.ButtonPressed = configSettings.SoundEnabled;
        _asteroidsRotationEnabledCheckbox.ButtonPressed = configSettings.AsteroidsRotationEnabled;
        _asteroidsStartingQuantitySpinBox.Value = configSettings.AsteroidsStartingQuantity;

        Visible = true;
    }

    private void OnCancelButtonPressed()
    {
        Hide();
        EmitSignal(SignalName.Cancel);
    }

    private void OnOkButtonPressed()
    {
        GD.Print(_soundEnabledCheckbox.ButtonPressed);

        Settings = new ConfigSettings
        {
            SoundEnabled = _soundEnabledCheckbox.ButtonPressed,
            AsteroidsRotationEnabled = _asteroidsRotationEnabledCheckbox.ButtonPressed,
            AsteroidsStartingQuantity = (int)_asteroidsStartingQuantitySpinBox.Value
        };

        Hide();

        EmitSignal(SignalName.OkPressed);
    }
}
