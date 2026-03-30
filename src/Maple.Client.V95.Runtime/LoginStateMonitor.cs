using System.Diagnostics;
using Maple.Memory;

namespace Maple.Client.V95.Runtime;

/// <summary>
/// Polls the shared v95 login-state surface until one caller-defined condition is met.
/// </summary>
/// <remarks>
/// This type is intentionally narrower than Rue's plugin monitor. It owns only polling over
/// <see cref="LoginStateResolver"/> plus caller-supplied timeout and interval policy; logging,
/// bootstrap, and milestone tracking remain in Rue.
/// </remarks>
public sealed class LoginStateMonitor
{
    private readonly LoginStateResolver _loginStateResolver;
    private readonly Func<TimeSpan, CancellationToken, Task> _delayAsync;
    private readonly ILoginMonitorSink? _sink;

    /// <summary>
    /// Creates a login-state monitor over <paramref name="memoryAccessor"/>.
    /// </summary>
    public LoginStateMonitor(MemoryAccessor memoryAccessor, StructFieldRegistry? registry = null)
        : this(new LoginStateResolver(memoryAccessor, registry)) { }

    /// <summary>
    /// Creates a login-state monitor over an existing login-state resolver.
    /// </summary>
    public LoginStateMonitor(LoginStateResolver loginStateResolver)
        : this(
            loginStateResolver,
            sink: null,
            static (delay, cancellationToken) =>
                delay > TimeSpan.Zero ? Task.Delay(delay, cancellationToken) : Task.CompletedTask
        ) { }

    /// <summary>
    /// Creates a login-state monitor over an existing resolver and a shared lifecycle sink.
    /// </summary>
    public LoginStateMonitor(LoginStateResolver loginStateResolver, ILoginMonitorSink sink)
        : this(
            loginStateResolver,
            sink,
            static (delay, cancellationToken) =>
                delay > TimeSpan.Zero ? Task.Delay(delay, cancellationToken) : Task.CompletedTask
        ) { }

    internal LoginStateMonitor(
        LoginStateResolver loginStateResolver,
        ILoginMonitorSink? sink,
        Func<TimeSpan, CancellationToken, Task> delayAsync
    )
    {
        _loginStateResolver = loginStateResolver ?? throw new ArgumentNullException(nameof(loginStateResolver));
        _sink = sink;
        _delayAsync = delayAsync ?? throw new ArgumentNullException(nameof(delayAsync));
    }

    /// <summary>
    /// Waits until <paramref name="predicate"/> matches one raw login-state snapshot.
    /// </summary>
    public Task<RawLoginState> WaitForRawAsync(
        Predicate<RawLoginState> predicate,
        TimeSpan timeout,
        TimeSpan pollingInterval,
        CancellationToken cancellationToken = default
    ) =>
        WaitForRawCoreAsync(
            predicate,
            timeout,
            pollingInterval,
            "observe the requested raw login state",
            cancellationToken
        );

    /// <summary>
    /// Waits until <paramref name="predicate"/> matches one decoded login-state snapshot.
    /// </summary>
    public async Task<ResolvedLoginState> WaitForAsync(
        Predicate<ResolvedLoginState> predicate,
        TimeSpan timeout,
        TimeSpan pollingInterval,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(predicate);

        RawLoginState rawState = await WaitForRawCoreAsync(
                state => predicate(_loginStateResolver.Decode(state)),
                timeout,
                pollingInterval,
                "observe the requested login state",
                cancellationToken
            )
            .ConfigureAwait(false);

        return _loginStateResolver.Decode(rawState);
    }

    /// <summary>
    /// Waits until the login flow reaches <paramref name="step"/>.
    /// </summary>
    public Task<RawLoginState> WaitForStepAsync(
        LoginStep step,
        TimeSpan timeout,
        TimeSpan pollingInterval,
        CancellationToken cancellationToken = default
    ) =>
        WaitForRawCoreAsync(
            state => state.Step == step,
            timeout,
            pollingInterval,
            $"reach login step {step}",
            cancellationToken
        );

    /// <summary>
    /// Waits until the login flow reaches <paramref name="step"/> and step-transition work is no longer in progress.
    /// </summary>
    public Task<RawLoginState> WaitForStableStepAsync(
        LoginStep step,
        TimeSpan timeout,
        TimeSpan pollingInterval,
        CancellationToken cancellationToken = default
    ) =>
        WaitForRawCoreAsync(
            state => state.Step == step && !state.StepChanging,
            timeout,
            pollingInterval,
            $"reach stable login step {step}",
            cancellationToken
        );

    private async Task<RawLoginState> WaitForRawCoreAsync(
        Predicate<RawLoginState> predicate,
        TimeSpan timeout,
        TimeSpan pollingInterval,
        string description,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(predicate);
        ValidateWaitArguments(timeout, pollingInterval);

        var stopwatch = Stopwatch.StartNew();
        int attemptCount = 0;

        await EmitAsync(
                LoginMonitorEventKind.Started,
                new LoginMonitorContext(description, timeout, pollingInterval, 0, TimeSpan.Zero, false, null),
                cancellationToken
            )
            .ConfigureAwait(false);

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            attemptCount++;

            bool resolved = _loginStateResolver.TryResolveRaw(out RawLoginState state);
            TimeSpan elapsed = stopwatch.Elapsed;

            await EmitAsync(
                    LoginMonitorEventKind.Polling,
                    new LoginMonitorContext(
                        description,
                        timeout,
                        pollingInterval,
                        attemptCount,
                        elapsed,
                        resolved,
                        resolved ? state : null
                    ),
                    cancellationToken
                )
                .ConfigureAwait(false);

            if (resolved && predicate(state))
            {
                await EmitAsync(
                        LoginMonitorEventKind.Matched,
                        new LoginMonitorContext(
                            description,
                            timeout,
                            pollingInterval,
                            attemptCount,
                            elapsed,
                            true,
                            state
                        ),
                        cancellationToken
                    )
                    .ConfigureAwait(false);

                return state;
            }

            if (elapsed >= timeout)
            {
                await EmitAsync(
                        LoginMonitorEventKind.TimedOut,
                        new LoginMonitorContext(
                            description,
                            timeout,
                            pollingInterval,
                            attemptCount,
                            elapsed,
                            resolved,
                            resolved ? state : null
                        ),
                        cancellationToken
                    )
                    .ConfigureAwait(false);

                throw new TimeoutException($"Timed out waiting to {description} within {timeout}.");
            }

            TimeSpan remaining = timeout - elapsed;
            TimeSpan delay = pollingInterval <= remaining ? pollingInterval : remaining;
            await _delayAsync(delay, cancellationToken).ConfigureAwait(false);
        }
    }

    private static void ValidateWaitArguments(TimeSpan timeout, TimeSpan pollingInterval)
    {
        if (timeout <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(timeout), "Timeout must be positive.");

        if (pollingInterval < TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(pollingInterval), "Polling interval must not be negative.");
    }

    private ValueTask EmitAsync(
        LoginMonitorEventKind kind,
        LoginMonitorContext context,
        CancellationToken cancellationToken
    ) =>
        _sink is null
            ? ValueTask.CompletedTask
            : _sink.OnEventAsync(new LoginMonitorEvent(kind, context), cancellationToken);
}
