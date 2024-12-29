using Godot;

namespace Asteroids;

public partial class HelpDialog : CanvasLayer
{
    [Signal]
    public delegate void OkPressedEventHandler();

    private Button _okButton;

    private FadingPanelContainer _fadingPanelContainer;

    public override void _Ready()
    {
        _fadingPanelContainer = (FadingPanelContainer)FindChild("FadingPanelContainer");

        _okButton = (Button)FindChild("OK Button");
        _okButton.Pressed += OkButtonPressed;

        Hide(true);

        if (GetParent() is Window)
        {
            Show();
        }
    }

    private void Hide(bool immediate = false)
    {
        _fadingPanelContainer.Hide(immediate);
    }

    public void Show(bool immediate = false)
    {
        base.Show();
        _fadingPanelContainer.Show(immediate);
    }

    private void OkButtonPressed()
    {
        _fadingPanelContainer.Hide();
        EmitSignal(SignalName.OkPressed);
    }
}
