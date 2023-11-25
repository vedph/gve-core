using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gve.Text;

public sealed class SwapVarOperation : VarOperation
{
    public SwapVarOperation() : base(VarOperationType.Swap)
    {
    }

    public override AnnotatedText Execute(AnnotatedText text)
    {
        ArgumentNullException.ThrowIfNull(text);

        int index = At - 1;
        StringBuilder sb = new(text);
        string value = sb.ToString(index, Run);
        sb.Remove(index, Run);
        sb.Insert(index, Value);
        return sb.ToString();
    }

    internal static SwapVarOperation ParseOperation(string text)
    {
        throw new NotImplementedException();
    }
}
