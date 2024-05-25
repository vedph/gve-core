using System;
using System.Collections.Generic;
using System.Text;

namespace Gve.Text;

/// <summary>
/// Annotate operation for a chain of characters.
/// </summary>
/// <seealso cref="CharChainOperation" />
public sealed class CharChainAnnotateOperation : CharChainOperation
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CharChainAnnotateOperation"/>
    /// class.
    /// </summary>
    public CharChainAnnotateOperation() : base(OperationType.Annotate)
    {
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

        // add features to context
        IList<ChainNode<char>> nodes = GetReferenceNodes(context.Chain, inputTag);
        UpdateContext(context, inputTag, outputTag, nodes, nodes);
    }

    /// <summary>
    /// Parses the operation.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <returns>Operation.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="FormatException">Invalid move operation: {text}</exception>
    internal static CharChainAnnotateOperation ParseOperation(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        CharChainAnnotateOperation op = new();
        // AT RUN
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
