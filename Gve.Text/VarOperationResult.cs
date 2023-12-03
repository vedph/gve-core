using System;

namespace Gve.Text;

/// <summary>
/// Result of a <see cref="VarOperation"/> operation.
/// </summary>
public class VarOperationResult
{
    /// <summary>
    /// Gets the operation.
    /// </summary>
    public VarOperation Operation { get; }

    /// <summary>
    /// Gets the nodes.
    /// </summary>
    public VarOperationContext Context { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="VarOperationResult"/> class.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <param name="context">The current operation context.</param>
    /// <exception cref="ArgumentNullException">operation or context</exception>
    public VarOperationResult(VarOperation operation, VarOperationContext context)
    {
        Operation = operation ?? throw new ArgumentNullException(nameof(operation));
        Context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// Converts to string.
    /// </summary>
    /// <returns>
    /// A <see cref="string" /> that represents this instance.
    /// </returns>
    public override string ToString()
    {
        return $"{Operation} {Context}";
    }
}
