using System;
using System.Collections.Generic;

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
    public AddRightBaseVarOperation() : base(VarOperationType.AddLeft)
    {
        IsRight = true;
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
    /// <exception cref="ArgumentNullException">nodes or annotations</exception>
    public override void Execute(CharNodeSet set, IList<Feature> annotations)
    {
        ArgumentNullException.ThrowIfNull(set);
        ArgumentNullException.ThrowIfNull(annotations);

        AddFeatures(annotations);
        set.InsertAfter(At, Value ?? "");
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
