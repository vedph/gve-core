using System;
using System.Text;

namespace Gve.Text;

public sealed class ReplaceVarOperation : VarOperation
{
    public ReplaceVarOperation() : base(VarOperationType.Replace)
    {
    }

    public override string Execute(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        int index = At - 1;
        StringBuilder sb = new(text);
        sb.Remove(index, Run);
        if (!string.IsNullOrEmpty(Value))
        {
            sb.Insert(index, Value);
        }
        return sb.ToString();
    }
}
