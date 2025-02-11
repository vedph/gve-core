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
      List<CharChainNode> nodes)
    {
        if (nodes.Count == 0 || !root.HasChildren) return (root, 0);

        TreeNode<CharChainNode> current = root;
        int matchLength = 0;

        while (current.HasChildren && matchLength < nodes.Count)
        {
            bool foundMatch = false;
            foreach (TreeNode<CharChainNode> child in current.Children)
            {
                if (child.Data?.Data == nodes[matchLength].Data)
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

    /// <summary>
    /// Builds a tree from the specified chain operation execution context.
    /// Each chain version, starting from the latest, is projected on the tree
    /// by reusing its starting nodes and adding new nodes as needed.
    /// This reduces the multiple dimensions of the chain graph to the two
    /// dimensions of a tree, of course incurring in nodes duplication.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <returns>Root node of the tree. Each node except the root has payload
    /// data equal to a cloned instance of the corresponding chain node, with
    /// an added <see cref="TAG_FEATURE_NAME"/> feature to define the version
    /// tag it belongs to.</returns>
    /// <exception cref="ArgumentNullException">context</exception>
    public static TreeNode<CharChainNode> Build(
          ChainOperationContext<char> context)
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
                FindCommonPrefix(root, versionNodes);

            // add version tag to matching nodes
            if (matchLength > 0) AddVersionTagToNode(branchNode, tag);

            // add remaining nodes as new branch
            TreeNode<CharChainNode> currentNode = branchNode;
            for (int i = matchLength; i < versionNodes.Count; i++)
            {
                CharChainNode node = versionNodes[i];
                CharChainNode clone = node.Clone();
                clone.Features ??= [];
                clone.Features.Add(new Feature(TAG_FEATURE_NAME, tag,
                    FeatureSetPolicy.Multiple));

                TreeNode<CharChainNode> newNode = new(clone);
                currentNode.AddChild(newNode);
                currentNode = newNode;
            }
        }

        return root;
    }
}
