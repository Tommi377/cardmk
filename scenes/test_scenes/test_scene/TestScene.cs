using Godot;

namespace RealMK;

public partial class TestScene : Node2D
{
    public override void _Ready() {
        Log.Info("TestScene is ready!");
        Log.Debug("This is a debug message.");
        Log.Warning("This is a warning message.");
        Log.Error("This is an error message.");
    }
}