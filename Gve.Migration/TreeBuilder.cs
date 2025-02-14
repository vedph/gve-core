using Fusi.Tools.Data;
using Gve.Text;
using Gve.Text.DTO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Gve.Migration;

/// <summary>
/// Tree builder for a chain operation context.
/// </summary>
public static class TreeBuilder
{
    /// <summary>
    /// The name of the tag feature added to the data payload of each tree node
    /// to indicate the version it belongs to.
    /// </summary>
    public const string TAG_FEATURE_NAME = "_tag";

    private static List<CharChainNode> BuildVersionNodes(
         IList<ChainNode<char>> nodes,
         ChainOperationContext<char> context,
         string tag)
    {
        List<CharChainNode> taggedNodes = [];
        int i = 0;
        foreach (ChainNode<char> node in nodes)
        {
            IList<Feature> feats = context.GetNodeFeatures(node.Id, tag);
            CharChainNode m = new()
            {
                Id = node.Id,
                Index = i++,
                Label = node.Label,
                Data = node.Data,
                SourceTag = node.SourceTag,
                Features = feats.Count == 0 ? null : feats
            };
            taggedNodes.Add(m);
        }
        return taggedNodes;
    }

    private static (TreeNode<CharChainNode> node, int matchLength) FindCommonPrefix(
        TreeNode<CharChainNode> root,
        List<CharChainNode> nodes,
        bool binary)
    {
        if (nodes.Count == 0 || !root.HasChildren) return (root, 0);

        TreeNode<CharChainNode> current = root;
        int matchLength = 0;

        while (current.HasChildren && matchLength < nodes.Count)
        {
            bool foundMatch = false;
            foreach (TreeNode<CharChainNode> child in current.Children)
            {
                if (child.Data == null)
                {
                    (TreeNode<CharChainNode> deepNode, int deepMatchLength) =
                        FindCommonPrefix(child, [.. nodes.Skip(matchLength)],
                        binary);

                    if (deepMatchLength > 0)
                    {
                        current = deepNode;
                        matchLength += deepMatchLength;
                        foundMatch = true;
                        break;
                    }
                }
                else if (child.Data.Data == nodes[matchLength].Data)
                {
                    current = child;
                    matchLength++;
                    foundMatch = true;
                    break;
                }
            }
            if (!foundMatch) break;
        }

        return (current, matchLength);
    }

    /// <summary>
    /// Adds the specified version tag to node and all its ascendant nodes
    /// up to the root.
    /// </summary>
    /// <param name="node">The node.</param>
    /// <param name="tag">The tag.</param>
    private static void AddVersionTagToNode(TreeNode<CharChainNode>? node,
        string tag)
    {
        while (node != null)
        {
            if (node.Data != null)
            {
                node.Data.Features ??= [];
                if (!node.Data.Features.Any(f =>
                    f.Name == TAG_FEATURE_NAME && f.Value == tag))
                {
                    node.Data.Features.Add(new Feature(TAG_FEATURE_NAME, tag,
                        FeatureSetPolicy.Multiple));
                }
            }
            node = node.Parent;
        }
    }

    /// <summary>
    /// Creates a node branch for the specified nodes.
    /// </summary>
    /// <param name="nodes">The nodes to add to the new branch.</param>
    /// <param name="startIndex">The start node index in nodes.</param>
    /// <param name="tag">The version tag.</param>
    /// <returns>Branch node or null.</returns>
    private static TreeNode<CharChainNode>? CreateNodeBranch(
        List<CharChainNode> nodes, int startIndex, string tag)
    {
        if (startIndex >= nodes.Count) return null;

        // create first node
        CharChainNode firstNode = nodes[startIndex].Clone();
        firstNode.Features ??= [];
        firstNode.Features.Add(new Feature(TAG_FEATURE_NAME, tag,
            FeatureSetPolicy.Multiple));
        TreeNode<CharChainNode> branch = new(firstNode);

        // add remaining nodes
        TreeNode<CharChainNode> current = branch;
        for (int i = startIndex + 1; i < nodes.Count; i++)
        {
            CharChainNode node = nodes[i].Clone();
            node.Features ??= [];
            node.Features.Add(new Feature(TAG_FEATURE_NAME, tag,
                FeatureSetPolicy.Multiple));
            TreeNode<CharChainNode> next = new(node);
            current.AddChild(next);
            current = next;
        }

        return branch;
    }

