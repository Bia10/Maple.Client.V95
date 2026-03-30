namespace Maple.Client.V95.Runtime;

/// <summary>
/// One emitted login-monitor lifecycle event.
/// </summary>
public sealed class LoginMonitorEvent
{
    /// <summary>
    /// Creates one login-monitor lifecycle event.
    /// </summary>
    public LoginMonitorEvent(LoginMonitorEventKind kind, LoginMonitorContext context)
    {
        Kind = kind;
        Context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// Gets the lifecycle stage being reported.
    /// </summary>
    public LoginMonitorEventKind Kind { get; }

    /// <summary>
    /// Gets the stable context snapshot for this event.
    /// </summary>
    public LoginMonitorContext Context { get; }
}
