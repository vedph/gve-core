using System;
using System.Collections.Generic;
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
    public CharChainDeleteOperation() : base(VarOperationType.Delete)
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

        // get target nodes
        IList<ChainNode<char>> nodes = GetTargetNodes(context.Chain, inputTag);

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
        UpdateContext(context, inputTag, outputTag,
            GetTargetNodes(context.Chain, inputTag));
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
