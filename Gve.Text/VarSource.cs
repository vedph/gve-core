namespace Gve.Text;

/// <summary>
/// The source for a variant.
/// </summary>
public class VarSource
{
    /// <summary>
    /// Gets or sets the source identifier.
    /// </summary>
    public string Id { get; set; } = "";

    /// <summary>
    /// Gets or sets the source type (e.g. manuscript, author...).
    /// </summary>
    public string Type { get; set; } = "";

    /// <summary>
    /// Gets or sets the probability rank for the source attribution.
    /// </summary>
    public short Rank { get; set; }

    /// <summary>
    /// Gets or sets an optional note.
    /// </summary>
    public string? Note { get; set; }

    /// <summary>
    /// Converts to string.
    /// </summary>
    /// <returns>
    /// A <see cref="string" /> that represents this instance.
    /// </returns>
    public override string ToString()
    {
        return $"[{Type}] {Id} ({Rank})";
    }
}
