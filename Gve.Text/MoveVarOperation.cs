using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gve.Text;

public sealed class MoveVarOperation : VarOperation
{
    public MoveVarOperation() : base(VarOperationType.Move)
    {
    }

    public override string Execute(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        // TODO
        int index = At - 1;
        StringBuilder sb = new(text);
        string value = sb.ToString(index, Run);
        sb.Remove(index, Run);
        sb.Insert(index, Value);
        return sb.ToString();
    }
}
