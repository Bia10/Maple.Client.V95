namespace Maple.Client.V95.Runtime;

/// <summary>
/// Stable context snapshot for one login-monitor polling operation.
/// </summary>
/// <remarks>
/// Creates one login-monitor context snapshot.
/// </remarks>
public sealed class LoginMonitorContext(
    string description,
    TimeSpan timeout,
    TimeSpan pollingInterval,
    int attemptCount,
    TimeSpan elapsed,
    bool stateResolved,
    RawLoginState? rawState
)
{
    /// <summary>
    /// Gets the human-readable wait description.
    /// </summary>
    public string Description { get; } = description ?? throw new ArgumentNullException(nameof(description));

    /// <summary>
    /// Gets the total timeout configured for the wait.
    /// </summary>
    public TimeSpan Timeout { get; } = timeout;

    /// <summary>
    /// Gets the polling interval configured for the wait.
    /// </summary>
    public TimeSpan PollingInterval { get; } = pollingInterval;

    /// <summary>
    /// Gets the 1-based poll attempt count reached for this snapshot.
    /// </summary>
    public int AttemptCount { get; } = attemptCount;

    /// <summary>
    /// Gets the elapsed time at the moment the snapshot was created.
    /// </summary>
    public TimeSpan Elapsed { get; } = elapsed;

    /// <summary>
    /// Gets whether one raw login state could be resolved for this poll attempt.
    /// </summary>
    public bool StateResolved { get; } = stateResolved;

    /// <summary>
    /// Gets the resolved raw state when one was available for this snapshot.
    /// </summary>
    public RawLoginState? RawState { get; } = rawState;
}
