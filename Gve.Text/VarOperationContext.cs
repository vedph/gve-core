using System;
using System.Collections.Generic;
using System.Linq;

namespace Gve.Text;

/// <summary>
/// Data context for any variant text generation operation. This contains both
/// the nodes of the base text, and the nodes of the annotation text, which are
/// derived from the base text nodes according to its selection as specified
/// by an annotation operation.
/// </summary>
public class VarOperationContext
{
    /// <summary>
    /// Gets the set of character nodes to work on for base operations. This
    /// is derived from the input base text.
    /// </summary>
    public CharNodeSet Set { get; }

    /// <summary>
    /// Gets the annotation nodes to work on for annotation operations. These are
    /// extracted from <see cref="Set"/>, and then modified by the
    /// annotation operations, and represent the base text characters ordered
    /// into a list. Annotation operations work on this list relying on the
    /// indexes of each node in it, rather than on the left link of each node.
    /// </summary>
    public List<CharNode> AnnNodes { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="VarOperationContext"/> class.
    /// </summary>
    /// <param name="set">The set.</param>
    public VarOperationContext(CharNodeSet set)
    {
        Set = set ?? throw new ArgumentNullException(nameof(set));
        AnnNodes = [];
    }

    /// <summary>
    /// Sets tree nodes from the specified set.
    /// </summary>
    /// <param name="set">The set.</param>
    /// <param name="at">The ordinal of the first node to get.</param>
    /// <param name="run">The number of nodes to get starting from the first
    /// one.</param>
    /// <returns>Tree nodes.</returns>
    /// <remarks>Getting tree nodes from the set means to get them in the order
    /// defined by their left-node links, excluding the deleted nodes, for the
    /// range defined by <paramref name="at"/> and <paramref name="run"/>.
    /// </remarks>
    public void SetAnnNodes(CharNodeSet set, short at, short run)
    {
        AnnNodes.Clear();

        List<CharNode> ordered = set.Where(n => !n.IsDeleted)
            .Select(n => n.Clone())
            .ToList();

        int i = ordered.FindIndex(n => n.Ordinal == at);
        if (i > -1) AnnNodes.AddRange(ordered.Skip(i).Take(run).ToList());
    }

    /// <summary>
    /// Merges the annotation nodes back into their source set.
    /// </summary>
    public void MergeAnnNodes()
    {
        // ensure that each node in nodes is linked to the its left node
        for (int i = 1; i < AnnNodes.Count; i++)
            AnnNodes[i].LeftNode = AnnNodes[i - 1];

        // if any deleted node in set was linked to any of nodes,
        // then link its right node's left node to the deleted node
        foreach (CharNode node in Set.Where(n => n.IsDeleted))
        {
            if (node.LeftNode != null && AnnNodes.Contains(node.LeftNode))
                Set.GetRightNodeOf(node)!.LeftNode = node;
        }

        // add the nodes to the set unless already present
        Set.MergeNodes(AnnNodes);
    }

    /// <summary>
    /// Gets the text represented by <see cref="AnnNodes"/>.
    /// </summary>
    /// <returns>String.</returns>
    public string GetTreeText()
    {
        return string.Join("", AnnNodes.Select(n => n.Value));
    }

    /// <summary>
    /// Deeply clones this instance.
    /// </summary>
    /// <returns>New context.</returns>
    public virtual VarOperationContext Clone()
        => new VarOperationContext(Set.Clone());

    /// <summary>
    /// Converts to string.
    /// </summary>
    /// <returns>String.</returns>
    public override string ToString()
    {
        return $"{Set.GetText()} ({AnnNodes.Count})";
    }
}
