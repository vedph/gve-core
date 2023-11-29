using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gve.Text;

public abstract class AddAnnOperation : AnnOperation
{
    /// <summary>
    /// Gets or sets a value indicating whether this instance is add right
    /// or add left.
    /// </summary>
    protected bool IsRight { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AddAnnOperation"/> class.
    /// </summary>
    /// <param name="type">The operation type.</param>
    protected AddAnnOperation(VarOperationType type) : base(type)
    {
    }

    public override IList<CharNode> Execute(IList<CharNode> nodes)
    {
        ArgumentNullException.ThrowIfNull(nodes);
        if (string.IsNullOrEmpty(Value)) return nodes;

        int index = IsRight ? At : At - 1;
        List<CharNode> result = CloneNodes(nodes);

        if (!string.IsNullOrEmpty(Value))
        {
            List<CharNode> newNodes = Value.Select(c => new CharNode(c, 0)).ToList();
            result.InsertRange(index, newNodes);
        }

        return result;
    }
}
