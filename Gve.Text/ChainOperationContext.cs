using System.Collections.Generic;

namespace Gve.Text;

/// <summary>
/// The data context for a chain operation.
/// </summary>
/// <typeparam name="T">The type of chain node data.</typeparam>
public class ChainOperationContext<T>
{
    /// <summary>
    /// Gets the chain. This is the target of all the operations for this
    /// context.
    /// </summary>
    public Chain<T> Chain { get; }

    /// <summary>
    /// Gets or sets the version tag of the last operation executed on this
    /// context.
    /// </summary>
    public string LastTag { get; set; }

    /// <summary>
    /// Gets the steps executed on the chain, each consisting of an operation
    /// and input and output version tags.
    /// </summary>
    public List<ChainOperationContextStep<T>> Steps { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ChainOperationContext{T}"/>
    /// class.
    /// </summary>
    public ChainOperationContext()
    {
        Chain = new Chain<T>();
        LastTag = "";
        Steps = [];
    }
}
