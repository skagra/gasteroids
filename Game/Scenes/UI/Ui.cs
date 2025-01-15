using System;
using Godot;

namespace Asteroids;

public partial class Ui : CanvasLayer
{
    private Label _gameOverLabel;
    private FlashingLabel _startLabel;
    private Label _helpLabel;
    private HighScore _highScore;

    public override void _Ready()
    {
        _gameOverLabel = (Label)FindChild("Game Over") ?? throw new NullReferenceException("Game Over not found");
        _startLabel = (FlashingLabel)FindChild("Push Start") ?? throw new NullReferenceException("Push Start not found");
        _helpLabel = (Label)FindChild("Help") ?? throw new NullReferenceException("Help not found");
        _highScore = (HighScore)FindChild("High Score") ?? throw new NullReferenceException("High Score not found");
    }

    public void ShowGameOverLabel()
    {
        _gameOverLabel.Show();
    }

    public void HideGameOverLabel()
    {
        _gameOverLabel.Hide();
    }

    public void ShowStartLabel()
    {
        _startLabel.Show();
    }

    public void HideStartLabel()
    {
        _startLabel.Hide();
    }

    public void ShowHelpLabel()
    {
        _helpLabel.Show();
    }

    public void HideHelpLabel()
    {
        _helpLabel.Hide();
    }

    public int HighScore
    {
        get => _highScore.Value;
        set => _highScore.Value = value;
    }
}
