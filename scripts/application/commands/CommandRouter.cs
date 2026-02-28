using System;
using System.Collections.Generic;

namespace RealMK;

/// <summary>
/// In-memory command router based on runtime command type.
/// </summary>
public sealed class CommandRouter : ICommandRouter
{
    private readonly Dictionary<Type, IHandlerAdapter> _handlers = new();

    /// <summary>
    /// Registers a handler for a command type.
    /// </summary>
    public void Register<TCommand>(ICommandHandler<TCommand> handler) where TCommand : IGameCommand
    {
        ArgumentNullException.ThrowIfNull(handler);
        _handlers[typeof(TCommand)] = new HandlerAdapter<TCommand>(handler);
    }

    /// <inheritdoc/>
    public ValidationResult Validate(IGameCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (!_handlers.TryGetValue(command.GetType(), out IHandlerAdapter? handler))
        {
            return ValidationResult.Invalid("UNKNOWN_COMMAND", $"No handler registered for {command.GetType().Name}");
        }

        return handler.Validate(command);
    }

    /// <inheritdoc/>
    public IReadOnlyList<IGameEvent> Execute(IGameCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (!_handlers.TryGetValue(command.GetType(), out IHandlerAdapter? handler))
        {
            throw new InvalidOperationException($"No handler registered for {command.GetType().Name}");
        }

        return handler.Execute(command);
    }

    private interface IHandlerAdapter
    {
        ValidationResult Validate(IGameCommand command);
        IReadOnlyList<IGameEvent> Execute(IGameCommand command);
    }

    private sealed class HandlerAdapter<TCommand> : IHandlerAdapter where TCommand : IGameCommand
    {
        private readonly ICommandHandler<TCommand> _handler;

        public HandlerAdapter(ICommandHandler<TCommand> handler)
        {
            _handler = handler;
        }

        public ValidationResult Validate(IGameCommand command)
        {
            return _handler.Validate((TCommand)command);
        }

        public IReadOnlyList<IGameEvent> Execute(IGameCommand command)
        {
            return _handler.Execute((TCommand)command);
        }
    }
}
