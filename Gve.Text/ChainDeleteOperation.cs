using System;

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
}
