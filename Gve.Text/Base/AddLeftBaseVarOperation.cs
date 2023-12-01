using System;
using System.Collections.Generic;

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
    /// Executes this operation on the specified nodes, updating the received
    /// <paramref name="annotations" /> if required.
    /// </summary>
    /// <param name="set">The set.</param>
    /// <param name="annotations">The annotations.</param>
    /// <returns>
    /// New nodes resulting from the execution.
    /// </returns>
    /// <exception cref="ArgumentNullException">set or annotations</exception>
    public override void Execute(CharNodeSet set, IList<Feature> annotations)
    {
        ArgumentNullException.ThrowIfNull(set);
        ArgumentNullException.ThrowIfNull(annotations);

        AddFeatures(annotations);
        set.InsertBefore(At, Value ?? "");
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
