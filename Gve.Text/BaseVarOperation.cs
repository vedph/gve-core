using System;
using System.Collections.Generic;

namespace Gve.Text;

/// <summary>
/// Base for base text variant operations.
/// </summary>
/// <seealso cref="VarOperation" />
public abstract class BaseVarOperation : VarOperation
{
    /// <summary>
    /// Gets the set of nodes representing the base text.
    /// </summary>
    protected CharNodeSet Set { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseVarOperation"/> class.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <param name="set">The nodes set to use, representing the base text.</param>
    /// <exception cref="ArgumentNullException">set</exception>
    protected BaseVarOperation(VarOperationType type, CharNodeSet set) : base(type)
    {
        Set = set ?? throw new System.ArgumentNullException(nameof(set));
    }

    /// <summary>
    /// Executes this operation on the specified nodes, updating the received
    /// <paramref name="annotations"/> if required.
    /// </summary>
    /// <param name="nodes">The nodes.</param>
    /// <param name="annotations">The annotations.</param>
    /// <returns>New nodes resulting from the execution.</returns>
    public abstract IList<CharNode> Execute(IList<CharNode> nodes,
        IList<Feature> annotations);
}
