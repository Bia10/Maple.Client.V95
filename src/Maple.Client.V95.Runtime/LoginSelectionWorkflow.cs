using Maple.Memory;

namespace Maple.Client.V95.Runtime;

/// <summary>
/// Coordinates guarded world and channel selection updates for the v95 login flow.
/// </summary>
/// <remarks>
/// This is the first workflow-oriented layer above the raw field and selection resolvers.
/// It deliberately stays narrow: validate the current login state, ensure the relevant UI
/// singletons are active, apply the existing selection writes, and return a fresh login snapshot.
/// </remarks>
public sealed class LoginSelectionWorkflow
{
    private readonly LoginStateResolver _loginStateResolver;
    private readonly LoginSelectionResolver _loginSelectionResolver;
    private readonly SingletonResolver _singletonResolver;

    /// <summary>
    /// Creates a guarded login-selection workflow over <paramref name="memoryAccessor"/>.
    /// </summary>
    public LoginSelectionWorkflow(MemoryAccessor memoryAccessor, StructFieldRegistry? registry = null)
    {
        ArgumentNullException.ThrowIfNull(memoryAccessor);

        var fieldAccessor = new FieldAccessor(memoryAccessor, registry);
        _singletonResolver = new SingletonResolver(memoryAccessor);
        _loginStateResolver = new LoginStateResolver(fieldAccessor, _singletonResolver);
        _loginSelectionResolver = new LoginSelectionResolver(fieldAccessor, _singletonResolver);
    }

    /// <summary>
    /// Creates a guarded login-selection workflow over existing helpers.
    /// </summary>
    public LoginSelectionWorkflow(
        LoginStateResolver loginStateResolver,
        LoginSelectionResolver loginSelectionResolver,
        SingletonResolver singletonResolver
    )
    {
        _loginStateResolver = loginStateResolver ?? throw new ArgumentNullException(nameof(loginStateResolver));
        _loginSelectionResolver =
            loginSelectionResolver ?? throw new ArgumentNullException(nameof(loginSelectionResolver));
        _singletonResolver = singletonResolver ?? throw new ArgumentNullException(nameof(singletonResolver));
    }

    /// <summary>
    /// Selects one world after validating that the login flow is in a mutable world-selection state.
    /// </summary>
    public ResolvedLoginState SelectWorld(uint worldItemAddress, int worldIndex)
    {
        ArgumentOutOfRangeException.ThrowIfZero(worldItemAddress);

        ArgumentOutOfRangeException.ThrowIfNegative(worldIndex);

        if (
            !TrySelectWorld(
                worldItemAddress,
                worldIndex,
                out ResolvedLoginState state,
                out LoginSelectionFailure failure
            )
        )
            throw CreateSelectionFailureException(failure, state.Step, "select a world");

        return state;
    }

    /// <summary>
    /// Attempts to select one world without allocating exceptions for guarded failure paths.
    /// </summary>
    public bool TrySelectWorld(
        uint worldItemAddress,
        int worldIndex,
        out ResolvedLoginState state,
        out LoginSelectionFailure failure
    )
    {
        bool selected = TrySelectWorldRaw(worldItemAddress, worldIndex, out RawLoginState rawState, out failure);
        state = rawState.IsResolved ? _loginStateResolver.Decode(rawState) : default;
        return selected;
    }

    /// <summary>
    /// Selects one world and returns the raw login snapshot after the write.
    /// </summary>
    public RawLoginState SelectWorldRaw(uint worldItemAddress, int worldIndex)
    {
        ArgumentOutOfRangeException.ThrowIfZero(worldItemAddress);

        ArgumentOutOfRangeException.ThrowIfNegative(worldIndex);

        if (
            !TrySelectWorldRaw(worldItemAddress, worldIndex, out RawLoginState state, out LoginSelectionFailure failure)
        )
            throw CreateSelectionFailureException(failure, state.Step, "select a world");

        return state;
    }

    /// <summary>
    /// Attempts to select one world without string decoding.
    /// </summary>
    public bool TrySelectWorldRaw(
        uint worldItemAddress,
        int worldIndex,
        out RawLoginState state,
        out LoginSelectionFailure failure
    )
    {
        ArgumentOutOfRangeException.ThrowIfZero(worldItemAddress);

        ArgumentOutOfRangeException.ThrowIfNegative(worldIndex);

        if (!TryEnsureSelectableState(requireWorldSelect: true, requireChannelSelect: false, out state, out failure))
            return false;

        if (!_loginSelectionResolver.TryResolveRawWorldItem(worldItemAddress, out _))
        {
            failure = LoginSelectionFailure.SelectionTargetUnavailable;
            return false;
        }

        return TryApplySelectionChange(
            state,
            out state,
            out failure,
            () => _loginSelectionResolver.WriteWorldSelection(worldItemAddress, worldIndex)
        );
    }

