using Fusi.Tools;
using Fusi.Tools.Data;
using Gve.Text;
using Proteus.Rendering;
using Proteus.Rendering.Filters;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Gve.Migration;

/// <summary>
/// A class that builds a linear tree representing a text version derived from
/// a GVE chain. The built tree has a blank root node and a single branch
/// stemming from it, where each node represents a character and is parent of
/// the next character node.
/// </summary>
public sealed class GveTextTreeBuilder
{
    private sealed record TaggedId
    {
        public string Version { get; }
        public int Id { get; }
        public int Ordinal { get; }

        public TaggedId(string version, int id, int ordinal)
        {
            Version = version;
            Id = id;
            Ordinal = ordinal;
        }

        public override string ToString()
        {
            return $"{Version}_{Id}@{Ordinal}";
        }
    }

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
    public bool IncludeDeleted { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to build version trees only
    /// for those versions which are staged, i.e. marked by a global
    /// <see cref="OperationFeature.F_VERSION"/> feature.
    /// </summary>
    public bool StagedOnly { get; set; }

    /// <summary>
    /// Gets or sets the list of the text version tags to build trees for.
    /// If null or empty, all the versions will be built. Note that this filter
    /// might still be modified by <see cref="StagedOnly"/> when true.
    /// </summary>
    public HashSet<string>? VersionTags { get; set; }

    /// <summary>
    /// Get the operation ID from the specified trace feature value.
    /// The operation ID starts at the beginning of the value and ends at the first
    /// space character.
    /// </summary>
    /// <param name="value">Value or null.</param>
    /// <returns>Operation ID or null.</returns>
    private static string? GetOpid(string? value)
    {
        if (string.IsNullOrEmpty(value)) return null;
        int i = value.IndexOf(' ');
        return i < 0 ? value : value[..i];
    }

    /// <summary>
    /// Gets the text corresponding to the specified version tree.
    /// </summary>
    /// <param name="tree">The root node of the tree to get text from.</param>
    /// <param name="wrapDeleted">if set to <c>true</c>, wrap deleted text
    /// in braces.</param>
    /// <returns>Text.</returns>
    /// <exception cref="ArgumentNullException">tree</exception>
    public static string GetTreeText(TreeNode<ExportedSegment> tree,
        bool wrapDeleted = false)
    {
        ArgumentNullException.ThrowIfNull(tree);

        StringBuilder sb = new();

        if (!wrapDeleted)
        {
            tree.Traverse(node =>
            {
                if (node.Data != null) sb.Append(node.Data.Text);
                return true;
            });
            return sb.ToString();
        }

        // traverse all nodes wrapping deleted text in braces
        string? currentOpid = null;
        tree.Traverse(node =>
        {
            if (node.Data == null) return true;

            // get $del feature and its operation ID from it if any
            StringPair? deletedFeature = node.Data.Features?.FirstOrDefault(f =>
                f.Name == OperationFeature.F_TRACE_DELETED);
            string? opid = GetOpid(deletedFeature?.Value);

            // if there is a $del feature:
            if (opid != null)
            {
                // if not inside a deleted segment, we need to start one
                if (currentOpid == null)
                {
                    currentOpid = opid;
                    sb.Append('{');
                }
                // else if we are inside a deleted segment and the ID is the same,
                // we end it and start a new one
                else if (currentOpid != opid)
                {
                    // end of deleted segment
                    sb.Append('}');
                    // start of new deleted segment
                    currentOpid = opid;
                    sb.Append('{');
                }
            }
            // else if we were inside a deleted segment but this node has no
            // $del, end the segment
            else if (currentOpid != null)
            {
                // end of deleted segment
                currentOpid = null;
                sb.Append('}');
            }

            // always append the text
            sb.Append(node.Data.Text);

            return true;
        });

        // close any open deletion segment at the end
        if (currentOpid != null) sb.Append('}');

        return sb.ToString();
    }

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
    /// specified versions range.
    /// </summary>
    /// <param name="context">The execution context.</param>
    /// <param name="tags">The version tags.</param>
    /// <param name="minTagIndex">The index to the first version in
    /// <paramref name="tags"/>.</param>
    /// <param name="maxTagIndex">The index to the last version in
    /// <paramref name="tags"/>. IDs are collected from this version backwards,
    /// up to <paramref name="minTagIndex"/>.</param>
    /// <param name="secondary">if set to <c>true</c>, look for segment-2 features.
    /// </param>
    /// <returns>List of IDs, sorted by their version and then by their segment
    /// ordinal.</returns>
    private static List<TaggedId> GetIdsOfNodesWithFeature(
        ChainOperationContext<char> context, IList<string> tags,
        int minTagIndex, int maxTagIndex, bool secondary)
    {
        HashSet<TaggedId> ids = [];

        for (int i = maxTagIndex; i >= minTagIndex; i--)
        {
            // the key prefix in node features for the specified version
            string prefix = tags[i] + "_";

            // collect IDs and their ordinals
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
                    int nr = int.Parse(f.Value!.AsSpan(
                        f.Value!.LastIndexOf(' ') + 1));

                    ids.Add(new TaggedId(tags[i], id, nr));
                }
            }
        }

