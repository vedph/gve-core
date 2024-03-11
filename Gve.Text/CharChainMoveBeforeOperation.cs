using System;
using System.Text;
using System.Text.RegularExpressions;

namespace Gve.Text;

/// <summary>
/// A move-before operation for a chain of characters.
/// </summary>
/// <seealso cref="CharChainOperation" />
public sealed class CharChainMoveBeforeOperation : CharChainOperation
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
    /// Initializes a new instance of the <see cref="CharChainMoveBeforeOperation"/>
    /// class.
    /// </summary>
    public CharChainMoveBeforeOperation() : base(VarOperationType.MoveBefore)
    {
    }

    private ChainNode<char> GetToNode(Chain<char> chain, string tag)
    {
        return FindNode(chain, To, tag) ??
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

        // nothing to do if empty value
        if (string.IsNullOrEmpty(Value)) return;

        // get tags
        (string inputTag, string outputTag) = GetTags(context);

        // get bound nodes
        (ChainNode<char> node, ChainNode<char>? endNode) = GetBoundNodes(
            context.Chain, inputTag);

        // get to node
        ChainNode<char> toNode = GetToNode(context.Chain, inputTag);

        // apply to single node or range
        if (endNode != null)
        {
            context.Chain.MoveRangeBefore(inputTag, outputTag, node, endNode,
                toNode);
        }
        else
        {
            context.Chain.MoveBefore(inputTag, outputTag, node, toNode);
        }
    }

    /// <summary>
    /// Parses the operation.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <returns>Operation.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="FormatException">Invalid move operation: {text}</exception>
    internal static CharChainMoveBeforeOperation ParseOperation(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        CharChainMoveBeforeOperation op = new();
        // AT RUN
        string body = ParseHeadAndTail(text, op);
        Match m = AtRunToRegex().Match(body);
        if (!m.Success)
            throw new FormatException($"Invalid move operation: {text}");
        // TO
        op.ToAsIndex = m.Groups["tox"].Success;
        op.To = short.Parse(m.Groups["to"].Value);

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
        return sb.ToString();
    }
}
