using System;
using System.Collections.Generic;
using Godot;

namespace Asteroids;

public partial class Lives : Node2D
{
    private PackedScene _lifeScene = Resources.LifeScene;

    private readonly List<TextureRect> _lives = new();

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
        var newLife = _lifeScene.Instantiate<TextureRect>();
        // Y sizes are used for width because the image is rotated 270 degrees
        var width = newLife.Size.Y * newLife.Scale.Y;
        newLife.SetPosition(new Vector2(_lives.Count * width - 1, 0));
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