        // return IDs sorted by version and ordinal
        return [.. ids.OrderBy(id => id.Version).ThenBy(id => id.Ordinal)];
    }

    /// <summary>
    /// Gets the last node in the specified linear tree branch.
    /// </summary>
    /// <param name="root">The root node of the branch.</param>
    /// <returns>The last (=leaf) node.</returns>
    private static TreeNode<ExportedSegment> GetBranchLastNode(
        TreeNode<ExportedSegment> root)
    {
        TreeNode<ExportedSegment>? current = root;
        while (current?.FirstChild != null) current = current.FirstChild;
        return current!;
    }

    /// <summary>
    /// Builds a linear tree branch of export nodes from the specified source
    /// nodes.
    /// </summary>
    /// <param name="sources">The source nodes tree.</param>
    /// <param name="featPrefix">The features prefix for newly added features
    /// in built nodes.</param>
    /// <returns>The root of the built branch.</returns>
    private static TreeNode<ExportedSegment>? BuildBranch(
        List<ChainNode<char>> sources, string featPrefix)
    {
        TreeNode<ExportedSegment>? branch = null, current = null;
        int n = 1;
        foreach (ChainNode<char> source in sources)
        {
            TreeNode<ExportedSegment> newNode = new()
            {
                Label = source.Data.ToString(),
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
        return branch;
    }

    /// <summary>
    /// Gets the reference node from its node ID. Note that the ID might be
    /// duplicated because of restored nodes. So, if there is 1 ID only, use
    /// it; if there are many, pick the one which is not deleted; else pick
    /// the first (and only) deleted one.
    /// </summary>
    /// <param name="root">The root.</param>
    /// <param name="id">The node ID. When 0, null will be always returned.
    /// </param>
    /// <returns>The node or null.</returns>
    private static TreeNode<ExportedSegment>? GetRefNodeFromId(
        TreeNode<ExportedSegment> root, int id)
    {
        TreeNode<ExportedSegment>? refNode = null;
        if (id != 0)
        {
            List<TreeNode<ExportedSegment>> nodes = [];
            root.Traverse(n =>
            {
                if (n.Data?.SourceId == id) nodes.Add(n);
                return true;
            });

            switch (nodes.Count)
            {
                case 0: break;
                case 1:
                    refNode = nodes[0];
                    break;
                default:
                    refNode = nodes.FirstOrDefault(n =>
                        n.Data?.Features?.All(f =>
                            f.Name != OperationFeature.F_TRACE_DELETED) == true)
                        ?? nodes[0];
                    break;
            }
        }

        return refNode;
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
        TreeNode<ExportedSegment>? branch = BuildBranch(sources, featPrefix);
        if (branch == null) return;

        TreeNode<ExportedSegment>? refNode = GetRefNodeFromId(root, refId);

        // insert the branch before the reference node
        if (refNode != null)
        {
            // detach refNode from its parent
            TreeNode<ExportedSegment>? parent = refNode.Parent;
            parent?.Children.Remove(refNode);

            // attach refNode as the child of the last node in the branch
            TreeNode<ExportedSegment> last = GetBranchLastNode(branch);
            last.AddChild(refNode);

            // attach the branch to the original parent of refNode
            if (parent != null) parent.AddChild(branch);
            else root.AddChild(branch);
        }
        else
        {
            // insert at the end of the tree
            TreeNode<ExportedSegment>? last = GetBranchLastNode(root);

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
        TreeNode<ExportedSegment>? refNode = GetRefNodeFromId(root, refId);

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

    /// <summary>
    /// Gets the reference node ID(s) for inserting restored nodes.
    /// </summary>
    /// <param name="context">Chain operation context.</param>
    /// <param name="tags">Version tags.</param>
    /// <param name="tagIndex">Index to the current version tag in
    /// <paramref name="tags"/>.</param>
    /// <param name="op">The operation which caused the deletion.</param>
    /// <param name="headId">The ID of the head node to be restored.</param>
    /// <param name="tailId">The ID of the tail node to be restored.</param>
    /// <param name="head2Id">The ID of the 2nd head node to be restored (for
    /// swap operations).</param>
    /// <param name="root">The root of the tree node being built.</param>
    /// <returns>Node ID(s) (the second one is used for swap only), or -1,0
    /// if there is nothing to restore because the operation did not cause any
    /// deletion.</returns>
    private (int id1, int id2) GetRefNodeIds(ChainOperationContext<char> context,
        IList<string> tags, int tagIndex, CharChainOperation op,
        int headId, int tailId, int head2Id, TreeNode<ExportedSegment> root)
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
        if (IncludeDeleted)
        {
            id1 = AdjustReferenceId(root, id1);
            if (id2 > 0) id2 = AdjustReferenceId(root, id2);
        }

        return (id1, id2);
    }

    private static List<ChainNode<char>> CollectNodes(
        ChainOperationContext<char> context, string tag, IEnumerable<int> ids)
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

    /// <summary>
    /// Recursively restore the deleted nodes in the exported branch whose
    /// root node is <paramref name="root"/>, starting from the specified
    /// version tag, backwards.
    /// </summary>
    /// <param name="context">The execution context.</param>
    /// <param name="operations">The operations.</param>
    /// <param name="tags">The versions tags.</param>
    /// <param name="tagStartIndex">The index of the tag to start from,
    /// backwards.</param>
    /// <param name="root">The root of the output branch.</param>
    private void RestoreDeletions(ChainOperationContext<char> context,
        IList<CharChainOperation> operations,
        IList<string> tags, int tagStartIndex, TreeNode<ExportedSegment> root)
    {
        //Debug.WriteLine($"--RESTORE from {tagStartIndex}");//@@
        int i = tagStartIndex - 1;
        while (i > -1)
        {
            // collect IDs of all the nodes with seg-in feature in this version
            List<TaggedId> inIds = GetIdsOfNodesWithFeature(
                context, tags, i, i, false);

            // collect IDs of all the nodes with seg2-in feature in this version
            List<TaggedId> in2Ids = GetIdsOfNodesWithFeature(
                context, tags, i, i, true);

            // nothing to do if no seg-in
            if (inIds.Count == 0)
            {
                i--;
                continue;
            }

            // extract OPID from feature: all the collected nodes have the same
            // OPID, so we get it from the first one
            string value = context.NodeFeatures[$"{inIds[0].Version}_{inIds[0].Id}"]
                .First(f => f.Name == OperationFeature.F_TRACE_SEG_IN).Value!;
            string opId = value[..value.IndexOf(' ')];

            // get the corresponding operation
            CharChainOperation op = operations.First(o => o.Id == opId);

            // get the reference node ID(s) for inserting restored nodes
            var (id1, id2) = GetRefNodeIds(context, tags, i, op,
                inIds[0].Id, inIds[^1].Id, in2Ids.Count > 0 ? in2Ids[0].Id : 0,
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
                    CollectNodes(context, tags[i], inIds.Select(id => id.Id));

                // insert the collected nodes
                InsertNodesBefore(inNodes, root, featPrefix, id1);

                if (in2Ids.Count > 0)
                {
                    // insert seg2-in nodes before reference node:
                    // collect source nodes in the order specified by in2Ids
                    List<ChainNode<char>> in2Nodes =
                        CollectNodes(context, tags[i], in2Ids.Select(id => id.Id));

                    // insert the collected nodes
                    InsertNodesBefore(in2Nodes, root, featPrefix, id2);
                }
            }

            // move to previous version
            //Debug.WriteLine($"i={i} tag={tags[i]}: \"{GetTreeText(root, true)}\"");//@@
            i--;
        }
    }

    private static string? GetVersionFromTag(ChainOperationContext<char> context,
        string tag)
    {
        // we allow for null for v0
        ChainOperationContextStep? step = context.Steps.FirstOrDefault(
            s => s.OutputTag == tag);

        return step?.FeatureSet.Features.FirstOrDefault(
            f => f.Name == OperationFeature.F_VERSION)?.Value;
    }

    private static TreeNode<ExportedSegment> CreateSubTree(
        string tag, string? version)
    {
        TreeNode<ExportedSegment> root = new()
        {
            Data = new()
        };

        // add sub-id
        root.Data.AddFeature(CompositeTextTreeFilter.F_SUBTREE_ID, tag);

        // add version if any
        if (version != null)
            root.Data.AddFeature(OperationFeature.F_VERSION, version);

        return root;
    }

    /// <summary>
    /// Builds a "multi"-trees wrapper tree consisting of a blank root node
    /// having one child linear tree per version.
    /// This method builds the tree for each version tag with the features
    /// selected in <see cref="FeatureNames"/>. If this is not set, all features
    /// will be included.
    /// </summary>
    /// <param name="context">The execution context.</param>
    /// <param name="operations">The operations executed on the context.</param>
    /// <returns>The blank root of a tree having one child blank node per version,
    /// representing the root of a linear tree for that version.</returns>
    /// <exception cref="ArgumentNullException">chain or operations</exception>
    public TreeNode<ExportedSegment> Build(
        ChainOperationContext<char> context, IList<CharChainOperation> operations)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(operations);

        TreeNode<ExportedSegment> multi = new();

        // setup a cache for trace features as we process each version:
        // key = node ID, value = set of unique node features
        Dictionary<int, HashSet<StringPair>> cachedTraceFeats = [];

        // for each version tag
        IList<string> tags = context.Chain.GetDistinctTags();
        for (int i = 0; i < tags.Count; i++)
        {
            string tag = tags[i];
            bool addSub = true;

            // if staged-only version, skip if this is not staged
            string? version = GetVersionFromTag(context, tag);
            if (StagedOnly && version == null) addSub = false;

            // if version tags are set, skip if this is not in the list
            else if (VersionTags?.Count > 0 && !VersionTags.Contains(tag))
                addSub = false;

            // get the nodes for this tag
            IList<ChainNode<char>> tagNodes = context.Chain.GetTagNodes(tag);

            // get the features for these nodes
            Dictionary<int, IList<Feature>> tagFeats =
                context.GetTagNodeFeatures(tag);

            // create blank root and add it to the multi-tree as a child
            // with version tag as its subtree ID
            TreeNode<ExportedSegment>? sub =
                addSub ? CreateSubTree(tag, version) : null;

            // add sub-tree to multi-tree
            if (sub != null) multi.AddChild(sub);

            // add sub-tree descendants
            TreeNode<ExportedSegment>? current = sub;

            // for each node in current version tag nodes
            foreach (ChainNode<char> tagNode in tagNodes)
            {
                // get node features
                IList<Feature> nodeFeats = tagFeats.TryGetValue(
                    tagNode.Id, out IList<Feature>? value) ? value : [];

                // collect only the selected features for the target node
                HashSet<StringPair> targetFeats = [];
                foreach (Feature feature in nodeFeats
                    .Where(f => IsFeatureIncluded(f.Name)))
                {
                    targetFeats.Add(new StringPair(feature.Name, feature.Value));
                }

                // merge trace features from previous versions if requested
                if (TraceFeatureFlattening && cachedTraceFeats.TryGetValue(
                    tagNode.Id, out HashSet<StringPair>? nodeTraceFeats))
                {
                    targetFeats.UnionWith(nodeTraceFeats);
                }

                // add child target node with text, version tag and features
                if (sub != null)
                {
                    current = current!.AddChild(new TreeNode<ExportedSegment>
                    {
                        Label = tagNode.Data.ToString(),
                        Data = new ExportedSegment
                        {
                            SourceId = tagNode.Id,
                            Text = tagNode.Data.ToString(),
                            //Tags = [tag],
                            Features = targetFeats.Count == 0
                                ? null : [.. targetFeats]
                        }
                    });
                }

                // if required by flattening, cache all the trace features
                // from the target node for the next iteration
                if (TraceFeatureFlattening && nodeFeats.Any(f =>
                    f.Name.StartsWith(OperationFeature.F_TRACE_PREFIX)))
                {
                    if (!cachedTraceFeats.ContainsKey(tagNode.Id))
                        cachedTraceFeats[tagNode.Id] = [];

                    foreach (Feature feature in nodeFeats
                        .Where(f => f.Name.StartsWith(
                            OperationFeature.F_TRACE_PREFIX)))
                    {
                        cachedTraceFeats[tagNode.Id].Add(
                            new StringPair(feature.Name, feature.Value));
                    }
                }
            }

            // restore deletions if requested
            if (IncludeDeleted && sub != null)
                RestoreDeletions(context, operations, tags, i, sub);
        }

        return multi;
    }

    /// <summary>
    /// Builds versions linear trees from the specified base text and operations
    /// on it. This method builds the tree for each version tag with the features
    /// selected in <see cref="FeatureNames"/>. If this is not set, all features
    /// will be included.
    /// </summary>
    /// <returns>A dictionary where each version tag is the key and its value
    /// is the root blank node of the corresponding linear text tree.</returns>
    /// <exception cref="ArgumentNullException">text or operations</exception>
    public TreeNode<ExportedSegment> Build(string text,
        IList<CharChainOperation> operations)
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
