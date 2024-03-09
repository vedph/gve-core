using System;
using System.Text;

namespace Gve.Text.Base;

/// <summary>
/// Replace base text variant operation.
/// </summary>
/// <seealso cref="BaseVarOperation" />
public sealed class ReplaceBaseVarOperation : BaseVarOperation
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ReplaceBaseVarOperation"/>
    /// class.
    /// </summary>
    public ReplaceBaseVarOperation() : base(VarOperationType.Replace)
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
        context.Set.Replace(At, Run,
            context.Set.CreateNodesFor(Value ?? "", Features));
    }

    /// <summary>
    /// Parses the operation.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <returns>Operation.</returns>
    /// <exception cref="ArgumentNullException">text</exception>
    internal static ReplaceBaseVarOperation ParseOperation(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        ReplaceBaseVarOperation op = new();
        // "VALUE"
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
