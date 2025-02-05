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

    private static (TreeNode<CharChainNode> node, int length)
        FindLastEqualNodeRecursive(TreeNode<CharChainNode> currentNode,
        List<CharChainNode> nodes, int index)
    {
        if (!currentNode.HasChildren || index >= nodes.Count ||
            (currentNode.Data?.Data != null &&
             currentNode.Data.Data != nodes[index].Data))
        {
            return (currentNode, index);
        }

        TreeNode<CharChainNode> bestMatchNode = currentNode;
        int bestMatchLength = index;

        foreach (TreeNode<CharChainNode> child in currentNode.Children)
        {
            var (matchedNode, matchedLength) = FindLastEqualNodeRecursive(
                child, nodes, index + 1);
            if (matchedLength > bestMatchLength)
            {
                bestMatchNode = matchedNode;
                bestMatchLength = matchedLength;
            }
        }

        return (bestMatchNode, bestMatchLength);
    }

    private static (TreeNode<CharChainNode> node, int length) FindNodeForBranching(
        TreeNode<CharChainNode> root, List<CharChainNode> nodes)
    {
        if (!root.HasChildren) return (root, 0);

        return FindLastEqualNodeRecursive(root, nodes, 0);
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

        // for each tag starting from the last one
        IList<string> tags = context.Chain.GetDistinctTags();
        foreach (string tag in tags.Where(s => !string.IsNullOrEmpty(s))
            .Reverse())
        {
            // get the nodes tagged with this version
            List<CharChainNode> verNodes = BuildVersionNodes(
                context.Chain.GetTagNodes(tag), context, tag);

            // find the branching node for this version
            (TreeNode<CharChainNode> branchNode, int length) =
                FindNodeForBranching(root, verNodes);

            // add the remaining nodes as children of the branching node
            if (length > 0)
            {
                // add a new tag feature to branch node data and to all its
                // ascendants up to the root, excluded
                TreeNode<CharChainNode> node = branchNode;
                while (node?.Data != null)
                {
                    node.Data!.Features ??= [];
                    node.Data.Features.Add(new Feature(TAG_FEATURE_NAME, tag,
                        FeatureSetPolicy.Multiple));
                    node = node.Parent!;
                }
            }

            foreach (CharChainNode node in verNodes.Skip(length))
            {
                CharChainNode clone = node.Clone();
                clone.Features ??= [];
                clone.Features.Add(new Feature(TAG_FEATURE_NAME, tag,
                    FeatureSetPolicy.Multiple));

                TreeNode<CharChainNode> child = new(clone);
                branchNode.AddChild(child);
                branchNode = child;
            }
        }

        return root;
    }
}
