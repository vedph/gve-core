using System;
using System.Text;

namespace Gve.Text;

public sealed class AddLeftVarOperation : VarOperation
{
    public AddLeftVarOperation() : base(VarOperationType.AddLeft)
    {
    }

    public override string Execute(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        int index = At - 1;
        StringBuilder sb = new(text);
        if (!string.IsNullOrEmpty(Value))
        {
            sb.Insert(index, Value);
        }
        return sb.ToString();
    }
}
