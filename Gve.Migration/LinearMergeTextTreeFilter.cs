using Fusi.Tools.Configuration;
using Fusi.Tools.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Gve.Migration;

/// <summary>
/// A merge filter for linear trees. This filter merges multiple
/// nodes into a single one whose text is the concatenation of the text of
/// all the merged nodes. Merging is done by checking the features of the
/// nodes.
/// <para>Tag: <c>text-tree-filter.linear-merge</c>.</para>
/// </summary>
[Tag("text-tree-filter.linear-merge")]
public sealed class LinearMergeTextTreeFilter : ITextTreeFilter,
    IConfigurable<MergeTextTreeFilterOptions>
{
    private MergeTextTreeFilterOptions _options = new();

    private HashSet<string> GetRelevantFeatureNames(
        TreeNode<ExportedSegment> node)
    {
        HashSet<string> names = [];
        if (node.Data?.Features == null || node.Data.Features.Count == 0)
            return names;

        foreach (string name in node.Data.Features.Select(f => f.Name))
        {
            if (_options?.Features == null || _options.Features.Contains(name))
                names.Add(name);
        }

        return names;
    }

    private static void MergeNodes(TreeNode<ExportedSegment> source,
        TreeNode<ExportedSegment> target)
    {
        // nothing to do for an empty source
        if (source.Data == null) return;

        // just copy the source data if the target is empty
        if (target.Data == null)
        {
            target.Data = source.Data;
            return;
        }

        // merge text
        if (source.Data.Text != null)
        {
            target.Data.Text = target.Data.Text != null?
                target.Data.Text + source.Data.Text : source.Data.Text;
        }

        // (features are equal by definition)

        // merge tags
        if (source.Data.Tags != null)
        {
            if (target.Data.Tags == null)
            {
                target.Data.Tags = source.Data.Tags;
            }
            else
            {
                target.Data.Tags.UnionWith(source.Data.Tags);
            }
        }

        // merge data
        if (source.Data.Payloads != null)
        {
            if (target.Data.Payloads == null)
                target.Data.Payloads = source.Data.Payloads;
            else
                target.Data.Payloads.AddRange(source.Data.Payloads);
        }
    }

    /// <summary>
    /// Applies this filter to the specified tree, generating a new tree or
    /// just returning the source tree if no merge is possible.
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

        // nothing to do for an empty text
        if (tree.FirstChild == null) return tree;

        TreeNode<ExportedSegment> root = new(), // tree being built
            target = tree.FirstChild.Clone();   // current target node
        // current source node
        TreeNode<ExportedSegment>? current = tree.FirstChild;

        // collect relevant features from first text node
        HashSet<string> segFeats = GetRelevantFeatureNames(tree.FirstChild);

        // keep moving to next child until features are equal
        do
        {
            // get relevant features of current node
            HashSet<string> feats = GetRelevantFeatureNames(current);

            // if features are equal and either we don't care about LF or there
            // is no LF at current's text end, merge current node into target
            if (segFeats.SetEquals(feats) &&
                (_options.NoBreakAtLF || current.Data?.Text?.EndsWith('\n') != true))
            {
                // merge into current and move to next child
                MergeNodes(current, target);
                current = current.FirstChild;
            }
            else
            {
                // append a copy current node to the built branch
                target = target.AddChild(current.Clone());
                // move to next child
                current = current.FirstChild;
            }
        } while (current != null);

        return root;
    }

    /// <summary>
    /// Configures this filter with the specified options.
    /// </summary>
    /// <param name="options">The options.</param>
    /// <exception cref="ArgumentNullException">options</exception>
    public void Configure(MergeTextTreeFilterOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _options = options;
    }
}

/// <summary>
/// Options for the <see cref="LinearMergeTextTreeFilter"/>.
/// </summary>
public class MergeTextTreeFilterOptions
{
    /// <summary>
    /// Gets or sets the names of the features considered as criteria for
    /// merging. If not specified, all the features are considered.
    /// </summary>
    public HashSet<string>? Features { get; set; }

    /// <summary>
    /// Gets or sets a value indicating not to force a segment break after
    /// any node whose text ends with (or is equal to) a LF.
    /// </summary>
    public bool NoBreakAtLF { get; set; }
}
