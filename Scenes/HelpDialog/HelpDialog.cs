using System;
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
        _fadingPanelContainer = (FadingPanelContainer)FindChild("FadingPanelContainer") ?? throw new NullReferenceException("FadingPanelContainer not found");

        _okButton = (Button)FindChild("OK Button") ?? throw new NullReferenceException("OK Button not found");
        _okButton.Pressed += OkButtonPressed;

        Hide(true);

        if (GetParent() is Window)
        {
            Show();
        }
    }

    public void Hide(bool immediate = false)
    {
        _fadingPanelContainer.Hide(immediate);
    }

    public void Show(bool immediate = false)
    {
        _fadingPanelContainer.Show(immediate);
        base.Show();
    }

    private void OkButtonPressed()
    {
        _fadingPanelContainer.Hide();
        EmitSignal(SignalName.OkPressed);
    }
}
