using Fusi.Tools.Data;
using Gve.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
    /// before the reference node whose exported segment has the specified
    /// identifier.
    /// </summary>
    /// <param name="sources">The sources.</param>
    /// <param name="root">The root.</param>
    /// <param name="featPrefix">The feature prefix.</param>
    /// <param name="refId">The reference identifier, or 0 for appending nodes
    /// at the end.</param>
    private static void InsertNodesBefore(List<ChainNode<char>> sources,
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
                    Text = source.Data.ToString(),
                    Features =
                    [
                        new(OperationFeature.F_TRACE_DELETED,
                            $"{featPrefix}{n++}")
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

        // find the reference node in the tree unless it's 0 (=end)
        TreeNode<ExportedSegment>? refNode = null;
        if (refId != 0)
        {
            root.Traverse(n =>
            {
                // the reference node can't be a deleted one
                // (this avoids inserting at the wrong place in case of
                // duplicate nodes from restorations, e.g. in a swap)
                if (n.Data?.SourceId == refId &&
                    n.Data.Features?.All(f => f.Name !=
                        OperationFeature.F_TRACE_DELETED) == true)
                {
                    refNode = n;
                    return false;
                }
                return true;
            });
        }

        // insert the branch before the reference node
        if (refNode != null)
        {
            // detach refNode from its parent
            TreeNode<ExportedSegment>? parent = refNode.Parent;
            parent?.Children.Remove(refNode);

            // attach refNode as the child of the last node in the branch
            current!.AddChild(refNode);

            // attach the branch to the original parent of refNode
            if (parent != null) parent.AddChild(branch);
            else root.AddChild(branch);
        }
        else
        {
            // insert at the end of the tree
            TreeNode<ExportedSegment>? last = root.FirstChild;
            while (last != null) last = last.FirstChild;

            if (last != null) last.AddChild(branch);
            else root.AddChild(branch);
        }
    }

    /// <summary>
    /// Adjusts the reference node ID to account for deleted nodes that have
    /// already been restored in the tree.
    /// </summary>
    /// <param name="root">The root node of the tree.</param>
    /// <param name="refId">The original reference ID from the chain.</param>
    /// <returns>The ID of the node before which deleted nodes should be inserted.
    /// </returns>
    private static int AdjustReferenceId(TreeNode<ExportedSegment> root, int refId)
    {
        if (refId == 0) return 0; // beginning of tree, no adjustment needed

        // find the node in the tree with SourceId == refId
        TreeNode<ExportedSegment>? refNode = null;
        root.Traverse(n =>
        {
            if (n.Data?.SourceId == refId)
            {
                refNode = n;
                return false;
            }
            return true;
        });

        // if we couldn't find the exact node, we need to find the closest match
        // (the node that would come right after our insertion point)
        if (refNode == null)
        {
            // find a node with a SourceId greater than refId
            TreeNode<ExportedSegment>? nextNode = null;
            int minGreaterId = int.MaxValue;

            root.Traverse(n =>
            {
                if (n.Data?.SourceId > refId && n.Data.SourceId < minGreaterId)
                {
                    nextNode = n;
                    minGreaterId = n.Data.SourceId;
                }
                return true;
            });

            if (nextNode != null)
            {
                // find the parent of this node (which is our insertion point)
                refNode = nextNode.Parent;
            }
        }

        // if we still couldn't find a reference point, use the original ID
        return refNode?.Data?.SourceId ?? refId;
    }

    private (int id1, int id2) GetRefNodeId(ChainOperationContext<char> context,
        IList<string> tags, int tagIndex, CharChainOperation op,
        int headId, int tailId, int head2Id,
        TreeNode<ExportedSegment> root)
    {
        // first find the head(s) of the deleted segment(s)
        int id1;
        int id2 = 0;

        switch (op.Type)
        {
            case OperationType.Delete:
            case OperationType.MoveBefore:
            case OperationType.MoveAfter:
                // insert before 1st seg-in (deleted) node, but the problem
                // is that this node won't be present in the output version:
                // so we need to use the node past the last deleted node
                // (which can be null when the last node(s) get deleted)
                ChainNode<char>? next = context.Chain.GetNextNode(
                    tags[tagIndex], context.Chain.FindNode(tailId)!);

                // if not found, restore at end
                if (next == null) return (0, 0);
                // else restore before it
                return (next.Id, 0);

            case OperationType.Replace:
                // insert before the 1st segout of the next version
                id1 = GetHeadSegOutId(context, tags[tagIndex + 1]);
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

        // nothing to restore
        if (id1 == -1) return (-1, 0);

        // adjust IDs based on the target tree structure
        if (IncludeDeletions)
        {
            id1 = AdjustReferenceId(root, id1);
            if (id2 > 0) id2 = AdjustReferenceId(root, id2);
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

        // look for the first node with seg-out feature in TAG_ID keys
        foreach (string key in context.NodeFeatures.Keys
            .Where(k => k.StartsWith(prefix)))
        {
            // get the requested feature if present
            OperationFeature? f = context.NodeFeatures[key].FirstOrDefault(
                f => f.Name == OperationFeature.F_TRACE_SEG_OUT);

            // if found, add the ID with its ordinal
            if (f != null)
            {
                // extract NR from value (OPID TAGIN:TAGOUT NR)
                int i = f.Value!.LastIndexOf(' ') + 1;
                int nr = int.Parse(f.Value.AsSpan(i));

                // if NR is 1 (=segment head), return the ID of the node
                if (nr == 1) return int.Parse(key.AsSpan(prefix.Length));
            }
        }

        return 0;
    }

    private void RestoreDeletions(ChainOperationContext<char> context,
        IList<CharChainOperation> operations,
        IList<string> tags, int tagStartIndex, TreeNode<ExportedSegment> root)
    {
        int i = tagStartIndex - 1;
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
            string value = context.NodeFeatures[$"{tags[i]}_{inIds[0]}"]
                .First(f => f.Name == OperationFeature.F_TRACE_SEG_IN).Value!;
            string opId = value[..value.IndexOf(' ')];

            // get the corresponding operation
            CharChainOperation op = operations.First(o => o.Id == opId);

            // get the reference node ID(s) for inserting restored nodes
            var (id1, id2) = GetRefNodeId(context, tags, i, op,
                inIds[0], inIds[^1], in2Ids.Count > 0 ? in2Ids[0] : 0,
                root);

            // we care only about operations which imply deletions
            if (id1 > -1)
            {
                // feature prefix to be used when creating the new nodes
                // for their seg-in features (OPID TAGIN:TAGOUT + space)
                string featPrefix =
                    $"{opId} {tags[i]}:{tags[i + 1]} ";

                // insert seg-in nodes before reference node:
                // collect source nodes in the order specified by inIds
                List<ChainNode<char>> inNodes =
                    CollectNodes(context, tags[i], inIds);

                // insert the collected nodes
                InsertNodesBefore(inNodes, root, featPrefix, id1);

                if (in2Ids.Count > 0)
                {
                    // insert seg2-in nodes before reference node:
                    // collect source nodes in the order specified by in2Ids
                    List<ChainNode<char>> in2Nodes =
                        CollectNodes(context, tags[i], in2Ids);

                    // insert the collected nodes
                    InsertNodesBefore(in2Nodes, root, featPrefix, id2);
                }
            }

            // move to previous version
            i--;
        }
    }

    private static void FlattenTraceFeatures(List<NamedValue> nodeTraceFeats,
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
                        Text = source.Data.ToString(),
                        //Tags = [tag],
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
            }

            // restore deletions if requested
            if (IncludeDeletions)
                RestoreDeletions(context, operations, tags, i, root);
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
