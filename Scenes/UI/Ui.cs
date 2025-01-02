using Godot;

namespace Asteroids;

public partial class Ui : CanvasLayer
{
    private Label _gameOverLabel;
    private FlashingLabel _startLabel;
    private Label _helpLabel;
    private Score _score;
    private Lives _lives;
    private HighScore _highScore;

    public override void _Ready()
    {
        _gameOverLabel = (Label)FindChild("Game Over");
        _startLabel = (FlashingLabel)FindChild("Push Start");
        _helpLabel = (Label)FindChild("Help");
        _score = (Score)FindChild("Score");
        _lives = (Lives)FindChild("Lives");
        _highScore = (HighScore)FindChild("High Score");
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

    public int Score
    {
        get => _score.Value;
        set => _score.Value = value;
    }

    public int Lives
    {
        get => _lives.Value;
        set => _lives.Value = value;
    }

    public void AddLife()
    {
        _lives.AddLife();
    }

    public void RemoveLife()
    {
        _lives.RemoveLife();
    }

    public int HighScore
    {
        get => _highScore.Value;
        set => _highScore.Value = value;
    }
}
