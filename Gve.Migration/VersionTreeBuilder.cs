using Fusi.Tools.Data;
using Gve.Text;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Gve.Migration;

/// <summary>
/// A class that builds a linear tree representing a text version derived from
/// a GVE chain. The built tree has a blank root node and a single branch
/// stemming from it, where each node represents a character and is parent of
/// the next character node.
/// </summary>
public sealed class VersionTreeBuilder
{
    /// <summary>
    /// Gets or sets the filter features names. When set, only features with
    /// names equal to the names in this list are included in the tree.
    /// </summary>
    public HashSet<string>? FeatureNames { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether <see cref="FeatureNames"/> is
    /// inverted, i.e. it represents features to exclude from the tree.
    /// </summary>
    public bool IsFilterInverted { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to flatten trace features.
    /// When this is true, the trace features linked to nodes are not only drawn
    /// from the each processed version, but also from all the previous versions.
    /// </summary>
    public bool FlattenTraceFeatures { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to include deleted nodes
    /// in the tree.
    /// </summary>
    public bool IncludeDeletions { get; set; }

    private static List<ChainNode<char>> GetNodesFrom(string text)
    {
        List<ChainNode<char>> nodes = new(text.Length);
        foreach (char c in text)
            nodes.Add(new ChainNode<char>(c, c.ToString()));
        return nodes;
    }

    private bool IsFeatureIncluded(string name)
    {
        if (FeatureNames == null) return true;
        return IsFilterInverted ^ FeatureNames.Contains(name);
    }

    /// <summary>
    /// Builds a version linear tree from the specified execution context.
    /// This method builds the tree for each version tag with the features
    /// selected in <see cref="FeatureNames"/>. If this is not set, all features
    /// will be included.
    /// </summary>
    /// <param name="context">The execution context.</param>
    /// <returns>The root blank node of the tree.</returns>
    /// <exception cref="ArgumentNullException">chain or tag</exception>
    public IDictionary<string, TreeNode<ExportedSegment>>
        Build(ChainOperationContext<char> context)
    {
        ArgumentNullException.ThrowIfNull(context);

        // for each version in build order (distinct ensures that we
        // have no duplicates, even if this is not expected here)
        Dictionary<string, TreeNode<ExportedSegment>> versions = [];

        // setup a cache for trace features as we process each version
        Dictionary<int, List<NamedValue>> traceFeats = [];

        foreach (string tag in context.Chain.GetDistinctTags())
        {
            // get nodes for tag
            IList<ChainNode<char>> sources = context.Chain.GetTagNodes(tag);

            // create blank root
            TreeNode<ExportedSegment> root = new();
            versions[tag] = root;

            // add branch
            TreeNode<ExportedSegment> current = root;
            foreach (ChainNode<char> source in sources)
            {
                // get selected node features
                IList<Feature> sourceFeats = context.GetNodeFeatures(
                    source.Id, tag);
                List<NamedValue> targetFeats = [];
                foreach (Feature feature in sourceFeats
                    .Where(f => IsFeatureIncluded(f.Name)))
                {
                    targetFeats.Add(new NamedValue(feature.Name, feature.Value));
                }

                // flatten trace features from previous versions if requested
                if (FlattenTraceFeatures && traceFeats.TryGetValue(
                    source.Id, out List<NamedValue>? nodeTraceFeats))
                {
                    foreach (NamedValue traceFeat in nodeTraceFeats)
                    {
                        if (targetFeats.All(f => f.Name != traceFeat.Name &&
                            f.Value != traceFeat.Value))
                        {
                            targetFeats.Add(traceFeat);
                        }
                    }
                }

                // add child with text, version tag and features
                current = current.AddChild(new TreeNode<ExportedSegment>
                {
                    Data = new ExportedSegment
                    {
                        SourceId = source.Id,
                        Text = source.Label,
                        Tags = [tag],
                        Features = targetFeats.Count == 0 ? null : targetFeats
                    }
                });

                // cache all the trace features from node for next version
                // flattening
                if (FlattenTraceFeatures && sourceFeats.Any(f =>
                    f.Name.StartsWith(OperationFeature.F_TRACE_PREFIX)))
                {
                    if (!traceFeats.ContainsKey(source.Id))
                        traceFeats[source.Id] = [];

                    foreach (Feature feature in sourceFeats
                        .Where(f => f.Name.StartsWith(
                            OperationFeature.F_TRACE_PREFIX)))
                    {
                        traceFeats[source.Id].Add(
                            new NamedValue(feature.Name, feature.Value));
                    }
                }
            } // node-for
        }

        return versions;
    }

    /// <summary>
    /// Builds a version linear tree from the specified base text and operations
    /// on it. This method builds the tree for each version tag with the features
    /// selected in <see cref="FeatureNames"/>. If this is not set, all features
    /// will be included.
    /// </summary>
    /// <returns>The root blank node of the tree.</returns>
    /// <exception cref="ArgumentNullException">chain or tag</exception>
    public IDictionary<string, TreeNode<ExportedSegment>>
        Build(string text, IList<CharChainOperation> operations)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(nameof(text));
        ArgumentNullException.ThrowIfNullOrEmpty(nameof(operations));

        // init chain with base text (v0)
        ChainOperationContext<char> context = new();
        context.Chain.Reset(GetNodesFrom(text));
        context.LastTag = "v0";

        // execute all the operations in their order
        List<string> tags = [];
        foreach (CharChainOperation op in operations)
        {
            op.Execute(context);
            tags.Add(context.LastTag);
        }

        return Build(context);
    }
}
