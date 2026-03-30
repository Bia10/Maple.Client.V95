namespace Maple.Client.V95;

/// <summary>
/// Client-side <c>CLogin::m_nLoginStep</c> values for the GMS v95 client.
/// </summary>
public enum LoginStep : byte
{
    /// <summary>Initial title screen.</summary>
    Title = 0,

    /// <summary>World-selection screen.</summary>
    SelectWorld = 1,

    /// <summary>Character-selection screen.</summary>
    SelectCharacter = 2,

    /// <summary>New-character race selection.</summary>
    NewCharacter = 3,

    /// <summary>New-character naming/avatar selection.</summary>
    NewCharacterName = 4,

    /// <summary>View-all-characters mode.</summary>
    Vac = 5,
}
