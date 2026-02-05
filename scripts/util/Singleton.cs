using Godot;

namespace RealMK;

/// <summary>
/// Base class for singleton autoload nodes.
/// Handles the singleton pattern boilerplate automatically.
/// Derived classes can override OnReady() instead of _Ready().
/// </summary>
public abstract partial class Singleton<T> : Node where T : Node {
    public static T Instance { get; private set; } = null!;
    
    public override void _EnterTree() {
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (Instance is not null && !Instance.Equals(this)) {
            Log.Warning($"Multiple {typeof(T).Name} instances detected. Removing duplicate.");
            QueueFree();
            return;
        }
        
        Instance = (T)(object)this;
    }
    
    public override void _ExitTree() {
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (Instance is not null && Instance.Equals(this)) {
            Instance = null!;
        }
    }
}