namespace Gve.Text;

/// <summary>
/// The data context for <see cref="SpineOperation"/>.
/// </summary>
public class VarContext
{
    /// <summary>
    /// Gets or sets the current version tag.
    /// </summary>
    public string? VersionTag { get; set; }

    /// <summary>
    /// Gets or sets the current operation number.
    /// </summary>
    public int OperationNumber { get; set; }

    /// <summary>
    /// Gets or sets the base text.
    /// </summary>
    public string BaseText { get; set; } = "";
}
