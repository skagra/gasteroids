using Godot;
namespace Asteroids;

public partial class Main : Node
{
	private AsteroidField _asteroidField;
	private BulletController _bulletController;
	private Player _player;
	private Score _score;
	private Lives _lives;
	private Label _gameOverLabel;

	public override void _Ready()
	{
		_player = GetNode<Player>("Player");
		_bulletController = GetNode<BulletController>("BulletController");
		_asteroidField = GetNode<AsteroidField>("AsteroidField");
		_score = GetNode<Score>("Ui/Score");
		_lives = GetNode<Lives>("Ui/Lives");
		_gameOverLabel = GetNode<Label>("Ui/Game Over");

		_bulletController.Collided += OnBulletSignalCollided;

		_asteroidField.Collision += OnAsteroidSignalCollided;
		_asteroidField.CreateField(10, new Rect2(0, 0, 0, 0), false);

		var vp = GetViewport().GetVisibleRect();
		_player.Exploding += OnPlayerSignalExploding;
		_player.Exploded += OnPlayerSignalExploded;
		_player.Shoot += OnPlayerSignalShoot;
		_player.Activate(Screen.Instance.GetCentre());

		_lives.SetLives(3);
	}

	public override void _Process(double delta)
	{
	}

	private void OnPlayerSignalExploded()
	{
		GD.Print("In OnExploded in Main");
		_player.Deactivate();
		if (!_gameOver)
		{
			// This is safe wrt signal delivery order
			// as there is a game between "Exploding" and "Exploded"
			_player.Activate(Screen.Instance.GetCentre());
		}
	}

	private bool _gameOver = false;

	private void OnPlayerSignalExploding()
	{
		if (_lives.Value > 0)
		{
			_lives.RemoveLife();
			if (_lives.Value == 0)
			{
				GD.Print("GAME OVER");
				_gameOver = true;
				_gameOverLabel.Show();
			}
		}
	}

	private void OnPlayerSignalShoot(Vector2 position, Vector2 shipLinearVelocity, float shipRotation)
	{
		GD.Print("Spawning Bullet");
		_bulletController.SpawnBullet(position, shipLinearVelocity, shipRotation);
	}

	private void OnBulletSignalCollided(Bullet bullet, Node collidedWith)
	{
		GD.Print("Missile collision flagged in main");
		_bulletController.KillBullet(bullet);
		if (collidedWith is Asteroid asteroid)
		{
			_score.Increase(100);
		}
	}

	private void OnAsteroidSignalCollided(Asteroid asteroid, AsteroidSize size, Node collidedWith)
	{
		GD.Print($"Asteroid collision in main size={size} collided with={collidedWith.GetType().Name}");
	}
}
