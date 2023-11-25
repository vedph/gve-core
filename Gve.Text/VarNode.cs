using System.Collections.Generic;

namespace Gve.Text;

/// <summary>
/// A node in a text variant tree.
/// </summary>
public class VarNode
{
    /// <summary>
    /// Gets or sets the text edit operation for this node.
    /// </summary>
    public VarOperation Operation { get; }

    /// <summary>
    /// Gets or sets the diplomatic data about the node.
    /// </summary>
    public VarDiplomatic? Diplomatic { get; set; }

    /// <summary>
    /// Gets or sets the sources for the variant represented by this node.
    /// </summary>
    public IList<VarSource> Sources { get; set; }

    /// <summary>
    /// Gets or sets the children nodes.
    /// </summary>
    public IList<VarNode> Children { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="VarNode"/> class.
    /// </summary>
    public VarNode(VarOperation operation)
    {
        Operation = operation
            ?? throw new System.ArgumentNullException(nameof(operation));
        Sources = new List<VarSource>();
        Children = new List<VarNode>();
    }

    /// <summary>
    /// Converts to string.
    /// </summary>
    /// <returns>
    /// A <see cref="string" /> that represents this instance.
    /// </returns>
    public override string ToString()
    {
        // TODO
        return base.ToString();
    }
}
