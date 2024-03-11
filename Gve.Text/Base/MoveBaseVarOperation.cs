using System;
using System.Text;
using System.Text.RegularExpressions;

namespace Gve.Text.Base;

/// <summary>
/// Move base variant operation.
/// </summary>
/// <seealso cref="BaseVarOperation" />
public sealed class MoveBaseVarOperation : BaseVarOperation
{
    /// <summary>
    /// Gets or sets the target position.
    /// </summary>
    public short To { get; set; }


    /// <summary>
    /// Initializes a new instance of the <see cref="MoveBaseVarOperation"/>
    /// class.
    /// </summary>
    public MoveBaseVarOperation() : base(VarOperationType.MoveBefore)
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

        AddFeatures(context.Set.CollectNodes(At, Run));
        context.Set.Move(At, Run, To);
    }

    /// <summary>
    /// Parses the operation.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <returns>Operation.</returns>
    /// <exception cref="ArgumentNullException">text or set</exception>
    /// <exception cref="FormatException">Invalid move operation: {text}</exception>
    internal static MoveBaseVarOperation ParseOperation(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        MoveBaseVarOperation op = new();
        // AT
        string body = ParseHeadAndTail(text, op);
        Match m = AtRunRegex().Match(body);
        if (!m.Success)
            throw new FormatException($"Invalid move operation: {text}");
        op.To = short.Parse(m.Groups["at"].Value);

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
        // TO
        sb.Append(To);
        AppendTail(sb);
        return sb.ToString();
    }
}
