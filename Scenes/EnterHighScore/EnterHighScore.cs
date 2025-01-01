using System;
using Godot;

public partial class EnterHighScore : CanvasLayer
{
    [Signal]
    public delegate void NameEnteredEventHandler(string name);

    private LineEdit _highScore;

    public override void _Ready()
    {
        _highScore = (LineEdit)FindChild("High Score");
        _highScore.TextChangeRejected += OnTextChangeRejected;
        _highScore.TextSubmitted += OnTextSubmitted;
        Hide();

        if (GetParent() is Window)
        {
            Show();
        }
    }

    public new void Show()
    {
        base.Show();
        _highScore.Text = "";
        _highScore.GrabFocus();
    }

    private void OnTextSubmitted(string text)
    {
        if (text.Length > 0)
        {
            EmitSignal(SignalName.NameEntered, text);
        }
        else
        {
            GD.Print("Beeeep!");
        }
    }


    private void OnTextChangeRejected(string rejectedSubstring)
    {
        GD.Print("BEEEEEEP!");
    }

}
