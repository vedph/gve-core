using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Gve.Text;

/// <summary>
/// Move-after operation for a chain of characters.
/// </summary>
/// <seealso cref="CharChainOperation" />
public sealed class CharChainMoveAfterOperation : CharChainOperation,
    IHasToArgument
{
    /// <summary>
    /// Gets or sets a value indicating whether <see cref="To"/> refers to
    /// an index rather than a chain node ID.
    /// </summary>
    public bool ToAsIndex { get; set; }

    /// <summary>
    /// Gets or sets the reference position.
    /// </summary>
    public int To { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CharChainMoveAfterOperation"/>
    /// class.
    /// </summary>
    public CharChainMoveAfterOperation() : base(OperationType.MoveAfter)
    {
    }

    private ChainNode<char> GetToNode(Chain<char> chain, string tag)
    {
        return FindNode(chain, To, ToAsIndex, tag) ??
            throw new InvalidOperationException(
            "Node not found for the specified TO: " + ToString());
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

        // get tags
        (string inputTag, string outputTag) = GetTags(context);

        // get target nodes
        IList<ChainNode<char>> nodes = GetReferenceNodes(context.Chain, inputTag);

        // get to node
        ChainNode<char> toNode = GetToNode(context.Chain, inputTag);

        // add trace features before moving the input segment
        AddNodeTraceFeatures(inputTag, outputTag, false, nodes, context);
        // add anchor feature to reference node
        if (!NoTraceFeatures)
        {
            OperationFeature f = NewTraceFeature(OperationFeature.F_TRACE_ANCHOR,
                inputTag, outputTag);
            context.UpdateTraceFeatures(f, inputTag, toNode);
        }

        // apply to single node or range
        if (nodes.Count > 1)
        {
            context.Chain.MoveRangeAfter(inputTag, outputTag, nodes[0], nodes[^1],
                toNode);
        }
        else
        {
            context.Chain.MoveAfter(inputTag, outputTag, nodes[0], toNode);
        }

        // update context metadata
        UpdateContext(context, inputTag, outputTag, nodes, nodes);

        // add segment features to added nodes
        AddNodeTraceFeatures(inputTag, outputTag, true, nodes, context);
    }

    /// <summary>
    /// Parses the operation.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <returns>Operation.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="FormatException">Invalid move operation: {text}</exception>
    internal static CharChainMoveAfterOperation ParseOperation(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        CharChainMoveAfterOperation op = new();
        // AT RUN, then [@]TO consumed right after the head so that a
        // leading @ in TO is not mistaken for the tail's @GROUP marker
        ParseHeadAndTail(text, op, afterHead =>
        {
            Match m = ToRegex().Match(afterHead);
            if (!m.Success)
                throw new FormatException($"Invalid move operation: {text}");
            op.ToAsIndex = m.Groups["tox"].Success;
            op.To = int.Parse(m.Groups["to"].Value, CultureInfo.InvariantCulture);
            return m.Length;
        });

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
        // TO
        if (ToAsIndex) sb.Append('@');
        sb.Append(To);
        AppendTail(sb);
        return sb.ToString();    }

}
