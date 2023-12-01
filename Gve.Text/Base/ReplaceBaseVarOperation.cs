using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gve.Text.Base;

/// <summary>
/// Replace base text variant operation.
/// </summary>
/// <seealso cref="BaseVarOperation" />
public sealed class ReplaceBaseVarOperation : BaseVarOperation
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ReplaceBaseVarOperation"/>
    /// class.
    /// </summary>
    public ReplaceBaseVarOperation() : base(VarOperationType.Replace)
    {
    }

    /// <summary>
    /// Executes this operation on the specified nodes, updating the received
    /// <paramref name="annotations"/> if required.
    /// </summary>
    /// <param name="set">The set.</param>
    /// <param name="annotations">The annotations.</param>
    /// <returns>New nodes resulting from the execution.</returns>
    /// <exception cref="ArgumentNullException">set or annotations</exception>
    public override void Execute(CharNodeSet set,
        IList<Feature> annotations)
    {
        ArgumentNullException.ThrowIfNull(set);
        ArgumentNullException.ThrowIfNull(annotations);

        set.Replace(At, Run, new CharNodeSet(Value!).ToList());
        AddFeatures(annotations);
    }

    /// <summary>
    /// Parses the operation.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <returns>Operation.</returns>
    /// <exception cref="ArgumentNullException">text or set</exception>
    internal static ReplaceBaseVarOperation ParseOperation(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        ReplaceBaseVarOperation op = new();
        // "VALUE"
        string body = ParseHeadAndTail(text, op);
        op.Value = body.Trim('"');

        return op;
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
