namespace Maple.Client.V95.Runtime;

/// <summary>
/// Receives shared login-monitor lifecycle events.
/// </summary>
public interface ILoginMonitorSink
{
    /// <summary>
    /// Handles one emitted login-monitor event.
    /// </summary>
    ValueTask OnEventAsync(LoginMonitorEvent monitorEvent, CancellationToken cancellationToken);
}
