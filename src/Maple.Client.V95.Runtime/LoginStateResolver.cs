using Maple.Memory;

namespace Maple.Client.V95.Runtime;

/// <summary>
/// Resolves one composed snapshot of the active v95 login flow state.
/// </summary>
public sealed class LoginStateResolver
{
    private readonly FieldAccessor _fieldAccessor;
    private readonly SingletonResolver _singletonResolver;
    private readonly LoginSelectionResolver _loginSelectionResolver;

    /// <summary>
    /// Creates a login-state resolver over <paramref name="memoryAccessor"/>.
    /// </summary>
    public LoginStateResolver(MemoryAccessor memoryAccessor, StructFieldRegistry? registry = null)
    {
        ArgumentNullException.ThrowIfNull(memoryAccessor);
        _fieldAccessor = new FieldAccessor(memoryAccessor, registry);
        _singletonResolver = new SingletonResolver(memoryAccessor);
        _loginSelectionResolver = new LoginSelectionResolver(_fieldAccessor, _singletonResolver);
    }

    /// <summary>
    /// Creates a login-state resolver over existing field, singleton, and selection helpers.
    /// </summary>
    public LoginStateResolver(
        FieldAccessor fieldAccessor,
        SingletonResolver singletonResolver,
        LoginSelectionResolver? loginSelectionResolver = null
    )
    {
        _fieldAccessor = fieldAccessor ?? throw new ArgumentNullException(nameof(fieldAccessor));
        _singletonResolver = singletonResolver ?? throw new ArgumentNullException(nameof(singletonResolver));
        _loginSelectionResolver =
            loginSelectionResolver ?? new LoginSelectionResolver(fieldAccessor, singletonResolver);
    }

    /// <summary>
    /// Attempts to resolve the current login flow state without string decoding.
    /// </summary>
    public bool TryResolveRaw(out RawLoginState state)
    {
        try
        {
            if (!TryResolveLoginAddress(out uint loginAddress))
            {
                state = default;
                return false;
            }

            int? contextWorldId = null;
            int? contextChannelId = null;
            int? contextCharacterId = null;
            if (_singletonResolver.TryResolveContext(out uint cwvsContextAddress) && cwvsContextAddress != 0)
                TryResolveContext(cwvsContextAddress, out contextWorldId, out contextChannelId, out contextCharacterId);

            state = new RawLoginState(
                loginAddress,
                _fieldAccessor.ReadLoginStep(loginAddress),
                _fieldAccessor.ReadStepChanging(loginAddress),
                _fieldAccessor.ReadRequestSent(loginAddress),
                _fieldAccessor.ReadLoginWorldItem(loginAddress),
                contextWorldId,
                contextChannelId,
                contextCharacterId,
                TryResolveWorldSelection(),
                TryResolveChannelSelection()
            );

            return true;
        }
        catch (InvalidOperationException)
        {
            state = default;
            return false;
        }
    }

    /// <summary>
    /// Resolves the current login flow state without string decoding or throws if no active login UI can provide <c>CLogin</c>.
    /// </summary>
    public RawLoginState ResolveRaw()
    {
        if (TryResolveRaw(out RawLoginState state))
            return state;

        throw new InvalidOperationException(
            "Unable to resolve active CLogin state from the known v95 login UI singletons."
        );
    }

    /// <summary>
    /// Attempts to resolve the current login flow state.
    /// </summary>
    public bool TryResolve(out ResolvedLoginState state)
    {
        if (!TryResolveRaw(out RawLoginState rawState))
        {
            state = default;
            return false;
        }

        state = Decode(rawState);
        return true;
    }

    /// <summary>
    /// Resolves the current login flow state or throws if no active login UI can provide <c>CLogin</c>.
    /// </summary>
    public ResolvedLoginState Resolve()
    {
        if (TryResolve(out ResolvedLoginState state))
            return state;

        throw new InvalidOperationException(
            "Unable to resolve active CLogin state from the known v95 login UI singletons."
        );
    }

    /// <summary>
    /// Decodes one previously read raw login-state snapshot.
    /// </summary>
    public ResolvedLoginState Decode(RawLoginState state) => _fieldAccessor.DecodeLoginState(state);

    private bool TryResolveLoginAddress(out uint loginAddress)
    {
        if (_singletonResolver.TryResolveWorldSelect(out uint worldSelectAddress) && worldSelectAddress != 0)
        {
            loginAddress = _fieldAccessor.ReadWorldSelectLogin(worldSelectAddress);
            if (loginAddress != 0)
                return true;
        }

        if (_singletonResolver.TryResolveChannelSelect(out uint channelSelectAddress) && channelSelectAddress != 0)
        {
            loginAddress = _fieldAccessor.ReadChannelSelectLogin(channelSelectAddress);
            if (loginAddress != 0)
                return true;
        }

        loginAddress = 0;
        return false;
    }

    private RawWorldSelection? TryResolveWorldSelection()
    {
        try
        {
            if (!_singletonResolver.TryResolveWorldSelect(out uint worldSelectAddress) || worldSelectAddress == 0)
                return null;

            if (_fieldAccessor.ReadWorldSelectWorldItem(worldSelectAddress) == 0)
                return null;

            return _loginSelectionResolver.TryResolveRawWorldSelection(
                worldSelectAddress,
                out RawWorldSelection selection
            )
                ? selection
                : null;
        }
        catch (InvalidOperationException)
        {
            return null;
        }
    }

    private RawChannelSelection? TryResolveChannelSelection()
    {
        try
        {
            if (!_singletonResolver.TryResolveChannelSelect(out uint channelSelectAddress) || channelSelectAddress == 0)
                return null;

            if (_fieldAccessor.ReadChannelSelectWorldItem(channelSelectAddress) == 0)
                return null;

            return _loginSelectionResolver.TryResolveRawChannelSelection(
                channelSelectAddress,
                out RawChannelSelection selection
            )
                ? selection
                : null;
        }
        catch (InvalidOperationException)
        {
            return null;
        }
    }

    private bool TryResolveContext(uint cwvsContextAddress, out int? worldId, out int? channelId, out int? characterId)
    {
        try
        {
            worldId = _fieldAccessor.ReadWorldId(cwvsContextAddress);
            channelId = _fieldAccessor.ReadChannelId(cwvsContextAddress);
            characterId = _fieldAccessor.ReadCharacterId(cwvsContextAddress);
            return true;
        }
        catch (InvalidOperationException)
        {
            worldId = null;
            channelId = null;
            characterId = null;
            return false;
        }
    }
}
