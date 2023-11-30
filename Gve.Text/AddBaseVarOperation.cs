using System;
using System.Collections.Generic;
using System.Text;

namespace Gve.Text;

/// <summary>
/// Base class for add left/right base text operations.
/// </summary>
/// <seealso cref="BaseVarOperation" />
public abstract class AddBaseVarOperation : BaseVarOperation
{
    /// <summary>
    /// Gets or sets a value indicating whether this instance is add right
    /// or add left.
    /// </summary>
    protected bool IsRight { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AddLeftBaseVarOperation"/>
    /// class.
    /// </summary>
    /// <param name="type">The operation type.</param>
    /// <param name="set">The set.</param>
    protected AddBaseVarOperation(VarOperationType type, CharNodeSet set)
        : base(type, set)
    {
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
        if (IsRight) Set.InsertAfter(At, nodes);
        else Set.InsertBefore(At, nodes);
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
