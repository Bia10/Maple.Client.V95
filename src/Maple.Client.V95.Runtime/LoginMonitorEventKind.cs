namespace Maple.Client.V95.Runtime;

/// <summary>
/// Describes one stage in the shared login-monitor polling lifecycle.
/// </summary>
public enum LoginMonitorEventKind
{
    Started = 0,
    Polling,
    Matched,
    TimedOut,
}
