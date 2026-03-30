namespace Maple.Client.V95.Runtime;

/// <summary>
/// Composed snapshot of the current v95 login flow state.
/// </summary>
public readonly record struct ResolvedLoginState(
    uint LoginAddress,
    LoginStep Step,
    bool StepChanging,
    bool RequestSent,
    uint SelectedWorldItemAddress,
    int? ContextWorldId,
    int? ContextChannelId,
    int? ContextCharacterId,
    ResolvedWorldSelection? WorldSelection,
    ResolvedChannelSelection? ChannelSelection
)
{
    /// <summary>
    /// Gets whether the snapshot contains one resolved login address.
    /// </summary>
    public bool IsResolved => LoginAddress != 0;

    /// <summary>
    /// Gets whether a world-selection snapshot was available.
    /// </summary>
    public bool HasWorldSelection => WorldSelection is not null;

    /// <summary>
    /// Gets whether a channel-selection snapshot was available.
    /// </summary>
    public bool HasChannelSelection => ChannelSelection is not null;
}
