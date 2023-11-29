using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Gve.Text;

public sealed class SwapAnnOperation : AnnOperation
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

    public SwapAnnOperation() : base(VarOperationType.Swap)
    {
    }

    public override IList<CharNode> Execute(IList<CharNode> nodes)
    {
        ArgumentNullException.ThrowIfNull(nodes);

        List<CharNode> result = CloneNodes(nodes);

        // ensure At, Run, With, and WithRun are within the valid range
        if (At < 1 || At + Run - 1 > nodes.Count || With < 1 ||
            With + WithRun - 1 > nodes.Count)
        {
            throw new InvalidOperationException(
                "At, Run, With, and WithRun must be within the length of the text");
        }

        int li = Math.Min(At, With) - 1;
        int ri = Math.Max(At, With) - 1;

        // extract the nodes to be swapped
        List<CharNode> lv = result.GetRange(li, Run);
        List<CharNode> rv = result.GetRange(ri, WithRun);

        // remove the nodes from their original positions
        result.RemoveRange(ri, WithRun);
        result.RemoveRange(li, Run);

        // insert the nodes at the new positions
        result.InsertRange(ri, lv);
        result.InsertRange(li, rv);

        return result;
    }

    /// <summary>
    /// Parses the operation.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <returns>Operation.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="FormatException">Invalid swap operation: {text}</exception>
    internal static SwapVarOperation ParseOperation(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        SwapVarOperation op = new();
        // ATxRUN
        string body = ParseHeadAndTail(text, op);
        Match m = AtRunRegex().Match(body);
        if (!m.Success)
            throw new FormatException($"Invalid swap operation: {text}");
        op.With = short.Parse(m.Groups["at"].Value);
        if (m.Groups["run"].Success) op.WithRun = short.Parse(m.Groups["run"].Value);

        return op;
    }
}
