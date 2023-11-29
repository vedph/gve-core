using System.Collections.Generic;

namespace Gve.Text;

public abstract class BaseVarOperation : VarOperation
{
    protected BaseVarOperation(VarOperationType type) : base(type)
    {
    }

    public abstract IList<CharNode> Execute(IList<CharNode> nodes,
        IList<Feature> annotations);
}
