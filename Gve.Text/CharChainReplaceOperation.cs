using System;
using System.Collections.Generic;
using System.Text;

namespace Gve.Text;

/// <summary>
/// A replace operation for a chain of characters.
/// </summary>
/// <seealso cref="CharChainOperation" />
public sealed class CharChainReplaceOperation : CharChainOperation
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CharChainReplaceOperation"/>
    /// class.
    /// </summary>
    public CharChainReplaceOperation() : base(OperationType.Replace)
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
        IList<ChainNode<char>> nodes = GetReferenceNodes(context.Chain, inputTag);

        // apply to single node or range
        if (nodes.Count > 1)
        {
            context.Chain.ReplaceRange(inputTag, outputTag, nodes[0], nodes[^1],
                GetNodesFrom(Value ?? ""));
        }
        else
        {
            if (string.IsNullOrEmpty(Value))
            {
                context.Chain.Delete(inputTag, outputTag, nodes[0]);
            }
            else
            {
                if (Value.Length == 1)
                {
                    context.Chain.Replace(inputTag, outputTag, nodes[0],
                        GetNodesFrom(Value)[0]);
                }
                else
                {
                    context.Chain.ReplaceRange(inputTag, outputTag,
                        nodes[0], nodes[0], GetNodesFrom(Value));
                }
            }
        }

        // update context metadata
        UpdateContext(context, inputTag, outputTag, nodes,
            context.Chain.GetTagNodes(outputTag, true));
    }

    /// <summary>
    /// Parses the specified text representing a replace operation.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <returns>Operation.</returns>
    /// <exception cref="ArgumentNullException">text</exception>
    internal static CharChainReplaceOperation ParseOperation(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        CharChainReplaceOperation op = new();

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
