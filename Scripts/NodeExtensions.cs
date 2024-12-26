namespace Godot;

public static class GodotExtensions
{
    public static void Enable(this Area2D area2D, bool enable)
    {
        area2D.SetProcess(enable);
        area2D.SetPhysicsProcess(enable);
        area2D.SetProcessInput(enable);
        area2D.Monitorable = enable;
        area2D.Monitoring = enable;
    }

    public static void EnableDeferred(this Area2D area2D, bool enable)
    {
        area2D.CallDeferred(Area2D.MethodName.SetProcess, enable);
        area2D.CallDeferred(Area2D.MethodName.SetPhysicsProcess, enable);
        area2D.CallDeferred(Area2D.MethodName.SetProcessInput, enable);
        area2D.SetDeferred(Area2D.PropertyName.Monitorable, enable);
        area2D.SetDeferred(Area2D.PropertyName.Monitoring, enable);
    }

    public static void Enable(this Node node, bool enable)
    {
        node.SetProcess(enable);
        node.SetPhysicsProcess(enable);
        node.SetProcessInput(enable);
    }
    public static void EnableDeferred(this Node node, bool enable)
    {
        node.CallDeferred(Area2D.MethodName.SetProcess, enable);
        node.CallDeferred(Area2D.MethodName.SetPhysicsProcess, enable);
        node.CallDeferred(Area2D.MethodName.SetProcessInput, enable);
    }
}