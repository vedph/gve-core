using System;
using System.Text;
using System.Text.RegularExpressions;

namespace Gve.Text;

public sealed class MoveVarOperation : VarOperation
{
    public short To { get; set; }
    public short ToRun { get; set; } = 1;

    public MoveVarOperation() : base(VarOperationType.Move)
    {
    }

    public override AnnotatedText Execute(AnnotatedText text)
    {
        ArgumentNullException.ThrowIfNull(text);

        StringBuilder sb = new(text.Value);
        string value = sb.ToString(At - 1, Run);
        sb.Remove(At - 1, Run);
        sb.Insert(To - 1, value);

        return CreateResult(sb.ToString(), text.Annotations);
    }

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
