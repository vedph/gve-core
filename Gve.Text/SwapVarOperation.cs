using System;
using System.Text;
using System.Text.RegularExpressions;

namespace Gve.Text;

/// <summary>
/// Swap text operation.
/// </summary>
/// <seealso cref="VarOperation" />
public sealed class SwapVarOperation : VarOperation
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
    /// Initializes a new instance of the <see cref="SwapVarOperation"/> class.
    /// </summary>
    public SwapVarOperation() : base(VarOperationType.Swap)
    {
    }

    /// <summary>
    /// Execute this operation on the specified text.
    /// </summary>
    /// <param name="text">Input annotated text.</param>
    /// <returns>
    /// Annotated text.
    /// </returns>
    /// <exception cref="ArgumentNullException"></exception>
    public override AnnotatedText Execute(AnnotatedText text)
    {
        ArgumentNullException.ThrowIfNull(text);

        StringBuilder sb = new(text.Value);
        string value = sb.ToString(At - 1, Run);
        sb.Remove(At - 1, Run);
        string with = sb.ToString(With - 1, WithRun);
        sb.Remove(With - 1, WithRun);
        sb.Insert(At - 1, with);
        sb.Insert(With - 1, value);

        return CreateResult(sb.ToString(), text.Annotations);
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
