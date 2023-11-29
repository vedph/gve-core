using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gve.Text;

public sealed class SwapBaseVarOperation : BaseVarOperation
{
    public SwapBaseVarOperation() : base(VarOperationType.Swap)
    {
    }

    public override IList<CharNode> Execute(IList<CharNode> nodes,
               IList<Feature> annotations)
    {
        throw new NotImplementedException();
    }
}
