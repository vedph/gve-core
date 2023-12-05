using System;
using System.Text;

namespace Gve.Text.Base;

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
    protected AddBaseVarOperation(VarOperationType type) : base(type)
    {
    }

    /// <summary>
    /// Executes this operation on the specified context.
    /// </summary>
    /// <param name="context">The data context.</param>
    /// <exception cref="ArgumentNullException">context</exception>
    public override void Execute(VarOperationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (IsRight) context.Set.InsertAfter(At, Value ?? "", Features);
        else context.Set.InsertBefore(At, Value ?? "", Features);
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
