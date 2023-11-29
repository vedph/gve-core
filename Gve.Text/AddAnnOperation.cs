using System;
using System.Collections.Generic;
using System.Linq;

namespace Gve.Text;

/// <summary>
/// Base class for add left/right annotation operations.
/// </summary>
/// <seealso cref="AnnOperation" />
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

    /// <summary>
    /// Executes this operation for the specified nodes, returning a copy of
    /// them.
    /// </summary>
    /// <param name="nodes">The nodes.</param>
    /// <param name="annotations">The current annotations.</param>
    /// <returns>The output nodes.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public override IList<CharNode> Execute(IList<CharNode> nodes,
        IList<Feature> annotations)
    {
        ArgumentNullException.ThrowIfNull(nodes);

        AddNote(annotations);
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
