using Godot;

namespace Asteroids;

public partial class Resources : Node
{
    private static Resources _I;

    public const string AUDIO_BUS_NAME_MASTER = "Master";
    public const string AUDIO_BUS_NAME_FX = "FX";
    public const string AUDIO_BUS_NAME_UI = "UI";
    public const string GROUP_AUDIO_FX = "FX";

    private const string _CLASSIC_THEME_NAME = "res://Themes/Classic.tres";
    private const string _MODERN_THEME_NAME = "res://Themes/Modern.tres";
    private const string _THEMEABLE_GROUP = "Themeable";
    public enum Themes { Modern, Classic }
    private static readonly Theme _classicTheme = GD.Load<Theme>(_CLASSIC_THEME_NAME);
    private static readonly Theme _modernTheme = GD.Load<Theme>(_MODERN_THEME_NAME);

    private const string _ASTEROID_SCENE_BASE = "res://Scenes/Asteroid/";

    public static readonly PackedScene AsteroidType1Large = GD.Load<PackedScene>($"{_ASTEROID_SCENE_BASE}AsteroidType1Large.tscn");
    public static readonly PackedScene AsteroidType2Large = GD.Load<PackedScene>($"{_ASTEROID_SCENE_BASE}AsteroidType2Large.tscn");
    public static readonly PackedScene AsteroidType3Large = GD.Load<PackedScene>($"{_ASTEROID_SCENE_BASE}AsteroidType3Large.tscn");

    public static readonly PackedScene AsteroidType1Medium = GD.Load<PackedScene>($"{_ASTEROID_SCENE_BASE}AsteroidType1Medium.tscn");
    public static readonly PackedScene AsteroidType2Medium = GD.Load<PackedScene>($"{_ASTEROID_SCENE_BASE}AsteroidType2Medium.tscn");
    public static readonly PackedScene AsteroidType3Medium = GD.Load<PackedScene>($"{_ASTEROID_SCENE_BASE}AsteroidType3Medium.tscn");

    public static readonly PackedScene AsteroidType1Small = GD.Load<PackedScene>($"{_ASTEROID_SCENE_BASE}AsteroidType1Small.tscn");
    public static readonly PackedScene AsteroidType2Small = GD.Load<PackedScene>($"{_ASTEROID_SCENE_BASE}AsteroidType2Small.tscn");
    public static readonly PackedScene AsteroidType3Small = GD.Load<PackedScene>($"{_ASTEROID_SCENE_BASE}AsteroidType3Small.tscn");

    public static readonly PackedScene LifeScene = GD.Load<PackedScene>("res://Scenes/UI/LifeTextureRect.tscn");

    public static Panel _background;
    public override void _EnterTree()
    {
        _I = this;
    }

    public override void _Ready()
    {
        _background = (Panel)GetNode("/root/Main/Background");
    }

    public static void SwitchTheme(Themes theme)
    {
        switch (theme)
        {
            case Themes.Classic:
                _I.GetTree().CallGroup(_THEMEABLE_GROUP, Control.MethodName.SetTheme, _classicTheme);
                _background.Hide();
                break;
            case Themes.Modern:
                _I.GetTree().CallGroup(_THEMEABLE_GROUP, Control.MethodName.SetTheme, _modernTheme);
                _background.Show();
                break;
        }
    }
}
