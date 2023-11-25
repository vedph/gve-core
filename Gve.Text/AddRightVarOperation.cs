using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gve.Text;

public sealed class AddRightVarOperation : VarOperation
{
    public AddRightVarOperation() : base(VarOperationType.AddRight)
    {
    }

    public override string Execute(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        int index = At - 1;
        StringBuilder sb = new(text);
        if (!string.IsNullOrEmpty(Value))
        {
            sb.Insert(index + Run, Value);
        }
        return sb.ToString();
    }
}
