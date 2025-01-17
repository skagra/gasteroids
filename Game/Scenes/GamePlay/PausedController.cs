using Godot;

namespace Asteroids;

public partial class PausedController : Node
{
    public Ui UI { get; set; }

    public override void _UnhandledKeyInput(InputEvent inputEvent)
    {
        if (inputEvent.IsActionPressed("Pause"))
        {
            GetTree().Paused = false;
            GetViewport().SetInputAsHandled();
            UI.HidePausedLabel();
        }
    }
}
