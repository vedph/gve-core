using System;
using System.Collections.Generic;

namespace Gve.Text;

/// <summary>
/// Annotation base variant operation.
/// </summary>
/// <seealso cref="BaseVarOperation" />
public sealed class AnnotateBaseVarOperation : BaseVarOperation
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AnnotateBaseVarOperation"/>
    /// class.
    /// </summary>
    /// <param name="set">The set.</param>
    public AnnotateBaseVarOperation(CharNodeSet set)
        : base(VarOperationType.Annotate, set)
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
    }
}
