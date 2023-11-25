using System;
using System.Text;

namespace Gve.Text;

public sealed class AddLeftVarOperation : VarOperation
{
    public AddLeftVarOperation() : base(VarOperationType.AddLeft)
    {
    }

    public override AnnotatedText Execute(AnnotatedText text)
    {
        ArgumentNullException.ThrowIfNull(text);
        if (string.IsNullOrEmpty(Value)) return text;

        StringBuilder sb = new(text.Value);
        sb.Insert(At - 1, Value);

        return CreateResult(sb.ToString(), text.Annotations);
    }

    internal static AddLeftVarOperation ParseOperation(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        AddLeftVarOperation op = new();
        // "value"
        string body = ParseHeadAndTail(text, op);
        op.Value = body.Trim('"');

        return op;
    }
}
