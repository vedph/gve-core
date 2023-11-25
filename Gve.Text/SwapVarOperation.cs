using System;
using System.Text;
using System.Text.RegularExpressions;

namespace Gve.Text;

public sealed class SwapVarOperation : VarOperation
{
    public short With { get; set; }
    public short WithRun { get; set; } = 1;

    public SwapVarOperation() : base(VarOperationType.Swap)
    {
    }

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
