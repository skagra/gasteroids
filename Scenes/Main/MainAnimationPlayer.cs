using Godot;

namespace Asteroids;

public partial class MainAnimationPlayer : AnimationPlayer
{
    private const string _MAIN_LOOP_NAME = "Main Loop";
    private const string _GAME_OVER_NAME = "Game Over";
    private const string _DELAYED_MAIN_LOOP = "Delayed Main Loop";
    private const string _SPLASH = "Splash";

    public void PlayMainLoop()
    {
        Play(_MAIN_LOOP_NAME);
    }

    public void PlayGameOver()
    {
        Play(_GAME_OVER_NAME);
    }

    public void PlayDelayedMainLoop()
    {
        Play(_DELAYED_MAIN_LOOP);
    }

    public void PlaySplash()
    {
        Play(_SPLASH);
    }
}
