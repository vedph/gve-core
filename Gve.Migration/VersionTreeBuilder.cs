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
    public bool TraceFeatureFlattening { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to include deleted nodes
    /// in the tree. When this is true, for each generated version the builder
    /// will backtrack to the previous versions one after another, and insert
    /// nodes deleted by delete, replace, move, or swap operations at their
    /// place, with a delete feature. This relies on trace features: each
    /// operation injects segment-in trace features in the input version nodes,
    /// representing the nodes selected by it; and segment-out trace features
    /// in the output version nodes, representing the nodes produced or affected
    /// by it. So, when going backwards, we can find the nodes to insert by
    /// selecting nodes with segment-in features produced by operations which
    /// delete these segments, e.g. delete, replace, move, swap.
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
    /// Gets the IDs of all nodes with a segment/segment-2 feature in the
    /// specified version.
    /// </summary>
    /// <param name="context">The execution context.</param>
    /// <param name="tag">The version tag.</param>
    /// <param name="secondary">if set to <c>true</c>, look for segment-2 features.
    /// </param>
    /// <returns>List of IDs, sorted by their segment ordinal.</returns>
    private static List<int> GetIdsOfNodesWithFeature(
        ChainOperationContext<char> context, string tag, bool secondary)
    {
        // the key prefix in node features for the specified version
        string prefix = tag + "_";

        // collect IDs and their ordinals
        Dictionary<int, int> idAndNr = [];

        foreach (string key in context.NodeFeatures.Keys
            .Where(k => k.StartsWith(prefix)))
        {
            // get the requested feature if present
            OperationFeature? f = context.NodeFeatures[key].FirstOrDefault(
                f => secondary
                    ? f.Name == OperationFeature.F_TRACE_SEG2_IN
                    : f.Name == OperationFeature.F_TRACE_SEG_IN);

            // if found, add the ID with its ordinal
            if (f != null)
            {
                // parse ID from key (e.g. "v1_123" => 123)
                int id = int.Parse(key.AsSpan(prefix.Length));

                // parse NR from f.Value (OPID TAGIN:TAGOUT NR)
                int i = f.Value!.LastIndexOf(' ') + 1;
                idAndNr[id] = int.Parse(f.Value.AsSpan(i));
            }
        }

        // return IDs sorted by nr
        return [.. idAndNr.OrderBy(kv => kv.Value).Select(kv => kv.Key)];
    }

    /// <summary>
    /// Inserts new nodes into the tree whose root node is <paramref name="root"/>,
    /// after the reference node whose exported segment has the specified
    /// identifier.
    /// </summary>
    /// <param name="sources">The sources.</param>
    /// <param name="root">The root.</param>
    /// <param name="featPrefix">The feature prefix.</param>
    /// <param name="refId">The reference identifier.</param>
    private static void InsertNodes(List<ChainNode<char>> sources,
        TreeNode<ExportedSegment> root, string featPrefix, int refId)
    {
        // create the branch of new nodes
        TreeNode<ExportedSegment>? branch = null, current = null;
        int n = 1;
        foreach (ChainNode<char> source in sources)
        {
            TreeNode<ExportedSegment> newNode = new()
            {
                Data = new ExportedSegment
                {
                    SourceId = source.Id,
                    Text = source.Label,
                    Features =
                    [
                        new(OperationFeature.F_TRACE_DELETED,
                            $"{featPrefix} {n++}")
                    ]
                }
            };
            if (branch == null)
            {
                branch = newNode;
                current = branch;
            }
            else
            {
                current = current!.AddChild(newNode);
            }
        }
        if (branch == null) return;

        // find the reference node in the tree unless it's 0 (=beginning)
        TreeNode<ExportedSegment>? refNode = null;
        if (refId != 0)
        {
            root.Traverse(n =>
            {
                if (n.Data?.SourceId == refId)
                {
                    refNode = n;
                    return false;
                }
                return true;
            });
        }

        // insert the branch after refNode
        // get the tail to attach past the inserted branch
        TreeNode<ExportedSegment>? tail = refNode?.FirstChild ?? root.FirstChild;
        // insert branch
        if (refNode == null)
        {
            root.AddChild(branch);
        }
        else
        {
            // attach tail
            if (tail != null) tail.Parent = current;
            // attach branch to refNode
            refNode.AddChild(branch);
        }
    }

    private static (int id1, int id2) GetRefNodeId(
        ChainOperationContext<char> context, CharChainOperation op,
        string tag, int headId, int head2Id, int nextSegOutId)
    {
        int id1;
        int id2 = 0;

        switch (op.Type)
        {
            case OperationType.Delete:
            case OperationType.MoveBefore:
            case OperationType.MoveAfter:
                // delete and move select the nodes to act on, which are
                // then marked by seg-in feature. These are the nodes
                // to insert back into the tree.
                // Find the node before the 1st one with seg-in feature:
                // this will be the insertion point for the deleted nodes
                // (0=beginning of tree)
                id1 = context.Chain.GetPreviousNode(
                    tag, context.Chain.FindNode(headId)!)?.Id ?? 0;
                break;

            case OperationType.Replace:
                // insert before the 1st segout of the next version
                id1 = nextSegOutId;
                break;

            case OperationType.Swap:
                // insert 1 before 1st seg2in, and 2 before 1st segin
                id1 = head2Id;
                id2 = headId;
                break;

            default:
                id1 = -1;
                break;
        }

        return (id1, id2);
    }

    private static List<ChainNode<char>> CollectNodes(
        ChainOperationContext<char> context, string tag, List<int> ids)
    {
        IList<ChainNode<char>> tagNodes = context.Chain.GetTagNodes(tag);
        List<ChainNode<char>> nodes = [];
        foreach (int id in ids)
            nodes.Add(tagNodes.First(n => n.Id == id));
        return nodes;
    }

    /// <summary>
    /// Gets the ID of the first node of the output segment in the specified
    /// version.
    /// </summary>
    /// <param name="context">The execution context.</param>
    /// <param name="tag">The version tag.</param>
    /// <returns>ID or 0 if no output segment in version.</returns>
    private static int GetHeadSegOutId(ChainOperationContext<char> context,
        string tag)
    {
        // the key prefix in node features for the specified version
        string prefix = tag + "_";

        foreach (string key in context.NodeFeatures.Keys
            .Where(k => k.StartsWith(prefix)))
        {
            // get the requested feature if present
            OperationFeature? f = context.NodeFeatures[key].FirstOrDefault(
                f => f.Name == OperationFeature.F_TRACE_SEG_OUT);

            // if found, add the ID with its ordinal
            if (f != null) return int.Parse(key.AsSpan(prefix.Length));
        }

        return 0;
    }

    private static void RestoreDeletions(ChainOperationContext<char> context,
        IList<CharChainOperation> operations,
        IList<string> tags, int tagIndex, TreeNode<ExportedSegment> root)
    {
        int i = tagIndex - 1;
        while (i > -1)
        {
            // collect IDs of all the nodes with seg-in feature in this version
            List<int> inIds = GetIdsOfNodesWithFeature(context, tags[i], false);

            // collect IDs of all the nodes with seg2-in feature in this version
            List<int> in2Ids = GetIdsOfNodesWithFeature(context, tags[i], true);

            // nothing to do if no seg-in
            if (inIds.Count == 0)
            {
                i--;
                continue;
            }

            // extract OPID from feature: all the collected nodes have the same,
            // so we just use the first one as source
            string value = context.NodeFeatures[$"{tags[tagIndex]}_{inIds[0]}"]
                .First(f => f.Name == OperationFeature.F_TRACE_SEG_IN).Value!;
            string opId = value[..value.IndexOf(' ')];

            // get the corresponding operation
            CharChainOperation op = operations.First(o => o.Id == opId);
            var (id1, id2) = GetRefNodeId(context, op, tags[tagIndex],
                inIds[0], in2Ids.Count > 0 ? in2Ids[0] : 0,
                GetHeadSegOutId(context, tags[i]));

            // we care only about operations which imply deletions
            if (id1 > -1)
            {
                // feature prefix to be used when creating the new nodes
                // for their seg-in features (OPID TAGIN:TAGOUT + space)
                string featPrefix =
                    $"{opId} {tags[tagIndex]}:{tags[tagIndex + 1]} ";

                // insert seg-in nodes before reference node:
                // collect source nodes in the order specified by inIds
                List<ChainNode<char>> inNodes =
                    CollectNodes(context, tags[i], inIds);

                // insert the collected nodes
                InsertNodes(inNodes, root, featPrefix, id1);

                // insert seg2-in nodes before reference node:
                // collect source nodes in the order specified by in2Ids
                List<ChainNode<char>> in2Nodes =
                    CollectNodes(context, tags[i], in2Ids);

                // insert the collected nodes
                InsertNodes(in2Nodes, root, featPrefix, id2);
            }

            // move to previous version
            i--;
        }
    }

    private static void FlattenTraceFeatures(List<NamedValue>? nodeTraceFeats,
        List<NamedValue> targetFeats)
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

    /// <summary>
    /// Builds a version linear tree from the specified execution context.
    /// This method builds the tree for each version tag with the features
    /// selected in <see cref="FeatureNames"/>. If this is not set, all features
    /// will be included.
    /// </summary>
    /// <param name="context">The execution context.</param>
    /// <param name="operations">The operations executed on the context.</param>
    /// <returns>The root blank node of the tree.</returns>
    /// <exception cref="ArgumentNullException">chain or operations</exception>
    public IDictionary<string, TreeNode<ExportedSegment>> Build(
        ChainOperationContext<char> context, IList<CharChainOperation> operations)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(operations);

        // for each version in build order (distinct ensures that we
        // have no duplicates, even if this is not expected here)
        Dictionary<string, TreeNode<ExportedSegment>> versions = [];

        // setup a cache for trace features as we process each version
        Dictionary<int, List<NamedValue>> traceFeats = [];

        // for each version tag
        IList<string> tags = context.Chain.GetDistinctTags();
        for (int i = 0; i < tags.Count; i++)
        {
            string tag = tags[i];

            // get nodes for tag
            IList<ChainNode<char>> sources = context.Chain.GetTagNodes(tag);

            // get nodes features for tag
            Dictionary<int, IList<Feature>> sourceTagFeats =
                context.GetTagNodeFeatures(tag);

            // create blank root
            TreeNode<ExportedSegment> root = new();
            versions[tag] = root;

            // add branch
            TreeNode<ExportedSegment> current = root;
            foreach (ChainNode<char> source in sources)
            {
                // get selected node features
                IList<Feature> sourceFeats = sourceTagFeats.TryGetValue(
                    source.Id, out IList<Feature>? value) ? value : [];

                // inject only the selected features
                List<NamedValue> targetFeats = [];
                foreach (Feature feature in sourceFeats
                    .Where(f => IsFeatureIncluded(f.Name)))
                {
                    targetFeats.Add(new NamedValue(feature.Name, feature.Value));
                }

                // flatten trace features from previous versions if requested
                if (TraceFeatureFlattening && traceFeats.TryGetValue(
                    source.Id, out List<NamedValue>? nodeTraceFeats))
                {
                    FlattenTraceFeatures(nodeTraceFeats, targetFeats);
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
                if (TraceFeatureFlattening && sourceFeats.Any(f =>
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

                // restore deletions if requested
                if (IncludeDeletions)
                    RestoreDeletions(context, operations, tags, i, root);
            }
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
    /// <exception cref="ArgumentNullException">text or operations</exception>
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

        return Build(context, operations);
    }
}
