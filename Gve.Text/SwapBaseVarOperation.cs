using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gve.Text;

public sealed class SwapBaseVarOperation : BaseVarOperation
{
    /// <summary>
    /// Gets or sets the position to swap text with.
    /// </summary>
    public short With { get; set; }

    /// <summary>
    /// Gets or sets the number of characters to remove from the target position
    /// before swapping.
    /// </summary>
    public short WithRun { get; set; } = 1;

    /// <summary>
    /// Initializes a new instance of the <see cref="SwapBaseVarOperation"/>
    /// class.
    /// </summary>
    /// <param name="set">The set.</param>
    public SwapBaseVarOperation(CharNodeSet set)
        : base(VarOperationType.Swap, set)
    {
    }

    /// <summary>
    /// Executes this operation on the specified nodes, updating the received
    /// <paramref name="annotations" /> if required.
    /// </summary>
    /// <param name="nodes">The nodes.</param>
    /// <param name="annotations">The annotations.</param>
    /// <exception cref="ArgumentNullException">nodes or annotations</exception>
    public override void Execute(IList<CharNode> nodes,
        IList<Feature> annotations)
    {
        ArgumentNullException.ThrowIfNull(nodes);
        ArgumentNullException.ThrowIfNull(annotations);

        AddFeatures(annotations);
        Set.Swap(At, Run, With, WithRun);
    }
}
