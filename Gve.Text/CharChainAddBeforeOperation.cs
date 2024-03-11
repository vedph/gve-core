using System;
using System.Text;

namespace Gve.Text;

/// <summary>
/// An add-before operation for a chain of characters.
/// </summary>
/// <seealso cref="CharChainOperation" />
public sealed class CharChainAddBeforeOperation : CharChainOperation
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CharChainAddBeforeOperation"/>
    /// class.
    /// </summary>
    public CharChainAddBeforeOperation() : base(VarOperationType.AddBefore)
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

        // get bound node
        (ChainNode<char> node, ChainNode<char>? endNode) = GetBoundNodes(
            context.Chain, inputTag);

        if (endNode != null)
        {
            throw new InvalidOperationException(
                "Add-before operation cannot have a RUN different from 0.");
        }

        // apply to single node or range
        if (Value.Length == 1)
        {
            context.Chain.InsertBefore(inputTag, outputTag, node,
                GetNodesFrom(Value)[0]);
        }
        else
        {
            context.Chain.InsertRangeBefore(inputTag, outputTag, node,
                GetNodesFrom(Value));
        }
    }

    /// <summary>
    /// Parses the specified text representing a replace operation.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <returns>Operation.</returns>
    /// <exception cref="ArgumentNullException">text</exception>
    internal static CharChainAddBeforeOperation ParseOperation(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        CharChainAddBeforeOperation op = new();

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
