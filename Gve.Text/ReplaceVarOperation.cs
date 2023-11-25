using System;
using System.Text;

namespace Gve.Text;

public sealed class ReplaceVarOperation : VarOperation
{
    public ReplaceVarOperation() : base(VarOperationType.Replace)
    {
    }

    public override AnnotatedText Execute(AnnotatedText text)
    {
        ArgumentNullException.ThrowIfNull(text);

        int index = At - 1;
        StringBuilder sb = new(text.Value);
        sb.Remove(index, Run);
        if (!string.IsNullOrEmpty(Value))
        {
            sb.Insert(index, Value);
        }
        return CreateResult(sb.ToString(), text.Annotations);
    }
}
