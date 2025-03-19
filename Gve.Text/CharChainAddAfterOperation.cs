using System;
using System.Collections.Generic;
using System.Text;

namespace Gve.Text;

/// <summary>
/// An add-after operation for a chain of characters.
/// </summary>
/// <seealso cref="CharChainOperation" />
public sealed class CharChainAddAfterOperation : CharChainOperation
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CharChainAddAfterOperation"/>
    /// class.
    /// </summary>
    public CharChainAddAfterOperation() : base(OperationType.AddAfter)
    {
        Run = 0;
    }

    /// <summary>
    /// Executes this operation on the specified context.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <exception cref="ArgumentNullException">context</exception>
    /// <exception cref="InvalidOperationException">non-zero run</exception>
    public override void Execute(ChainOperationContext<char> context)
    {
        ArgumentNullException.ThrowIfNull(context);

        // nothing to do if empty value
        if (string.IsNullOrEmpty(Value)) return;

        // get tags
        (string inputTag, string outputTag) = GetTags(context);

        // get ref node
        IList<ChainNode<char>> nodes = GetReferenceNodes(context.Chain, inputTag);

        if (nodes.Count > 1)
        {
            throw new InvalidOperationException(
                "Add-after operation cannot have a RUN different from 0.");
        }

        // add anchor feature to reference node
        if (!NoTraceFeatures)
        {
            OperationFeature f = GetTraceFeature(F_TRACE_ANCHOR, inputTag);
            context.UpdateTraceFeatures(f, inputTag, nodes[0]);
        }

        // apply to single node or range
        if (Value.Length == 1)
        {
            context.Chain.InsertAfter(inputTag, outputTag, nodes[0],
                GetNodesFrom(Value)[0]);
        }
        else
        {
            context.Chain.InsertRangeAfter(inputTag, outputTag, nodes[0],
                GetNodesFrom(Value));
        }

        // get added nodes and add to each an opid feature with the value
        // equal to the ID of this operation
        IList<ChainNode<char>> addedNodes = context.Chain.GetTagNodes(
            outputTag, true);
        context.UpdateFeatures(
            [new OperationFeature(OPID_KEY, Id, FeatureSetPolicy.Single)],
            outputTag, addedNodes);

        // update context metadata (target nodes are the added nodes)
        UpdateContext(context, inputTag, outputTag, nodes, addedNodes);

        // add segment features to added nodes
        AddNodeTraceFeatures(inputTag, outputTag, true, addedNodes, context);
    }

    /// <summary>
    /// Parses the specified text representing a replace operation.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <returns>Operation.</returns>
    /// <exception cref="ArgumentNullException">text</exception>
    internal static CharChainAddAfterOperation ParseOperation(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        CharChainAddAfterOperation op = new();

        string body = ParseHeadAndTail(text, op);
        op.Value = body.Trim('"');

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
        sb.Append('"').Append(Value).Append('"');
        AppendTail(sb);
        return sb.ToString();
    }
}
