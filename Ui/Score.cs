using Godot;

namespace Asteroids;

public partial class Score : Label
{
    public int Value
    {
        get { return int.Parse(Text); }
        set { Text = $"{value:D5}"; }
    }

    public void Increase(int delta)
    {
        Value += delta;
    }
}
