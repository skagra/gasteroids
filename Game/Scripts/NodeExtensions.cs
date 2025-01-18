using Godot;
using static Godot.Node;

namespace Asteroids;

public static class GodotExtensions
{
    public static void Enable(this Area2D area2D, bool enable)
    {
        ((Node)area2D).Enable(enable);
        area2D.Monitorable = enable;
        area2D.Monitoring = enable;
    }

    public static void EnableDeferred(this Area2D area2D, bool enable)
    {
        ((Node)area2D).EnableDeferred(enable);
        area2D.SetDeferred(Area2D.PropertyName.Monitorable, enable);
        area2D.SetDeferred(Area2D.PropertyName.Monitoring, enable);
    }

    public static void Enable(this Node node, bool enable)
    {
        node.ProcessMode = enable ? ProcessModeEnum.Inherit : ProcessModeEnum.Disabled;
    }

    public static void EnableDeferred(this Node node, bool enable)
    {
        node.SetDeferred(Node.PropertyName.ProcessMode, enable ? (int)ProcessModeEnum.Inherit : (int)ProcessModeEnum.Disabled);
    }
}