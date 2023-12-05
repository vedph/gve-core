using System;
using System.Collections.Generic;
using System.Linq;

namespace Gve.Text;

/// <summary>
/// Data context for any variant text generation operation.
/// </summary>
public class VarOperationContext
{
    /// <summary>
    /// Gets the set of character nodes to work on for base operations. This
    /// is derived from the input base text.
    /// </summary>
    public CharNodeSet Set { get; }

    /// <summary>
    /// Gets the tree nodes to work on for annotation operations. These are
    /// initially cloned from the <see cref="Set"/> and then modified by the
    /// annotation operations, and represent the base text characters ordered
    /// into a list. Annotation operations work on this list relying on the
    /// indexes of each node in it, rather than on the left link of each node.
    /// </summary>
    public List<CharNode> TreeNodes { get; }

    /// <summary>
    /// Gets or sets the tags attached to the context by the various operations
    /// executed on it.
    /// </summary>
    public HashSet<string> Tags { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="VarOperationContext"/> class.
    /// </summary>
    /// <param name="set">The set.</param>
    public VarOperationContext(CharNodeSet set)
    {
        Set = set ?? throw new ArgumentNullException(nameof(set));
        TreeNodes = GetTreeNodesFromSet(set);
        Tags = [];
    }

    /// <summary>
    /// Gets tree nodes from the specified set.
    /// </summary>
    /// <param name="set">The set.</param>
    /// <returns>Tree nodes.</returns>
    public static List<CharNode> GetTreeNodesFromSet(CharNodeSet set)
    {
        ArgumentNullException.ThrowIfNull(set);
        return set.Where(n => !n.IsDeleted).Select(n => n.Clone()).ToList();
    }

    /// <summary>
    /// Merges the specified tree nodes back into their source set.
    /// </summary>
    /// <param name="nodes">The nodes.</param>
    /// <param name="set">The source set of nodes.</param>
    public static void MergeTreeNodesInSet(IList<CharNode> nodes,
        CharNodeSet set)
    {
        ArgumentNullException.ThrowIfNull(nodes);
        ArgumentNullException.ThrowIfNull(set);

        // ensure that each node in nodes is linked to the its left node
        for (int i = 1; i < nodes.Count; i++)
            nodes[i].LeftNode = nodes[i - 1];

        // if any deleted node in set was linked to any of nodes,
        // then link its right node's left node to the deleted node
        foreach (CharNode node in set.Where(n => n.IsDeleted))
        {
            if (node.LeftNode != null && nodes.Contains(node.LeftNode))
                set.GetRightNodeOf(node)!.LeftNode = node;
        }
    }

    /// <summary>
    /// Gets the text represented by <see cref="TreeNodes"/>.
    /// </summary>
    /// <returns>String.</returns>
    public string GetTreeText()
    {
        return string.Join("", TreeNodes.Select(n => n.Value));
    }

    /// <summary>
    /// Deeply clones this instance.
    /// </summary>
    /// <returns>New context.</returns>
    public virtual VarOperationContext Clone()
    {
        return new VarOperationContext(Set.Clone())
        {
            Tags = new HashSet<string>(Tags)
        };
    }

    /// <summary>
    /// Converts to string.
    /// </summary>
    /// <returns>String.</returns>
    public override string ToString()
    {
        return $"[{string.Join(" ", Tags)}] {Set.GetText()}";
    }
}