    private static void AddBranchToBinaryTree(
        TreeNode<CharChainNode> parent, List<CharChainNode> nodes,
        int startIndex, string tag)
    {
        if (startIndex >= nodes.Count) return;

        TreeNode<CharChainNode>? newBranch = CreateNodeBranch(
            nodes, startIndex, tag);
        if (newBranch == null) return;

        // if parent has no children, just add the branch
        if (!parent.HasChildren)
        {
            parent.AddChild(newBranch);
            return;
        }

        // if parent already has a blank fork node child, use it
        if (parent.Children.Count == 1 && parent.Children[0].Data == null)
        {
            TreeNode<CharChainNode> fork = parent.Children[0];
            if (fork.Children.Count < 2)
            {
                fork.AddChild(newBranch);
            }
            else
            {
                // create new fork node
                TreeNode<CharChainNode> newFork = new(null);
                TreeNode<CharChainNode> secondChild = fork.Children[1];
                fork.Children[1] = newFork;
                newFork.Parent = fork;
                newFork.AddChild(secondChild);
                newFork.AddChild(newBranch);
            }
            return;
        }

        // create new blank fork node
        TreeNode<CharChainNode> blankFork = new(null);

        // if parent has one child, move it under fork
        if (parent.Children.Count == 1)
        {
            TreeNode<CharChainNode> existingChild = parent.Children[0];
            parent.Children.Clear();
            parent.AddChild(blankFork);
            blankFork.AddChild(existingChild);
            blankFork.AddChild(newBranch);
        }
        // else parent has multiple children
        else
        {
            List<TreeNode<CharChainNode>> existingChildren = [.. parent.Children];
            parent.Children.Clear();
            parent.AddChild(blankFork);

            // first child gets original branches
            TreeNode<CharChainNode> subFork = new(null);
            blankFork.AddChild(subFork);
            foreach (TreeNode<CharChainNode>? child in existingChildren)
                subFork.AddChild(child);

            // second child gets new branch
            blankFork.AddChild(newBranch);
        }
    }

    /// <summary>
    /// Builds a tree from the specified chain operation execution context.
    /// Each chain version, starting from the latest, is projected on the tree
    /// by reusing its starting nodes and adding new nodes as needed.
    /// This reduces the multiple dimensions of the chain graph to the two
    /// dimensions of a tree, of course incurring in nodes duplication.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="binary">True to use binary behavior, which allows max 2
    /// children per node. When more than 2 children are needed, we insert a
    /// blank fork node having as first child the original node and as second
    /// the new one.</param>
    /// <returns>Root node of the tree. Each node except the root has payload
    /// data equal to a cloned instance of the corresponding chain node, with
    /// an added <see cref="TAG_FEATURE_NAME"/> feature to define the version
    /// tag it belongs to.</returns>
    /// <exception cref="ArgumentNullException">context</exception>
    public static TreeNode<CharChainNode> Build(
        ChainOperationContext<char> context, bool binary)
    {
        ArgumentNullException.ThrowIfNull(context);
        TreeNode<CharChainNode> root = new(null);

        // process tags in reverse order (newest to oldest)
        IList<string> tags = [.. context.Chain.GetDistinctTags()
            .Where(s => !string.IsNullOrEmpty(s))
            .Reverse()];

        foreach (string tag in tags)
        {
            // get nodes for this version
            List<CharChainNode> versionNodes = BuildVersionNodes(
                context.Chain.GetTagNodes(tag), context, tag);

            // find where this version branches from existing tree
            (TreeNode<CharChainNode> branchNode, int matchLength) =
                FindCommonPrefix(root, versionNodes, binary);

            // add version tag to matching nodes
            if (matchLength > 0) AddVersionTagToNode(branchNode, tag);

            if (binary)
            {
                // add remaining nodes as new binary branch
                AddBranchToBinaryTree(branchNode, versionNodes, matchLength, tag);
            }
            else
            {
                // non-binary behavior
                TreeNode<CharChainNode> currentNode = branchNode;
                for (int i = matchLength; i < versionNodes.Count; i++)
                {
                    CharChainNode node = versionNodes[i].Clone();
                    node.Features ??= [];
                    node.Features.Add(new Feature(TAG_FEATURE_NAME, tag,
                        FeatureSetPolicy.Multiple));

                    TreeNode<CharChainNode> newNode = new(node);
                    currentNode.AddChild(newNode);
                    currentNode = newNode;
                }
            }

            //Debug.WriteLine("=== TAG: " + tag);
            //Debug.WriteLine(root.Dump());
        }

        return root;
    }

