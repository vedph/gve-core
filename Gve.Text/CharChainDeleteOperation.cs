using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gve.Text;

/// <summary>
/// "Delete" operation for a chain.
/// </summary>
public sealed class CharChainDeleteOperation : CharChainOperation
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CharChainDeleteOperation"/>
    /// class.
    /// </summary>
    public CharChainDeleteOperation() : base(OperationType.Delete)
    {
    }

    private void AddAdjacentNodeTraceFeature(string inputTag, string outputTag,
        string name, string tail,
        ChainNode<char> node, ChainOperationContext<char> context)
    {
        OperationFeature f = NewTraceFeature(name, inputTag, outputTag, tail);
        context.UpdateNodeFeatures<char>(f, outputTag, node);
    }

    /// <summary>
    /// Executes this operation on the specified context.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <exception cref="ArgumentNullException">context</exception>
    public override void Execute(ChainOperationContext<char> context)
    {
        ArgumentNullException.ThrowIfNull(context);

        // get tags
        (string inputTag, string outputTag) = GetTags(context);

        // get target nodes
        IList<ChainNode<char>> nodes = GetReferenceNodes(context.Chain, inputTag);
        ChainNode<char>? leftNode = context.Chain.GetPreviousNode(
            inputTag, nodes[0]);
        ChainNode<char>? rightNode = context.Chain.GetNextNode(
            inputTag, nodes[^1]);
        string tail = string.Join(",", nodes.Select(n => n.Id.ToString()));

        // add trace features to nodes before executing the operation
        AddNodeTraceFeatures(inputTag, outputTag, false, nodes, context);

        // apply to single node or range
        if (nodes.Count > 1)
        {
            context.Chain.DeleteRange(inputTag, outputTag, nodes[0], nodes[^1]);
        }
        else
        {
            context.Chain.Delete(inputTag, outputTag, nodes[0]);
        }

        // trace features for nodes after executing the operation
        if (leftNode is not null)
        {
            AddAdjacentNodeTraceFeature(inputTag, outputTag,
                OperationFeature.F_TRACE_LEFT_ANCHOR, tail,
                leftNode, context);
        }
        if (rightNode is not null)
        {
            AddAdjacentNodeTraceFeature(inputTag, outputTag,
                OperationFeature.F_TRACE_RIGHT_ANCHOR, tail,
                rightNode, context);
        }

        // update context metadata
        UpdateContext(context, inputTag, outputTag, nodes,
            GetReferenceNodes(context.Chain, inputTag));

        // add delete feature
        for (int i = 0; i < nodes.Count; i++)
        {
            context.UpdateNodeFeatures<char>(
                NewTraceFeature(DEL_KEY, inputTag, outputTag, i + 1),
                outputTag, nodes[i]);
        }
    }

    /// <summary>
    /// Parses the operation. A delete operation has format (whitespaces here
    /// are added to separate components):
    /// <c>(IT:OT) @ATxRUN OP ^RANK @GROUP [TAGS] {META}</c>,
    /// where AT and OP are the only required components.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <returns>Operation.</returns>
    /// <exception cref="ArgumentNullException">text</exception>
    internal static CharChainDeleteOperation ParseOperation(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        CharChainDeleteOperation op = new();
        ParseHeadAndTail(text, op);
        return op;
    }

    /// <summary>
    /// Converts to string.
    /// </summary>
    /// <returns>
    /// A <see cref="string" /> that represents this instance.
    /// </returns>
    public override string ToString()
    {
        StringBuilder sb = new();
        AppendHead(sb);
        AppendTail(sb);
        return sb.ToString();
    }
}
