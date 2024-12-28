using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Asteroids;

public partial class HighScoreTable : CanvasLayer
{
    private const int _MAX_SCORES = 10;
    private const string _DEFAULT_NAME = "---";

    private readonly List<ScoreDetails> _scoreDetails = new();

    private GridContainer _scoresContainer;

    private bool _changedSinceLastShow = false;

    public int HighScore { get => _scoreDetails.Count > 0 ? _scoreDetails[0].Score : 0; }

    public class ScoreControl
    {
        public Label name;
        public Label score;
    }
    private readonly List<ScoreControl> _scoreControls = new();

    public List<ScoreDetails> GetScores()
    {
        var result = new List<ScoreDetails>();
        result.AddRange(_scoreDetails.Select(sd => new ScoreDetails { Name = sd.Name, Score = sd.Score }));
        return result;
    }

    public override void _Ready()
    {
        Hide();

        _scoresContainer = (GridContainer)FindChild("GridContainer");

        for (var i = 0; i < _MAX_SCORES; i++)
        {
            var scoreControl = new ScoreControl();
            var scoreDetails = new ScoreDetails();

            var rank = new Label
            {
                Text = $"{i + 1}.",
                SizeFlagsHorizontal = Control.SizeFlags.ShrinkBegin,
                HorizontalAlignment = HorizontalAlignment.Left,
                ThemeTypeVariation = "HighScoreLabel"
            };
            _scoresContainer.AddChild(rank);

            var score = new Label
            {
                SizeFlagsHorizontal = Control.SizeFlags.ShrinkEnd | Control.SizeFlags.Expand,
                HorizontalAlignment = HorizontalAlignment.Right,
                ThemeTypeVariation = "HighScoreLabel"
            };
            scoreControl.score = score;
            _scoresContainer.AddChild(score);

            var name = new Label()
            {
                SizeFlagsHorizontal = Control.SizeFlags.ShrinkEnd | Control.SizeFlags.Expand,
                HorizontalAlignment = HorizontalAlignment.Right,
                ThemeTypeVariation = "HighScoreLabel"

            };
            scoreControl.name = name;
            _scoresContainer.AddChild(name);

            scoreDetails.Name = _DEFAULT_NAME;
            scoreDetails.Score = 0;

            _scoreControls.Add(scoreControl);
            _scoreDetails.Add(scoreDetails);
        }

        ApplyScoresToControls();

        if (GetParent() is Window)
        {
            AddScore("Bob", 22000);
            AddScore("Bill", 19);
            AddScore("Zaphod", 6600);

            Show();
        }
    }

    public void SetHighScores(List<ScoreDetails> scores)
    {
        _scoreDetails.Clear();
        _scoreDetails.AddRange(scores.Select(sd => new ScoreDetails { Name = sd.Name, Score = sd.Score }));
        _scoreDetails.Sort((a, b) => b.Score.CompareTo(a.Score));
        ApplyScoresToControls();
    }

    private void ApplyScoresToControls()
    {
        for (var i = 0; i < _scoreControls.Count; i++)
        {
            _scoreControls[i].name.Text = _scoreDetails[i].Name;
            _scoreControls[i].score.Text = _scoreDetails[i].Score.ToString();
        }
    }


    public bool IsEligibleForInclusion(int score)
    {
        return score > (_scoreDetails.Count > 0 ? _scoreDetails[^1].Score : 0);
    }

    public new void Show()
    {
        if (_changedSinceLastShow)
        {
            ApplyScoresToControls();
            _changedSinceLastShow = false;
        }

        base.Show();
    }

    public void AddScore(string name, int score)
    {
        _changedSinceLastShow = true;

        _scoreDetails.Add(new ScoreDetails { Name = name, Score = score });
        _scoreDetails.Sort((a, b) => b.Score.CompareTo(a.Score));
        if (_scoreDetails.Count > _MAX_SCORES)
        {
            _scoreDetails.RemoveAt(_scoreDetails.Count - 1);
        }
    }
}