    /// <summary>
    /// Dumps the text versions with their tags, as represented in the received
    /// tree root node.
    /// </summary>
    /// <param name="root">The tree's root node.</param>
    /// <returns>Dictionary where key=tag and value=text, or null if root has no
    /// children.</returns>
    /// <exception cref="ArgumentNullException">root</exception>
    public static IDictionary<string, string>? DumpVersions(
        TreeNode<CharChainNode> root)
    {
        ArgumentNullException.ThrowIfNull(root);

        if (!root.HasChildren) return null;
        Dictionary<string, StringBuilder> versions = [];

        root.Traverse(node =>
        {
            if (node.Data?.Features != null)
            {
                foreach (string tag in node.Data.Features
                    .Where(f => f.Name == TAG_FEATURE_NAME)
                    .Select(f => f.Value!))
                {
                    if (!versions.ContainsKey(tag))
                        versions[tag] = new StringBuilder();
                    versions[tag].Append(node.Data.Data);
                }
            }
            return true;
        });

        SortedDictionary<string, string> results = [];
        foreach (KeyValuePair<string, StringBuilder> kvp in versions)
            results[kvp.Key] = kvp.Value.ToString();
        return results;
    }

    /// <summary>
    /// Builds the code for a Graphviz tree diagram representing the received
    /// tree.
    /// </summary>
    /// <param name="root">The root node of the tree.</param>
    /// <returns>Graphviz code.</returns>
    /// <exception cref="ArgumentNullException">root</exception>
    public static string BuildGraph(TreeNode<CharChainNode> root)
    {
        ArgumentNullException.ThrowIfNull(root);

        StringBuilder sb = new();
        sb.AppendLine("digraph G {");
        int id = 0;
        Dictionary<TreeNode<CharChainNode>, int> ids = [];

        root.Traverse(node =>
        {
            ids[node] = ++id;
            if (node.Parent != null)
                sb.AppendLine($"  {ids[node.Parent]} -> {ids[node]};");

            if (node.Data != null)
            {
                string tags = node.Data.Features != null
                    ? string.Join(" ", node.Data.Features
                        .Where(f => f.Name == TAG_FEATURE_NAME)
                        .Select(f => f.Value)
                        .Order())
                    : "";
                sb.AppendLine(tags.Length > 0
                    ? $"  {ids[node]} [label=\"{node.Data.Data}\", " +
                      $"color=\"#c0c0f0\", style=\"filled\", xlabel=\"{tags}\"];"
                    : $"  {ids[node]} [label=\"{node.Data.Data}\", " +
                       "color=\"#c0c0f0\", style=\"filled\"];");
            }
            else
            {
                sb.AppendLine($"  {ids[node]} [label=\"-\"];");
            }

            return true;
        });

        sb.AppendLine("}");
        return sb.ToString();
    }
}