    /// <summary>
    /// Selects one channel for the currently selected world.
    /// </summary>
    public ResolvedLoginState SelectChannel(int channelIndex)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(channelIndex);

        if (!TrySelectChannel(channelIndex, out ResolvedLoginState state, out LoginSelectionFailure failure))
            throw CreateSelectionFailureException(failure, state.Step, "select a channel");

        return state;
    }

    /// <summary>
    /// Attempts to select one channel without allocating exceptions for guarded failure paths.
    /// </summary>
    public bool TrySelectChannel(int channelIndex, out ResolvedLoginState state, out LoginSelectionFailure failure)
    {
        bool selected = TrySelectChannelRaw(channelIndex, out RawLoginState rawState, out failure);
        state = rawState.IsResolved ? _loginStateResolver.Decode(rawState) : default;
        return selected;
    }

    /// <summary>
    /// Selects one channel and returns the raw login snapshot after the write.
    /// </summary>
    public RawLoginState SelectChannelRaw(int channelIndex)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(channelIndex);

        if (!TrySelectChannelRaw(channelIndex, out RawLoginState state, out LoginSelectionFailure failure))
            throw CreateSelectionFailureException(failure, state.Step, "select a channel");

        return state;
    }

    /// <summary>
    /// Attempts to select one channel without string decoding.
    /// </summary>
    public bool TrySelectChannelRaw(int channelIndex, out RawLoginState state, out LoginSelectionFailure failure)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(channelIndex);

        if (!TryEnsureSelectableState(requireWorldSelect: false, requireChannelSelect: true, out state, out failure))
            return false;

        if (state.SelectedWorldItemAddress == 0)
        {
            failure = LoginSelectionFailure.MissingSelectedWorld;
            return false;
        }

        if (!_loginSelectionResolver.TryResolveRawChannelItem(state.SelectedWorldItemAddress, channelIndex, out _))
        {
            failure = LoginSelectionFailure.SelectionTargetUnavailable;
            return false;
        }

        uint selectedWorldItemAddress = state.SelectedWorldItemAddress;

        return TryApplySelectionChange(
            state,
            out state,
            out failure,
            () => _loginSelectionResolver.WriteChannelSelection(selectedWorldItemAddress, channelIndex)
        );
    }

    /// <summary>
    /// Selects one world and then one channel under the same guarded login-state preflight.
    /// </summary>
    public ResolvedLoginState SelectWorldAndChannel(uint worldItemAddress, int worldIndex, int channelIndex)
    {
        ArgumentOutOfRangeException.ThrowIfZero(worldItemAddress);
        ArgumentOutOfRangeException.ThrowIfNegative(worldIndex);
        ArgumentOutOfRangeException.ThrowIfNegative(channelIndex);

        if (
            !TrySelectWorldAndChannel(
                worldItemAddress,
                worldIndex,
                channelIndex,
                out ResolvedLoginState state,
                out LoginSelectionFailure failure
            )
        )
            throw CreateSelectionFailureException(failure, state.Step, "select a world and channel");

        return state;
    }

    /// <summary>
    /// Attempts to select one world and then one channel under the same guarded preflight.
    /// </summary>
    public bool TrySelectWorldAndChannel(
        uint worldItemAddress,
        int worldIndex,
        int channelIndex,
        out ResolvedLoginState state,
        out LoginSelectionFailure failure
    )
    {
        bool selected = TrySelectWorldAndChannelRaw(
            worldItemAddress,
            worldIndex,
            channelIndex,
            out RawLoginState rawState,
            out failure
        );
        state = rawState.IsResolved ? _loginStateResolver.Decode(rawState) : default;
        return selected;
    }

    /// <summary>
    /// Selects one world and then one channel and returns the raw login snapshot after the writes.
    /// </summary>
    public RawLoginState SelectWorldAndChannelRaw(uint worldItemAddress, int worldIndex, int channelIndex)
    {
        ArgumentOutOfRangeException.ThrowIfZero(worldItemAddress);
        ArgumentOutOfRangeException.ThrowIfNegative(worldIndex);
        ArgumentOutOfRangeException.ThrowIfNegative(channelIndex);

        if (
            !TrySelectWorldAndChannelRaw(
                worldItemAddress,
                worldIndex,
                channelIndex,
                out RawLoginState state,
                out LoginSelectionFailure failure
            )
        )
            throw CreateSelectionFailureException(failure, state.Step, "select a world and channel");

        return state;
    }

    /// <summary>
    /// Attempts to select one world and then one channel without string decoding.
    /// </summary>
    public bool TrySelectWorldAndChannelRaw(
        uint worldItemAddress,
        int worldIndex,
        int channelIndex,
        out RawLoginState state,
        out LoginSelectionFailure failure
    )
    {
        ArgumentOutOfRangeException.ThrowIfZero(worldItemAddress);
        ArgumentOutOfRangeException.ThrowIfNegative(worldIndex);
        ArgumentOutOfRangeException.ThrowIfNegative(channelIndex);

        if (!TryEnsureSelectableState(requireWorldSelect: true, requireChannelSelect: true, out state, out failure))
            return false;

        if (!_loginSelectionResolver.TryResolveRawWorldItem(worldItemAddress, out _))
        {
            failure = LoginSelectionFailure.SelectionTargetUnavailable;
            return false;
        }

        if (!_loginSelectionResolver.TryResolveRawChannelItem(worldItemAddress, channelIndex, out _))
        {
            failure = LoginSelectionFailure.SelectionTargetUnavailable;
            return false;
        }

        return TryApplySelectionChange(
            state,
            out state,
            out failure,
            () =>
            {
                _loginSelectionResolver.WriteWorldSelection(worldItemAddress, worldIndex);
                _loginSelectionResolver.WriteChannelSelection(worldItemAddress, channelIndex);
            }
        );
    }

    private bool TryEnsureSelectableState(
        bool requireWorldSelect,
        bool requireChannelSelect,
        out RawLoginState state,
        out LoginSelectionFailure failure
    )
    {
        if (!_loginStateResolver.TryResolveRaw(out state))
        {
            failure = LoginSelectionFailure.LoginStateUnavailable;
            return false;
        }

        if (state.Step != LoginStep.SelectWorld)
        {
            failure = LoginSelectionFailure.InvalidLoginStep;
            return false;
        }

        if (state.StepChanging)
        {
            failure = LoginSelectionFailure.StepChanging;
            return false;
        }

        if (state.RequestSent)
        {
            failure = LoginSelectionFailure.RequestSent;
            return false;
        }

        if (requireWorldSelect && !_singletonResolver.HasActiveWorldSelect())
        {
            failure = LoginSelectionFailure.WorldSelectInactive;
            return false;
        }

        if (requireChannelSelect && !_singletonResolver.HasActiveChannelSelect())
        {
            failure = LoginSelectionFailure.ChannelSelectInactive;
            return false;
        }

        failure = LoginSelectionFailure.None;
        return true;
    }

    private bool TryApplySelectionChange(
        RawLoginState fallbackState,
        out RawLoginState state,
        out LoginSelectionFailure failure,
        Action applySelection
    )
    {
        try
        {
            applySelection();
        }
        catch (InvalidOperationException)
        {
            state = ResolveCurrentStateOrFallback(fallbackState);
            failure = LoginSelectionFailure.LoginStateUnavailable;
            return false;
        }

        return TryResolvePostSelectionState(fallbackState, out state, out failure);
    }

    private bool TryResolvePostSelectionState(
        RawLoginState fallbackState,
        out RawLoginState state,
        out LoginSelectionFailure failure
    )
    {
        if (_loginStateResolver.TryResolveRaw(out state))
        {
            failure = LoginSelectionFailure.None;
            return true;
        }

        state = fallbackState;
        failure = LoginSelectionFailure.LoginStateUnavailable;
        return false;
    }

    private RawLoginState ResolveCurrentStateOrFallback(RawLoginState fallbackState) =>
        _loginStateResolver.TryResolveRaw(out RawLoginState currentState) ? currentState : fallbackState;

    private static Exception CreateSelectionFailureException(
        LoginSelectionFailure failure,
        LoginStep step,
        string actionDescription
    ) =>
        failure switch
        {
            LoginSelectionFailure.LoginStateUnavailable => new InvalidOperationException(
                "Unable to resolve active CLogin state from the known v95 login UI singletons."
            ),
            LoginSelectionFailure.InvalidLoginStep => new InvalidOperationException(
                $"Cannot {actionDescription} while login step is {step}; expected {LoginStep.SelectWorld}."
            ),
            LoginSelectionFailure.StepChanging => new InvalidOperationException(
                $"Cannot {actionDescription} while CLogin::m_bStepChanging is set."
            ),
            LoginSelectionFailure.RequestSent => new InvalidOperationException(
                $"Cannot {actionDescription} after CLogin::m_bRequestSent is set."
            ),
            LoginSelectionFailure.WorldSelectInactive => new InvalidOperationException(
                $"Cannot {actionDescription} because CUIWorldSelect is not active."
            ),
            LoginSelectionFailure.ChannelSelectInactive => new InvalidOperationException(
                $"Cannot {actionDescription} because CUIChannelSelect is not active."
            ),
            LoginSelectionFailure.MissingSelectedWorld => new InvalidOperationException(
                "Cannot select a channel when CLogin::m_pWorldItem is not set."
            ),
            LoginSelectionFailure.SelectionTargetUnavailable => new InvalidOperationException(
                $"Cannot {actionDescription} because the requested world or channel target is not readable."
            ),
            _ => new InvalidOperationException(
                $"Cannot {actionDescription} due to an unknown guarded workflow failure."
            ),
        };
}
