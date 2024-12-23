using Godot;

public partial class ControlsDialog : CanvasLayer
{
    [Signal]
    public delegate void OkPressedEventHandler();

    private Button _okButton;

    public override void _Ready()
    {
        _okButton = (Button)FindChild("Ok Button");
        _okButton.Pressed += OkButtonPressed;

        if (GetParent() is Window)
        {
            Show();
        }
    }

    public new void Show()
    {
        base.Show();
    }

    private void OkButtonPressed()
    {
        Hide();
        EmitSignal(SignalName.OkPressed);
    }
}
