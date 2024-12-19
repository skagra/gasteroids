using System.Collections.Generic;
using Godot;

namespace Asteroids;

public partial class Lives : Node2D
{
	private PackedScene _lifeScene = GD.Load<PackedScene>("res://Ui/Life.tscn");

	private readonly List<Sprite2D> _lives = new();

	public int Value
	{
		get { return _lives.Count; }
	}

	public void SetLives(int lives)
	{
		for (int i = 0; i < lives; i++)
		{
			RemoveLife();
		}

		for (int i = 0; i < lives; i++)
		{
			AddLife(true);
		}
	}

	public void AddLife(bool mute = false)
	{
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
			var lifeToRemove = _lives[^1];
			_lives.RemoveAt(_lives.Count - 1);
			lifeToRemove.QueueFree();
		}
	}
}
