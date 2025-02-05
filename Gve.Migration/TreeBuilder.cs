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
            currentNode.Data?.Data != nodes[index].Data)
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

    public static TreeNode<CharChainNode> Build(ChainOperationContext<char> context)
    {
        ArgumentNullException.ThrowIfNull(context);
        TreeNode<CharChainNode> root = new(null);

        // for each tag starting from the last one
        IList<string> tags = context.Chain.GetDistinctTags();
        foreach (string tag in tags.Where(s => !string.IsNullOrEmpty(s)).Reverse())
        {
            // get the nodes tagged with this version
            List<CharChainNode> verNodes = BuildVersionNodes(
                context.Chain.GetTagNodes(tag), context, tag);

            // find the branching node for this version
            (TreeNode<CharChainNode> branchNode, int length) =
                FindNodeForBranching(root, verNodes);

            // add the remaining nodes as children of the branching node
            foreach (CharChainNode node in verNodes.Skip(length))
            {
                TreeNode<CharChainNode> child = new(node);
                branchNode.AddChild(child);
                branchNode = child;
            }
        }

        return root;
    }
}
