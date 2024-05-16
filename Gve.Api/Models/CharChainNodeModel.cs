using Gve.Text;

namespace Gve.Api.Models;

/// <summary>
/// Model for a character chain node.
/// </summary>
public class CharChainNodeModel
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
    /// The node features.
    /// </summary>
    public IList<Feature>? Features { get; set; }
}
