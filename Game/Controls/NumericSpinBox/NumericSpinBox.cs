using Godot;
using System;

namespace Asteroids;

public partial class NumericSpinBox : SpinBox
{
    [Export]
    private AudioStream _errorBeep;

    private AudioStreamPlayer _audioStreamPlayer = new();

    private LineEdit _lineEdit;

    public override void _Ready()
    {
        _audioStreamPlayer.Bus = Resources.AUDIO_BUS_NAME_UI;
        _audioStreamPlayer.Stream = _errorBeep ?? throw new NullReferenceException("Error beep not set");
        AddChild(_audioStreamPlayer);

        Alignment = HorizontalAlignment.Right;
        _lineEdit = GetLineEdit();
        _lineEdit.GuiInput += LineEditGuiInput;

        if (GetParent() is Window)
        {
            Position = Screen.Centre;
            Show();
            GrabClickFocus();
        }
    }

    private void LineEditGuiInput(InputEvent inputEvent)
    {
        if (inputEvent is InputEventKey inputEventKey
            && !inputEventKey.IsEcho()
            && inputEventKey.Pressed)
        {
            var character = inputEventKey.AsText();
            var key = inputEventKey.Keycode;
            if (!Char.IsDigit(character[0]) && ((key & Key.Special) == 0))
            {
                _audioStreamPlayer.Play();
                AcceptEvent();
            }
        }
    }
}
