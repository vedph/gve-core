using System;
using System.Text;
using System.Text.RegularExpressions;

namespace Gve.Text;

/// <summary>
/// Move text to the specified position.
/// </summary>
/// <seealso cref="VarOperation" />
public sealed class MoveVarOperation : VarOperation
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
    /// Initializes a new instance of the <see cref="MoveVarOperation"/> class.
    /// </summary>
    public MoveVarOperation() : base(VarOperationType.Move)
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
        sb.Insert(To - 1, value);

        return CreateResult(sb.ToString(), text.Annotations);
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
