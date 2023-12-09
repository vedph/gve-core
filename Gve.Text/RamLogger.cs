using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Gve.Text;

/// <summary>
/// RAM-based logger essential implementation. Note that this does not honor
/// scopes, but it is thread-safe.
/// </summary>
/// <remarks>Usage:
/// <code>services.AddSingleton&lt;ILogger&gt;(
///   new RamLogger(LogLevel.Information));</code>
/// </remarks>
/// <seealso cref="ILogger" />
public class RamLogger : ILogger
{
    private readonly LogLevel _minLevel;
    private readonly ConcurrentQueue<string> _logs;

    /// <summary>
    /// Initializes a new instance of the <see cref="RamLogger"/> class.
    /// </summary>
    /// <param name="minLevel">The minimum level.</param>
    public RamLogger(LogLevel minLevel = LogLevel.Information)
    {
        _minLevel = minLevel;
        _logs = new();
    }

    /// <summary>
    /// Begins a logical operation scope.
    /// </summary>
    /// <typeparam name="TState">The type of the state to begin scope for.</typeparam>
    /// <param name="state">The identifier for the scope.</param>
    /// <returns>
    /// An <see cref="T:System.IDisposable" /> that ends the logical operation
    /// scope on dispose.
    /// </returns>
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        => null;

    /// <summary>
    /// Checks if the given <paramref name="logLevel" /> is enabled.
    /// </summary>
    /// <param name="logLevel">Level to be checked.</param>
    /// <returns>
    ///   <c>true</c> if enabled.
    /// </returns>
    public bool IsEnabled(LogLevel logLevel) => logLevel >= _minLevel;

    /// <summary>
    /// Writes a log entry.
    /// </summary>
    /// <typeparam name="TState">The type of the object to be written.</typeparam>
    /// <param name="logLevel">Entry will be written on this level.</param>
    /// <param name="eventId">Id of the event.</param>
    /// <param name="state">The entry to be written. Can be also an object.</param>
    /// <param name="exception">The exception related to this entry.</param>
    /// <param name="formatter">Function to create a <see cref="string" />
    /// message of the <paramref name="state" /> and <paramref name="exception" />.
    /// </param>
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state,
        Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }

        string message = formatter(state, exception);
        _logs.Enqueue(message);
    }

    /// <summary>
    /// Clears all the entries collected in this logger.
    /// </summary>
    public void Clear() => _logs.Clear();

    /// <summary>
    /// Gets the entries clearing the log.
    /// </summary>
    /// <returns>Entries.</returns>
    public IList<string> GetEntries()
    {
        List<string> entries = [];
        while (_logs.TryDequeue(out string? entry))
        {
            entries.Add(entry);
        }
        return entries;
    }
}