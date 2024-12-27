using System.Collections.Generic;
using Godot;

public partial class HighScoreTable : CanvasLayer
{
    private const int _MAX_SCORES = 10;

    private class ScoreDetails
    {
        public string name;
        public int score;
    }

    private List<ScoreDetails> _scoreDetails = new();

    private GridContainer _scoresContainer;

    private bool _changedSinceLastShow = false;

    public int HighScore { get => _scoreDetails.Count > 0 ? _scoreDetails[0].score : 0; }

    public override void _Ready()
    {
        _scoresContainer = (GridContainer)FindChild("GridContainer");

        Hide();

        if (GetParent() is Window)
        {
            AddScore("Bob", 22000);
            AddScore("Bill", 19);
            AddScore("Zaphod", 6600);

            Show();
        }
    }

    public bool IsEligibleForInclusion(int score)
    {
        return score > (_scoreDetails.Count > 0 ? _scoreDetails[^1].score : 0);
    }

    public void ClearScores()
    {
        foreach (var control in _scoresContainer.GetChildren())
        {
            control.QueueFree();
        }
        _scoreDetails.Clear();

        _changedSinceLastShow = true;
    }

    public new void Show()
    {
        if (_changedSinceLastShow)
        {
            for (int i = 0; i < _scoreDetails.Count; i++)
            {
                var rank = new Label
                {
                    Text = $"{i + 1}.",
                    SizeFlagsHorizontal = Control.SizeFlags.ShrinkBegin,
                    HorizontalAlignment = HorizontalAlignment.Left

                };
                _scoresContainer.AddChild(rank);

                var score = new Label
                {
                    Text = _scoreDetails[i].score.ToString(),
                    SizeFlagsHorizontal = Control.SizeFlags.ShrinkEnd | Control.SizeFlags.Expand,
                    HorizontalAlignment = HorizontalAlignment.Right
                };
                _scoresContainer.AddChild(score);

                var name = new Label()
                {
                    Text = _scoreDetails[i].name,
                    SizeFlagsHorizontal = Control.SizeFlags.ShrinkEnd | Control.SizeFlags.Expand,
                    HorizontalAlignment = HorizontalAlignment.Right
                };

                _scoresContainer.AddChild(name);
            }
        }

        _changedSinceLastShow = false;

        base.Show();
    }

    public void AddScore(string name, int score)
    {
        _changedSinceLastShow = true;

        _scoreDetails.Add(new ScoreDetails { name = name, score = score });
        _scoreDetails.Sort((a, b) => b.score.CompareTo(a.score));
        if (_scoreDetails.Count > _MAX_SCORES)
        {
            _scoreDetails.RemoveAt(_scoreDetails.Count - 1);
        }
    }
}
