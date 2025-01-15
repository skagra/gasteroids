using Asteroids;
using Godot;

public static class Identities
{
    public static bool IsPlayerMissile(Node node)
    {
        return node is Missile missile && missile.GetParent()?.GetParent() is PlayerController;
    }
}