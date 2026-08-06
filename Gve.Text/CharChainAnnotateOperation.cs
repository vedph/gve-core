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
        IList<ChainNode<char>> nodes = GetAnnotatedNodes(context, inputTag,
            outputTag);
        AddNodeTraceFeatures(inputTag, outputTag, false, nodes, context);

        UpdateContext(context, inputTag, outputTag, nodes, nodes);

        AddNodeTraceFeatures(inputTag, outputTag, true, nodes, context);
    }

    /// <summary>
    /// Gets the nodes targeted by this annotation. If the target range is
    /// no longer present in <paramref name="inputTag"/> (e.g. a later
    /// operation replaced or deleted it), this backtracks through ancestor
    /// versions -- the input of <paramref name="inputTag"/>, and so on, up
    /// to the base version -- until a version containing the target range is
    /// found, and merges its structure into <paramref name="outputTag"/> so
    /// the nodes become reachable there too. This lets an annotation still
    /// target text that is no longer part of the current version, which is
    /// useful e.g. to attach visualization-only features (such as an
    /// underline) to text that was later rewritten. This is limited to the
    /// annotate operation because it is the only one which does not alter
    /// the text, so merging an ancestor's structure into the output version
    /// cannot corrupt it: <see cref="Chain{T}.AddVersion"/> only adds a link
    /// when no link with the same tag, source and target already exists, so
    /// the links copied from <paramref name="inputTag"/> (added first, right
    /// before this method runs) keep taking precedence when the output
    /// version is traversed.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="inputTag">The input tag.</param>
    /// <param name="outputTag">The output tag.</param>
    /// <returns>Target node(s).</returns>
    /// <exception cref="InvalidOperationException">Nodes not found even
    /// after backtracking to the base version.</exception>
    private IList<ChainNode<char>> GetAnnotatedNodes(
        ChainOperationContext<char> context, string inputTag, string outputTag)
    {
        try
        {
            return GetReferenceNodes(context.Chain, outputTag);
        }
        catch (InvalidOperationException)
        {
            string? ancestorTag = inputTag;
            while ((ancestorTag = GetAncestorTag(context, ancestorTag)) != null)
            {
                context.Chain.AddVersion(ancestorTag, outputTag);
                try
                {
                    return GetReferenceNodes(context.Chain, outputTag);
                }
                catch (InvalidOperationException)
                {
                    // keep backtracking
                }
            }
            throw;
        }
    }

    /// <summary>
    /// Gets the input tag of the step which produced <paramref name="tag"/>
    /// as its output, i.e. the version <paramref name="tag"/> was edited
    /// from.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="tag">The tag.</param>
    /// <returns>The ancestor tag, or null if <paramref name="tag"/> is the
    /// base version or was not produced by any tracked step.</returns>
    private static string? GetAncestorTag(ChainOperationContext<char> context,
        string tag)
    {
        for (int i = context.Steps.Count - 1; i >= 0; i--)
        {
            if (context.Steps[i].OutputTag == tag)
                return context.Steps[i].InputTag;
        }
        return null;
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
