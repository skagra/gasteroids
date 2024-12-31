using System.Collections.Generic;
using Godot;

namespace Asteroids;

public partial class Lives : Node2D
{
    [Export]
    private AudioStream _extraLifeSound;

    private PackedScene _lifeScene = Resources.LifeScene;

    private readonly AudioStreamPlayer2D _extraLifeSoundPlayer = new();
    private readonly List<Sprite2D> _lives = new();

    public override void _Ready()
    {
        _extraLifeSoundPlayer.Bus = Resources.AUDIO_BUS_NAME_FX;
        _extraLifeSoundPlayer.Stream = _extraLifeSound;
        AddChild(_extraLifeSoundPlayer);
    }

    public int Value
    {
        get => _lives.Count;
        set => SetLives(value);
    }

    public void SetLives(int lives)
    {
        foreach (var life in _lives)
        {
            life.QueueFree();
        }
        _lives.Clear();

        for (int i = 0; i < lives; i++)
        {
            AddLife(true);
        }
    }

    public void AddLife(bool mute = false)
    {
        if (!mute)
        {
            _extraLifeSoundPlayer.Play();
        }

        var newLife = _lifeScene.Instantiate<Sprite2D>();
        var width = newLife.Texture.GetWidth() * newLife.Scale.X;
        newLife.SetPosition(new Vector2(_lives.Count * width, 0));
        _lives.Add(newLife);
        AddChild(newLife);
    }

    public void RemoveLife()
    {
        if (_lives.Count > 0)
        {
            var lifeToRemove = _lives[_lives.Count - 1];
            _lives.RemoveAt(_lives.Count - 1);
            lifeToRemove.QueueFree();
        }
    }
}
