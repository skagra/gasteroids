using System;
using Godot;

namespace Asteroids;

public partial class EnterHighScore : CanvasLayer
{
    [Signal]
    public delegate void NameEnteredEventHandler(string name);

    [Export]
    private AudioStream _errorBeep;

    private AudioStreamPlayer2D _audioStreamPlayer = new();

    private LineEdit _highScore;

    public override void _Ready()
    {
        _audioStreamPlayer.Bus = Resources.AUDIO_BUS_NAME_UI;
        _audioStreamPlayer.Stream = _errorBeep ?? throw new NullReferenceException("Error beep not set");
        AddChild(_audioStreamPlayer);

        _highScore = (LineEdit)FindChild("High Score") ?? throw new NullReferenceException("High Score not found");
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
            _audioStreamPlayer.Play();
        }
    }


    private void OnTextChangeRejected(string rejectedSubstring)
    {
        _audioStreamPlayer.Play();
    }

}
