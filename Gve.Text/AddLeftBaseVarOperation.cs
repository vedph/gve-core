using System;
using System.Collections.Generic;

namespace Gve.Text;

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
    /// <param name="set">The set.</param>
    public AddLeftBaseVarOperation(CharNodeSet set)
        : base(VarOperationType.AddLeft, set)
    {
        IsRight = false;
    }

    /// <summary>
    /// Executes this operation on the specified nodes, updating the received
    /// <paramref name="annotations" /> if required.
    /// </summary>
    /// <param name="nodes">The nodes.</param>
    /// <param name="annotations">The annotations.</param>
    /// <returns>
    /// New nodes resulting from the execution.
    /// </returns>
    /// <exception cref="ArgumentNullException">nodes or annotations</exception>
    public override void Execute(IList<CharNode> nodes,
        IList<Feature> annotations)
    {
        ArgumentNullException.ThrowIfNull(nodes);
        ArgumentNullException.ThrowIfNull(annotations);

        AddFeatures(annotations);
        Set.InsertBefore(At, nodes);
    }

    /// <summary>
    /// Parses the operation.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <param name="set">The nodes set.</param>
    /// <returns>Operation.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    internal static AddLeftBaseVarOperation ParseOperation(string text,
        CharNodeSet set)
    {
        ArgumentNullException.ThrowIfNull(text);
        ArgumentNullException.ThrowIfNull(set);

        AddLeftBaseVarOperation op = new(set);
        // "VALUE"
        string body = ParseHeadAndTail(text, op);
        op.Run = 0;
        op.Value = body.Trim('"');

        return op;
    }
}
