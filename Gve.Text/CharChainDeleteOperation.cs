using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

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

    /// <summary>
    /// Adds more operation-specific node trace features after the generic ones
    /// have been added. This adds the deleted feature to each deleted node.
    /// </summary>
    /// <param name="inputTag">The input tag.</param>
    /// <param name="outputTag">The output tag.</param>
    /// <param name="afterExecution">Not used.</param>
    /// <param name="nodes">The nodes.</param>
    /// <param name="context">The context.</param>
    protected override void AddMoreNodeTraceFeatures(string inputTag,
        string outputTag, bool afterExecution,
        IList<ChainNode<char>> nodes, ChainOperationContext<char> context)
    {
        if (NoTraceFeatures) return;

        for (int i = 0; i < nodes.Count; i++)
        {
            OperationFeature f = NewTraceFeature(
                OperationFeature.F_TRACE_DELETED,
                inputTag, outputTag,
                i + 1);
            context.UpdateTraceFeatures(f, outputTag, nodes[i]);
        }
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

        // update context metadata
        UpdateContext(context, inputTag, outputTag, nodes,
            GetReferenceNodes(context.Chain, inputTag));

        // no trace features for nodes after executing the operation
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
