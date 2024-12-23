using Godot;

namespace Asteroids;

public partial class Beats : Node
{
    // Values configuration via the inspector
    [Export]
    private AudioStream _beat1;
    [Export]
    private AudioStream _beat2;
    [Export]
    private float _initialGap = 1f;
    [Export]
    private float _gapDelta = 0.01f;
    [Export]
    private float _increaseSpeedThreshold = 5;
    [Export]
    private float _minGap = 0.1f;

    private float _currentGap;
    private bool _playing = false;
    private float _gapTimer;
    private float _increaseSpeedTimer;
    private AudioStreamPlayer2D _beatsAudioStream;
    private int _currentSample = 1;

    public override void _Ready()
    {
        _beatsAudioStream = new AudioStreamPlayer2D();
        AddChild(_beatsAudioStream);
    }

    private void PlaySample()
    {
        if (_currentSample == 1)
        {
            _beatsAudioStream.Stream = _beat2;
            _currentSample = 2;
        }
        else
        {
            _beatsAudioStream.Stream = _beat1;
            _currentSample = 1;
        }
        _beatsAudioStream.Play();
    }

    public void Stop()
    {
        _playing = false;
        SetPhysicsProcess(false);
    }

    public void Reset()
    {
        _currentGap = _initialGap;
        _increaseSpeedTimer = 0;
    }

    public void Start()
    {
        _playing = true;
        SetPhysicsProcess(true);
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_playing)
        {
            if (!_beatsAudioStream.Playing)
            {
                _increaseSpeedTimer += (float)delta;
                _gapTimer += (float)delta;
                if (_gapTimer > _currentGap)
                {
                    PlaySample();
                    _gapTimer = 0;

                    if (_increaseSpeedTimer > _increaseSpeedThreshold)
                    {
                        _increaseSpeedTimer = 0;
                        _currentGap -= _gapDelta;
                        _currentGap = Mathf.Max(_currentGap, _minGap);
                    }
                }
            }
        }
    }

}
