using System.Collections.Generic;
using Godot;

namespace Asteroids;

public partial class AudioHub : Node
{
    [Export]
    private int _poolSize = 10;

    private List<AudioStreamPlayer> _playerPool = new();

    public static AudioHub I { get; private set; }

    public HashSet<AudioStream> _playing = new();

    public override void _Ready()
    {
        I = this;

        for (var i = 0; i < _poolSize; i++)
        {
            var player = new AudioStreamPlayer
            {
                Bus = Resources.AUDIO_BUS_NAME_FX
            };
            player.Finished += () => OnFinished(player);
            _playerPool.Add(player);
            AddChild(player);
        }
    }

    private void OnFinished(AudioStreamPlayer player)
    {
        _playing.Remove(player.Stream);
        _playerPool.Add(player);
    }

    public void Play(AudioStream stream, bool singleton = false)
    {
        if (_playerPool.Count > 0 &&
            (!singleton || !_playing.Contains(stream)))
        {
            var player = _playerPool[0];
            player.Stream = stream;
            _playing.Add(stream);
            player.Play();
            _playerPool.RemoveAt(0);
        }
    }
}
