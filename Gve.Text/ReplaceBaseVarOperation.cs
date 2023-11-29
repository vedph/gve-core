using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gve.Text;

public sealed class ReplaceBaseVarOperation : BaseVarOperation
{
    public ReplaceBaseVarOperation() : base(VarOperationType.Replace)
    {
    }

    public override IList<CharNode> Execute(IList<CharNode> nodes,
        IList<Feature> annotations)
    {
        ArgumentNullException.ThrowIfNull(nodes);
        ArgumentNullException.ThrowIfNull(annotations);

        throw new NotImplementedException();
    }
}
