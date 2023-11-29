using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Gve.Text;

public sealed class MoveAnnOperation : AnnOperation
{
    /// <summary>
    /// Gets or sets the target position.
    /// </summary>
    public short To { get; set; }

    /// <summary>
    /// Gets or sets the run of characters to include from <see cref="To"/>.
    /// These will be removed from the source.
    /// </summary>
    public short ToRun { get; set; } = 1;

    public MoveAnnOperation() : base(VarOperationType.Move)
    {
    }

    public override IList<CharNode> Execute(IList<CharNode> nodes)
    {
        ArgumentNullException.ThrowIfNull(nodes);

        List<CharNode> result = CloneNodes(nodes);

        // Ensure At, Run, To, and ToRun are within the valid range
        if (At < 1 || At + Run - 1 > nodes.Count || To < 1 ||
            To + ToRun - 1 > nodes.Count)
        {
            throw new InvalidOperationException("At, Run, To, and ToRun " +
                "must be within the length of the text");
        }

        int indexAt = At - 1;
        int indexTo = To - 1;

        // extract the nodes to be moved
        List<CharNode> movingNodes = result.GetRange(indexAt, Run);
        result.RemoveRange(indexAt, Run);

        // insert the extracted nodes at the new position
        result.InsertRange(indexTo, movingNodes);

        return result;
    }

    /// <summary>
    /// Parses the operation.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <returns>Operation.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="FormatException">Invalid move operation: {text}</exception>
    internal static MoveVarOperation ParseOperation(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        MoveVarOperation op = new();
        // ATxRUN
        string body = ParseHeadAndTail(text, op);
        Match m = AtRunRegex().Match(body);
        if (!m.Success)
            throw new FormatException($"Invalid move operation: {text}");
        op.To = short.Parse(m.Groups["at"].Value);
        if (m.Groups["run"].Success) op.ToRun = short.Parse(m.Groups["run"].Value);

        return op;
    }
}
