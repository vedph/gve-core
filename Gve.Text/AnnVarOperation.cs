using System;
using System.Collections.Generic;

namespace Gve.Text;

/// <summary>
/// A variant generation operation on the annotations plane.
/// </summary>
/// <seealso cref="VarOperationMetadata" />
public abstract class AnnVarOperation : VarOperation
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AnnVarOperation"/> class.
    /// </summary>
    /// <param name="type">The operation type.</param>
    protected AnnVarOperation(VarOperationType type) : base(type)
    {
    }

    /// <summary>
    /// Deeply clones the nodes into a new list.
    /// </summary>
    /// <param name="nodes">The nodes to clone.</param>
    /// <returns>A new list with cloned nodes.</returns>
    protected static List<CharNode> CloneNodes(IList<CharNode> nodes)
    {
        ArgumentNullException.ThrowIfNull(nodes);

        List<CharNode> newNodes = new(nodes.Count);
        foreach (CharNode node in nodes) newNodes.Add(node.Clone());
        return newNodes;
    }

    /// <summary>
    /// Executes this operation for the specified nodes, returning a copy of
    /// them.
    /// </summary>
    /// <param name="nodes">The nodes.</param>
    /// <param name="annotations">The current annotations.</param>
    /// <returns>The output nodes.</returns>
    public abstract IList<CharNode> Execute(IList<CharNode> nodes,
        IList<Feature> annotations);
}
