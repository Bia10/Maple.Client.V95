namespace Maple.Client.V95.Runtime;

/// <summary>
/// Stable context snapshot for one login-monitor polling operation.
/// </summary>
public sealed class LoginMonitorContext
{
    /// <summary>
    /// Creates one login-monitor context snapshot.
    /// </summary>
    public LoginMonitorContext(
        string description,
        TimeSpan timeout,
        TimeSpan pollingInterval,
        int attemptCount,
        TimeSpan elapsed,
        bool stateResolved,
        RawLoginState? rawState
    )
    {
        Description = description ?? throw new ArgumentNullException(nameof(description));
        Timeout = timeout;
        PollingInterval = pollingInterval;
        AttemptCount = attemptCount;
        Elapsed = elapsed;
        StateResolved = stateResolved;
        RawState = rawState;
    }

    /// <summary>
    /// Gets the human-readable wait description.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Gets the total timeout configured for the wait.
    /// </summary>
    public TimeSpan Timeout { get; }

    /// <summary>
    /// Gets the polling interval configured for the wait.
    /// </summary>
    public TimeSpan PollingInterval { get; }

    /// <summary>
    /// Gets the 1-based poll attempt count reached for this snapshot.
    /// </summary>
    public int AttemptCount { get; }

    /// <summary>
    /// Gets the elapsed time at the moment the snapshot was created.
    /// </summary>
    public TimeSpan Elapsed { get; }

    /// <summary>
    /// Gets whether one raw login state could be resolved for this poll attempt.
    /// </summary>
    public bool StateResolved { get; }

    /// <summary>
    /// Gets the resolved raw state when one was available for this snapshot.
    /// </summary>
    public RawLoginState? RawState { get; }
}
