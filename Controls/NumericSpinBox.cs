using Godot;
using System;

namespace Asteroids;

public partial class NumericSpinBox : SpinBox
{
    [Export]
    private AudioStream _errorBeep;

    private AudioStreamPlayer2D _audioStreamPlayer;

    private LineEdit _lineEdit;

    public override void _Ready()
    {
        if (_errorBeep != null)
        {
            _audioStreamPlayer = new();
            _audioStreamPlayer.Stream = _errorBeep;
            AddChild(_audioStreamPlayer);
        }

        Alignment = HorizontalAlignment.Right;
        _lineEdit = GetLineEdit();
        _lineEdit.GuiInput += LineEditGuiInput;
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
                _audioStreamPlayer?.Play();
                AcceptEvent();
            }
        }
    }
}
