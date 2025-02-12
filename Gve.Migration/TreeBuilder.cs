using Fusi.Tools.Data;
using Gve.Text;
using Gve.Text.DTO;
using System;
using System.Collections.Generic;
using System.Linq;

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
                // skip blank fork nodes in matching
                if (child.Data == null)
                {
                    var (deepNode, deepMatchLength) = FindCommonPrefix(child,
                        nodes.Skip(matchLength).ToList(), binary);
                    if (deepMatchLength > 0)
                    {
                        current = deepNode;
                        matchLength += deepMatchLength;
                        foundMatch = true;
                        break;
                    }
                    continue;
                }

                if (child.Data.Data == nodes[matchLength].Data)
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

    private static void AddVersionTagToNode(TreeNode<CharChainNode> node,
        string tag)
    {
        while (node?.Data != null)
        {
            node.Data.Features ??= [];
            if (!node.Data.Features.Any(f =>
                f.Name == TAG_FEATURE_NAME && f.Value == tag))
            {
                node.Data.Features.Add(new Feature(TAG_FEATURE_NAME, tag,
                    FeatureSetPolicy.Multiple));
            }
            node = node.Parent!;
        }
    }

    private static void InsertBlankForkNode(
        TreeNode<CharChainNode> parent,
        TreeNode<CharChainNode> existingChild,
        TreeNode<CharChainNode> newChild)
    {
        // Create blank fork node
        TreeNode<CharChainNode> forkNode = new(null);

        // Replace existing child with fork node
        int index = parent.Children.IndexOf(existingChild);
        parent.Children[index] = forkNode;
        forkNode.Parent = parent;

        // Add existing child as first child of fork
        forkNode.AddChild(existingChild);

        // Add new child as second child of fork
        forkNode.AddChild(newChild);
    }

    private static void AddBranchToBinaryTree(
        TreeNode<CharChainNode> parent,
        List<CharChainNode> nodes,
        int startIndex,
        string tag)
    {
        if (startIndex >= nodes.Count) return;

        // Create first node of new branch
        CharChainNode firstNode = nodes[startIndex].Clone();
        firstNode.Features ??= [];
        firstNode.Features.Add(new Feature(TAG_FEATURE_NAME, tag,
            FeatureSetPolicy.Multiple));
        TreeNode<CharChainNode> newNode = new(firstNode);

        if (parent.Children.Count == 0)
        {
            // Simple case: no children yet
            parent.AddChild(newNode);
        }
        else if (parent.Children.Count == 1)
        {
            // Add as second child if only one child exists
            parent.AddChild(newNode);
        }
        else
        {
            // Insert fork node before second child
            TreeNode<CharChainNode> secondChild = parent.Children[1];
            InsertBlankForkNode(parent, secondChild, newNode);
        }

        // Continue adding remaining nodes in the branch
        TreeNode<CharChainNode> currentNode = newNode;
        for (int i = startIndex + 1; i < nodes.Count; i++)
        {
            CharChainNode node = nodes[i].Clone();
            node.Features ??= [];
            node.Features.Add(new Feature(TAG_FEATURE_NAME, tag,
                FeatureSetPolicy.Multiple));

            TreeNode<CharChainNode> nextNode = new(node);
            currentNode.AddChild(nextNode);
            currentNode = nextNode;
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
           ChainOperationContext<char> context,
           bool binary)
    {
        ArgumentNullException.ThrowIfNull(context);
        TreeNode<CharChainNode> root = new(null);

        // process tags in reverse order (newest to oldest)
        IList<string> tags = context.Chain.GetDistinctTags()
            .Where(s => !string.IsNullOrEmpty(s))
            .Reverse()
            .ToList();

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
        }

        return root;
    }
}
