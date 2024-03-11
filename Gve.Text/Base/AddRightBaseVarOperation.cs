using System;

namespace Gve.Text.Base;

/// <summary>
/// Add right base variant operation.
/// </summary>
/// <seealso cref="BaseVarOperation" />
public sealed class AddRightBaseVarOperation : AddBaseVarOperation
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AddRightBaseVarOperation"/>
    /// class.
    /// </summary>
    public AddRightBaseVarOperation() : base(VarOperationType.AddBefore)
    {
        IsRight = true;
    }

    /// <summary>
    /// Executes this operation on the specified context.
    /// </summary>
    /// <param name="context">The data context.</param>
    /// <exception cref="ArgumentNullException">context</exception>
    public override void Execute(VarOperationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        context.Set.InsertAfter(At, Value ?? "", Features);
    }

    /// <summary>
    /// Parses the operation.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <returns>Operation.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    internal static AddRightBaseVarOperation ParseOperation(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        AddRightBaseVarOperation op = new();
        // "VALUE"
        string body = ParseHeadAndTail(text, op);
        op.Run = 0;
        op.Value = body.Trim('"');

        return op;
    }
}
