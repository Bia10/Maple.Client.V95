namespace Maple.Client.V95.Analysis;

/// <summary>
/// Explains whether one runtime function target was successfully dumped.
/// </summary>
public enum RuntimeFunctionDumpStatus
{
    Success = 0,
    AddressOutsideModule,
}
