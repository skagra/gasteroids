using Asteroids;
using Godot;

namespace Asteroids;

public static class Identities
{
    public static bool IsPlayerMissile(Node node)
    {
        return node is Missile missile && missile.GetParent()?.GetParent() is PlayerController;
    }

    public static bool IsPlayer(Node node)
    {
        return node is Player;
    }
}