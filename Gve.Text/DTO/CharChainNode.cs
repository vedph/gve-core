using System.Collections.Generic;
using System.Linq;

namespace Gve.Text.DTO;

/// <summary>
/// Base text character chain node.
/// </summary>
public class CharChainNode
{
    /// <summary>
    /// The autonumber assigned to this node in the context of its chain.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the node's index in the base text.
    /// </summary>
    public int Index { get; set; }

    /// <summary>
    /// The node label.
    /// </summary>
    public string Label { get; set; } = "";

    /// <summary>
    /// The data attached to this node.
    /// </summary>
    public char Data { get; set; }

    /// <summary>
    /// The source tag for this node, i.e. the tag of the version which first
    /// added it to the chain.
    /// </summary>
    public string SourceTag { get; set; } = "";

    /// <summary>
    /// The features for this node, grouped by version tag. Each entry is the
    /// list of features that apply to this node at that version. Null when the
    /// node has no features in any version.
    /// </summary>
    public IDictionary<string, IList<Feature>>? Features { get; set; }

    /// <summary>
    /// Clones this instance.
    /// </summary>
    /// <returns>New instance.</returns>
    public CharChainNode Clone()
    {
        Dictionary<string, IList<Feature>>? feats = null;
        if (Features != null)
        {
            feats = [];
            foreach (KeyValuePair<string, IList<Feature>> kvp in Features)
                feats[kvp.Key] = [.. kvp.Value];
        }

        return new CharChainNode
        {
            Id = Id,
            Index = Index,
            Label = Label,
            Data = Data,
            SourceTag = SourceTag,
            Features = feats
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
        string feats = Features == null ? ""
            : string.Join("; ", Features.Select(kvp =>
                $"{kvp.Key}:[{string.Join(", ", kvp.Value)}]"));
        return $"#{Id}={Label} ({feats})";
    }
}
