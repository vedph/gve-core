using System;
using System.Collections.Generic;
using System.Text;

namespace Gve.Text.Annotations;

/// <summary>
/// Base class for add left/right annotation operations.
/// </summary>
/// <seealso cref="AnnVarOperation" />
public abstract class AddAnnVarOperation : AnnVarOperation
{
    /// <summary>
    /// Gets or sets a value indicating whether this instance is add right
    /// or add left.
    /// </summary>
    protected bool IsRight { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AddAnnVarOperation"/> class.
    /// </summary>
    /// <param name="type">The operation type.</param>
    protected AddAnnVarOperation(VarOperationType type) : base(type)
    {
    }

    /// <summary>
    /// Executes this operation on the specified context.
    /// </summary>
    /// <param name="context">The data context.</param>
    /// <exception cref="ArgumentNullException">context</exception>
    public override void Execute(VarOperationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        AddFeatures(context.Set.CollectNodes(At, Run));

        if (string.IsNullOrEmpty(Value)) return;

        int index = IsRight ? At : At - 1;
        if (!string.IsNullOrEmpty(Value))
        {
            IList<CharNode> newNodes = context.Set.CreateNodesFor(Value);
            context.TreeNodes.InsertRange(index, newNodes);
        }
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
