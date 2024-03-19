using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Gve.Text;

/// <summary>
/// Swap operation for a chain of characters.
/// </summary>
/// <seealso cref="CharChainOperation" />
public sealed class CharChainSwapOperation : CharChainOperation
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
    /// Gets or sets the run of characters from <see cref="To"/>.
    /// </summary>
    public int ToRun { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CharChainSwapOperation"/>
    /// class.
    /// </summary>
    public CharChainSwapOperation() : base(VarOperationType.Swap)
    {
        ToRun = 1;
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
        IList<ChainNode<char>> nodes = GetTargetNodes(context.Chain, inputTag);

        // get to target nodes
        IList<ChainNode<char>> toNodes = GetTargetNodes(context.Chain, outputTag,
            To, ToRun);

        // apply to single node or range
        if (nodes.Count > 1 || toNodes.Count > 1)
        {
            context.Chain.SwapRanges(inputTag, outputTag,
                nodes[0], nodes[^1],
                toNodes[0], toNodes[^1]);
        }
        else
        {
            context.Chain.MoveBefore(inputTag, outputTag, nodes[0], toNodes[0]);
        }

        // update context metadata
        UpdateContext(context, inputTag, outputTag);
    }

    /// <summary>
    /// Parses the operation.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <returns>Operation.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="FormatException">Invalid move operation: {text}</exception>
    internal static CharChainSwapOperation ParseOperation(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        CharChainSwapOperation op = new();
        // AT RUN
        string body = ParseHeadAndTail(text, op);
        Match m = AtRunToRegex().Match(body);
        if (!m.Success)
            throw new FormatException($"Invalid move operation: {text}");
        // TO RUN
        op.ToAsIndex = m.Groups["tox"].Success;
        op.To = short.Parse(m.Groups["to"].Value);
        op.ToRun = short.Parse(m.Groups["torun"].Value);

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
        sb.Append('x').Append(ToRun);
        AppendTail(sb);
        return sb.ToString();
    }
}
