using System;
using System.Collections.Generic;
using System.Text;

namespace Gve.Text;

/// <summary>
/// Annotate operation for a chain of characters. This operation does not
/// affect the text but just adds features to the selected range of nodes.
/// Arguments: AT, RUN.
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

        // clone version
        context.Chain.AddVersion(inputTag, outputTag);

        // add features to context
        IList<ChainNode<char>> nodes = GetReferenceNodes(context.Chain, outputTag);
        AddNodeTraceFeatures(inputTag, outputTag, false, nodes, context);

        UpdateContext(context, inputTag, outputTag, nodes, nodes);

        AddNodeTraceFeatures(inputTag, outputTag, true, nodes, context,
            synchStep: true);
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
