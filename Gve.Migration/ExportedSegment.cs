using Gve.Text;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Fusi.Tools.Data;

namespace Gve.Migration;

/// <summary>
/// An exported segment of text. This is typically used as the payload of
/// a <see cref="TreeNode{T}"/> when building the linear tree to start the export
/// process with.
/// </summary>
public class ExportedSegment
{
    /// <summary>
    /// Gets or sets the source identifier for this segment. This is initially
    /// set to the source node ID when building the linear tree character by
    /// character.
    /// </summary>
    public int SourceId { get; set; }

    /// <summary>
    /// Gets or sets the segment's text.
    /// </summary>
    public string? Text { get; set; }

    /// <summary>
    /// Gets or sets the features linked to this segment.
    /// </summary>
    public List<NamedValue>? Features { get; set; }

    /// <summary>
    /// Gets or sets the tags attached to this segment.
    /// </summary>
    public HashSet<string>? Tags { get; set; }

    /// <summary>
    /// Clones this instance.
    /// </summary>
    /// <returns>New segment.</returns>
    public ExportedSegment Clone()
    {
        return new ExportedSegment
        {
            Text = Text,
            Features = Features?.Select(f =>
                new NamedValue(f.Name, f.Value)).ToList(),
            Tags = Tags == null ? null : [.. Tags]
        };
    }

    /// <summary>
    /// Converts to string.
    /// </summary>
    /// <returns>
    /// A <see cref="string" /> that represents this instance.
    /// </returns>
    public override string ToString()
    {
        StringBuilder sb = new();
        sb.Append(Text);
        if (Features?.Count > 0) sb.Append(" (F=").Append(Features.Count).Append(')');
        if (Tags?.Count > 0)
            sb.Append(" [").AppendJoin(",", Tags).Append(']');
        return sb.ToString();
    }
}
