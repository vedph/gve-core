using System;
using System.Text;

namespace Gve.Text;

public sealed class AddRightVarOperation : VarOperation
{
    public AddRightVarOperation() : base(VarOperationType.AddRight)
    {
    }

    public override AnnotatedText Execute(AnnotatedText text)
    {
        ArgumentNullException.ThrowIfNull(text);
        if (string.IsNullOrEmpty(Value)) return text;

        StringBuilder sb = new(text.Value);
        if (Run > 0) sb.Remove(At, Run);
        sb.Insert(At - 1, Value);

        return CreateResult(sb.ToString(), text.Annotations);
    }

    internal static AddRightVarOperation ParseOperation(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        AddRightVarOperation op = new();
        // "value"
        string body = ParseHeadAndTail(text, op);
        op.Value = body.Trim('"');

        return op;
    }
}
