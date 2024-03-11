using System;
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
    public CharChainReplaceOperation() : base(VarOperationType.Replace)
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

        // get bound nodes
        (ChainNode<char> node, ChainNode<char>? endNode) = GetBoundNodes(
            context.Chain, inputTag);

        // apply to single node or range
        if (endNode != null)
        {
            context.Chain.ReplaceRange(inputTag, outputTag, node, endNode,
                GetNodesFrom(Value ?? ""));
        }
        else
        {
            if (string.IsNullOrEmpty(Value))
            {
                context.Chain.Delete(inputTag, outputTag, node);
            }
            else
            {
                if (Value.Length == 1)
                {
                    context.Chain.Replace(inputTag, outputTag, node,
                        GetNodesFrom(Value)[0]);
                }
                else
                {
                    context.Chain.ReplaceRange(inputTag, outputTag, node, node,
                        GetNodesFrom(Value));
                }
            }
        }

        // update context metadata
        UpdateContext(context, inputTag, outputTag);
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
