using System;

namespace Gve.Text.Base;

/// <summary>
/// Add left base variant operation.
/// </summary>
/// <seealso cref="BaseVarOperation" />
public sealed class AddLeftBaseVarOperation : AddBaseVarOperation
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AddLeftBaseVarOperation"/>
    /// class.
    /// </summary>
    public AddLeftBaseVarOperation() : base(VarOperationType.AddLeft)
    {
        IsRight = false;
    }

    /// <summary>
    /// Executes this operation on the specified context.
    /// </summary>
    /// <param name="context">The data context.</param>
    /// <exception cref="ArgumentNullException">context</exception>
    public override void Execute(VarOperationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        context.Set.InsertBefore(At, Value ?? "", Features);
    }

    /// <summary>
    /// Parses the operation.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <returns>Operation.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    internal static AddLeftBaseVarOperation ParseOperation(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        AddLeftBaseVarOperation op = new();
        // "VALUE"
        string body = ParseHeadAndTail(text, op);
        op.Run = 0;
        op.Value = body.Trim('"');

        return op;
    }
}
