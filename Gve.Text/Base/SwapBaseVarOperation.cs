using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Gve.Text.Base;

/// <summary>
/// Swap base variant operation.
/// </summary>
/// <seealso cref="BaseVarOperation" />
public sealed class SwapBaseVarOperation : BaseVarOperation
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
    /// Initializes a new instance of the <see cref="SwapBaseVarOperation"/>
    /// class.
    /// </summary>
    public SwapBaseVarOperation() : base(VarOperationType.Swap)
    {
    }

    /// <summary>
    /// Executes this operation on the specified nodes, updating the received
    /// <paramref name="annotations" /> if required.
    /// </summary>
    /// <param name="set">The set.</param>
    /// <param name="annotations">The annotations.</param>
    /// <exception cref="ArgumentNullException">nodes or annotations</exception>
    public override void Execute(CharNodeSet set, IList<Feature> annotations)
    {
        ArgumentNullException.ThrowIfNull(set);
        ArgumentNullException.ThrowIfNull(annotations);

        AddFeatures(annotations);
        set.Swap(At, Run, With, WithRun);
    }

    /// <summary>
    /// Parses the operation.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <returns>Operation.</returns>
    /// <exception cref="ArgumentNullException">text or set</exception>
    /// <exception cref="FormatException">Invalid swap operation: {text}</exception>
    internal static SwapBaseVarOperation ParseOperation(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        SwapBaseVarOperation op = new();
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
