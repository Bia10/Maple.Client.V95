namespace Maple.Client.V95.Runtime;

/// <summary>
/// One emitted login-monitor lifecycle event.
/// </summary>
/// <remarks>
/// Creates one login-monitor lifecycle event.
/// </remarks>
public sealed class LoginMonitorEvent(LoginMonitorEventKind kind, LoginMonitorContext context)
{
    /// <summary>
    /// Gets the lifecycle stage being reported.
    /// </summary>
    public LoginMonitorEventKind Kind { get; } = kind;

    /// <summary>
    /// Gets the stable context snapshot for this event.
    /// </summary>
    public LoginMonitorContext Context { get; } = context ?? throw new ArgumentNullException(nameof(context));
}
