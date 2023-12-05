using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Gve.Text.Annotations;

/// <summary>
/// Move annotation operation.
/// Syntax: <c>ATxRUN>ATxRUN^RANK@GROUP [TAG ...]  {NOTE}</c>.
/// </summary>
/// <seealso cref="AnnVarOperation" />
public sealed class MoveAnnVarOperation : AnnVarOperation
{
    /// <summary>
    /// Gets or sets the target position.
    /// </summary>
    public short To { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MoveAnnVarOperation"/> class.
    /// </summary>
    public MoveAnnVarOperation() : base(VarOperationType.Move)
    {
    }

    /// <summary>
    /// Executes this operation on the specified context.
    /// </summary>
    /// <param name="context">The data context.</param>
    /// <exception cref="ArgumentNullException">context</exception>
    /// <exception cref="InvalidOperationException">At, Run, To, and ToRun
    /// must be within the length of the text.</exception>
    public override void Execute(VarOperationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        // ensure At, Run, To, and ToRun are within the valid range
        if (At < 1 || At + Run - 1 > context.TreeNodes.Count || To < 1 ||
            To > context.TreeNodes.Count)
        {
            throw new InvalidOperationException("At, Run, To, and ToRun " +
                "must be within the length of the text");
        }

        int indexAt = At - 1;
        int indexTo = To - 1;

        AddFeatures(context.Set.CollectNodes(At, Run));

        // extract the nodes to be moved
        List<CharNode> movingNodes = context.TreeNodes.GetRange(indexAt, Run);
        context.TreeNodes.RemoveRange(indexAt, Run);
        if (indexAt < indexTo) indexTo -= Run;

        // insert the extracted nodes at the new position
        context.TreeNodes.InsertRange(indexTo, movingNodes);
    }

    /// <summary>
    /// Parses the operation.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <returns>Operation.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="FormatException">Invalid move operation: {text}</exception>
    internal static MoveAnnVarOperation ParseOperation(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        MoveAnnVarOperation op = new();
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
