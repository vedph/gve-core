using Fusi.Tools.Configuration;
using Fusi.Tools.Data;
using System;

namespace Proteus.Rendering;

/// <summary>
/// A multiple trees text tree filter. This assumes that the received tree
/// has a blank root node whose children are each a separate tree. So, all the
/// children of the root will be blank nodes, each representing the sub-tree
/// root. The inner filter of this composite filte will be applied to each
/// such sub-tree.
/// <para>Tag: <c>text-tree-filter.multi</c>.</para>
/// </summary>
/// <seealso cref="CompositeTextTreeFilter" />
/// <seealso cref="ITextTreeFilter" />
[Tag("text-tree-filter.multi")]
public sealed class MultiTextTreeFilter : CompositeTextTreeFilter, ITextTreeFilter
{
    /// <summary>
    /// Applies this filter to the specified tree, generating a new tree
    /// or just returning the received one.
    /// </summary>
    /// <param name="tree">The tree's root node.</param>
    /// <param name="source">The source being rendered.</param>
    /// <returns>
    /// The root node of the new tree.
    /// </returns>
    /// <exception cref="ArgumentNullException">tree</exception>
    public TreeNode<ExportedSegment> Apply(TreeNode<ExportedSegment> tree,
        object? source = null)
    {
        ArgumentNullException.ThrowIfNull(tree);

        // no subtrees or not configured, nothing to do
        if (tree.Children.Count == 0 || Component == null) return tree;

        // apply inner filter to each sub-tree
        foreach (TreeNode<ExportedSegment> sub in tree.Children)
        {
            TreeNode<ExportedSegment> filtered = Component.Apply(sub, source);
            if (filtered != sub) sub.ReplaceWith(filtered);
        }

        return tree;
    }
}
