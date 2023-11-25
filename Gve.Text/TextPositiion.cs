using System.Collections.Generic;
using System.Text;

namespace Gve.Text;

/// <summary>
/// The SVG-like position of a text portion.
/// </summary>
public class TextPosition
{
    /// <summary>
    /// Gets or sets the text anchor.
    /// </summary>
    public string? TextAnchor { get; set; }

    /// <summary>
    /// Gets or sets the dominant baseline.
    /// </summary>
    public string? DominantBaseline { get; set; }

    /// <summary>
    /// Gets or sets the transforms applied to the position.
    /// </summary>
    public IList<string>? Transforms { get; set; }

    /// <summary>
    /// Converts to string.
    /// </summary>
    /// <returns>
    /// A <see cref="string" /> that represents this instance.
    /// </returns>
    public override string ToString()
    {
        StringBuilder sb = new();

        if (!string.IsNullOrEmpty(TextAnchor))
            sb.Append("A=").Append(TextAnchor).Append(" | ");

        if (!string.IsNullOrEmpty(DominantBaseline))
            sb.Append("B=").Append(DominantBaseline).Append(" | ");

        if (Transforms?.Count > 0)
            sb.Append("T=").AppendJoin(" ", Transforms);

        return sb.ToString().TrimEnd(' ', '|');
    }
}
