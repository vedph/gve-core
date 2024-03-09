using System;
using System.Text;

namespace Gve.Text;

/// <summary>
/// "Delete" operation for a chain.
/// </summary>
/// <typeparam name="T">The type of chain node data.</typeparam>
public sealed class ChainDeleteOperation<T> : ChainOperation<T>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ChainDeleteOperation{T}"/>
    /// class.
    /// </summary>
    public ChainDeleteOperation() : base(VarOperationType.Delete)
    {
    }

    /// <summary>
    /// Executes this operation on the specified context.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <exception cref="ArgumentNullException">context</exception>
    public override void Execute(ChainOperationContext<T> context)
    {
        ArgumentNullException.ThrowIfNull(context);

        (string inputTag, string outputTag) = GetTags(context);

        // get bound nodes
        (ChainNode<T> node, ChainNode<T>? endNode) = GetBoundNodes(
            context.Chain, inputTag);

        // apply to single node or range
        bool applied;
        if (endNode != null)
        {
            applied = context.Chain.DeleteRange(
                inputTag, outputTag, node, endNode);
        }
        else
        {
            applied = context.Chain.Delete(inputTag, outputTag, node);
        }

        // add step if operation applied
        if (applied)
        {
            context.Steps.Add(new ChainOperationContextStep<T>(
                this, inputTag, outputTag));
        }
    }

    /// <summary>
    /// Parses the operation.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <returns>Operation.</returns>
    /// <exception cref="ArgumentNullException">text</exception>
    internal static ChainDeleteOperation<T> ParseOperation(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        ChainDeleteOperation<T> op = new();
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
