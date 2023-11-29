using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Gve.Text;

/// <summary>
/// Move annotation operation.
/// Syntax: <c>ATxRUN>ATxRUN^RANK@GROUP [TAG ...]  {NOTE}</c>.
/// </summary>
/// <seealso cref="AnnOperation" />
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

    /// <summary>
    /// Initializes a new instance of the <see cref="MoveAnnOperation"/> class.
    /// </summary>
    public MoveAnnOperation() : base(VarOperationType.Move)
    {
    }

    /// <summary>
    /// Executes this operation for the specified nodes, returning a copy of
    /// them.
    /// </summary>
    /// <param name="nodes">The nodes.</param>
    /// <param name="annotations">The current annotations.</param>
    /// <returns>The output nodes.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="InvalidOperationException">At, Run, To, and ToRun
    /// must be within the length of the text</exception>
    public override IList<CharNode> Execute(IList<CharNode> nodes,
        IList<Feature> annotations)
    {
        ArgumentNullException.ThrowIfNull(nodes);

        AddNote(annotations);
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
    internal static MoveAnnOperation ParseOperation(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        MoveAnnOperation op = new();
        // ATxRUN
        string body = ParseHeadAndTail(text, op);
        Match m = AtRunRegex().Match(body);
        if (!m.Success)
            throw new FormatException($"Invalid move operation: {text}");
        op.To = short.Parse(m.Groups["at"].Value);
        if (m.Groups["run"].Success) op.ToRun = short.Parse(m.Groups["run"].Value);

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
        // TOxTORUN
        sb.Append(To);
        if (ToRun > 1) sb.Append('x').Append(ToRun);
        AppendTail(sb);
        return sb.ToString();
    }
}
