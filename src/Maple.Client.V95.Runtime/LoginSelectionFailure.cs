namespace Maple.Client.V95.Runtime;

/// <summary>
/// Describes why a guarded login-selection workflow step could not be performed.
/// </summary>
public enum LoginSelectionFailure
{
    /// <summary>The workflow step succeeded.</summary>
    None = 0,

    /// <summary>No active login state could be resolved from the known UI singletons.</summary>
    LoginStateUnavailable,

    /// <summary>The current login step is not the world-selection step required for mutation.</summary>
    InvalidLoginStep,

    /// <summary>The login flow is currently transitioning between steps.</summary>
    StepChanging,

    /// <summary>A login-related request was already sent, so local selection mutation is no longer safe.</summary>
    RequestSent,

    /// <summary>The world-selection UI singleton is not active.</summary>
    WorldSelectInactive,

    /// <summary>The channel-selection UI singleton is not active.</summary>
    ChannelSelectInactive,

    /// <summary>No selected world item is available for a channel-selection update.</summary>
    MissingSelectedWorld,

    /// <summary>The requested world or channel target could not be read safely before mutation.</summary>
    SelectionTargetUnavailable,
}
