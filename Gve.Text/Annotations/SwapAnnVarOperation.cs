using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Gve.Text.Annotations;

/// <summary>
/// Swap annotation operation.
/// Syntax: <c>ATxRUN&lt;&gt;ATxRUN^RANK@GROUP [TAG ...]  {NOTE}</c>.
/// </summary>
/// <seealso cref="AnnVarOperation" />
public sealed class SwapAnnVarOperation : AnnVarOperation
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
    /// Initializes a new instance of the <see cref="SwapAnnVarOperation"/> class.
    /// </summary>
    public SwapAnnVarOperation() : base(VarOperationType.Swap)
    {
    }

    /// <summary>
    /// Executes this operation on the specified context.
    /// </summary>
    /// <param name="context">The data context.</param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="InvalidOperationException">At, Run, With, and WithRun
    /// must be within the length of the text.</exception>
    public override void Execute(VarOperationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        // ensure At, Run, With, and WithRun are within the valid range
        if (At < 1 || At + Run - 1 > context.TreeNodes.Count || With < 1 ||
            With + WithRun - 1 > context.TreeNodes.Count)
        {
            throw new InvalidOperationException(
                "At, Run, With, and WithRun must be within the length of the text");
        }

        AddFeatures(context.Features);

        int li = Math.Min(At, With) - 1;
        int ri = Math.Max(At, With) - 1;

        // extract the nodes to be swapped
        List<CharNode> lv = context.TreeNodes.GetRange(li, Run);
        List<CharNode> rv = context.TreeNodes.GetRange(ri, WithRun);

        // remove the nodes from their original positions
        context.TreeNodes.RemoveRange(ri, WithRun);
        context.TreeNodes.RemoveRange(li, Run);

        // adjust the indexes if necessary
        if (li < ri) ri -= Run;
        else li -= WithRun;

        // insert the nodes at the new positions
        context.TreeNodes.InsertRange(ri, lv);
        context.TreeNodes.InsertRange(li, rv);
    }

    /// <summary>
    /// Parses the operation.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <returns>Operation.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="FormatException">Invalid swap operation: {text}</exception>
    internal static SwapAnnVarOperation ParseOperation(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        SwapAnnVarOperation op = new();
        // ATxRUN
        string body = ParseHeadAndTail(text, op);
        Match m = AtRunRegex().Match(body);
        if (!m.Success)
            throw new FormatException($"Invalid swap operation: {text}");
        op.With = short.Parse(m.Groups["at"].Value);
        if (m.Groups["run"].Success) op.WithRun = short.Parse(m.Groups["run"].Value);

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
        // WITHxWITHRUN
        sb.Append(With);
        if (WithRun > 1) sb.Append('x').Append(WithRun);
        AppendTail(sb);
        return sb.ToString();
    }
}
