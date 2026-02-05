using System;
using System.Collections.Generic;

namespace RealMK;

/// <summary>
/// Event bus for publishing and subscribing to game events.
/// Provides decoupled communication between game systems.
/// </summary>
public sealed class EventBus
{
    private readonly Dictionary<Type, List<Delegate>> _handlers = new();
    private readonly object _lock = new();

    /// <summary>
    /// Subscribe to events of a specific type.
    /// </summary>
    /// <typeparam name="TEvent">Type of event to subscribe to.</typeparam>
    /// <param name="handler">Handler to invoke when event is published.</param>
    public void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : IGameEvent
    {
        ArgumentNullException.ThrowIfNull(handler);

        lock (_lock)
        {
            Type eventType = typeof(TEvent);
            if (!_handlers.TryGetValue(eventType, out List<Delegate>? handlers))
            {
                handlers = new List<Delegate>();
                _handlers[eventType] = handlers;
            }
            handlers.Add(handler);
        }
    }

    /// <summary>
    /// Unsubscribe from events of a specific type.
    /// </summary>
    /// <typeparam name="TEvent">Type of event to unsubscribe from.</typeparam>
    /// <param name="handler">Handler to remove.</param>
    /// <returns>True if handler was found and removed.</returns>
    public bool Unsubscribe<TEvent>(Action<TEvent> handler) where TEvent : IGameEvent
    {
        ArgumentNullException.ThrowIfNull(handler);

        lock (_lock)
        {
            Type eventType = typeof(TEvent);
            if (_handlers.TryGetValue(eventType, out List<Delegate>? handlers))
            {
                return handlers.Remove(handler);
            }
            return false;
        }
    }

    /// <summary>
    /// Publish an event to all registered handlers.
    /// </summary>
    /// <param name="gameEvent">Event to publish.</param>
    public void Publish(IGameEvent gameEvent)
    {
        ArgumentNullException.ThrowIfNull(gameEvent);

        List<Delegate>? handlersCopy;
        lock (_lock)
        {
            Type eventType = gameEvent.GetType();
            if (!_handlers.TryGetValue(eventType, out List<Delegate>? handlers))
            {
                return;
            }
            // Copy to avoid issues if handlers modify subscriptions
            handlersCopy = new List<Delegate>(handlers);
        }

        foreach (Delegate handler in handlersCopy)
        {
            handler.DynamicInvoke(gameEvent);
        }
    }

    /// <summary>
    /// Publish multiple events in order.
    /// </summary>
    /// <param name="events">Events to publish.</param>
    public void PublishAll(IEnumerable<IGameEvent> events)
    {
        foreach (IGameEvent gameEvent in events)
        {
            Publish(gameEvent);
        }
    }

    /// <summary>
    /// Clear all subscriptions.
    /// </summary>
    public void Clear()
    {
        lock (_lock)
        {
            _handlers.Clear();
        }
    }

    /// <summary>
    /// Get the number of handlers subscribed to a specific event type.
    /// </summary>
    /// <typeparam name="TEvent">Type of event.</typeparam>
    /// <returns>Number of subscribers.</returns>
    public int GetSubscriberCount<TEvent>() where TEvent : IGameEvent
    {
        lock (_lock)
        {
            Type eventType = typeof(TEvent);
            if (_handlers.TryGetValue(eventType, out List<Delegate>? handlers))
            {
                return handlers.Count;
            }
            return 0;
        }
    }

    /// <summary>
    /// Check if there are any subscribers for a specific event type.
    /// </summary>
    /// <typeparam name="TEvent">Type of event.</typeparam>
    /// <returns>True if there are subscribers.</returns>
    public bool HasSubscribers<TEvent>() where TEvent : IGameEvent
    {
        return GetSubscriberCount<TEvent>() > 0;
    }
}
