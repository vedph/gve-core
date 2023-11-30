using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gve.Text.Annotations;

/// <summary>
/// Base class for add left/right annotation operations.
/// </summary>
/// <seealso cref="AnnVarOperation" />
public abstract class AddAnnVarOperation : AnnVarOperation
{
    /// <summary>
    /// Gets or sets a value indicating whether this instance is add right
    /// or add left.
    /// </summary>
    protected bool IsRight { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AddAnnVarOperation"/> class.
    /// </summary>
    /// <param name="type">The operation type.</param>
    protected AddAnnVarOperation(VarOperationType type) : base(type)
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

        AddFeatures(annotations);
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

    /// <summary>
    /// Converts to string.
    /// </summary>
    /// <returns>
    /// A <see cref="string" /> that represents this instance.
    /// </returns>
    public override string ToString()
    {
        StringBuilder sb = new();
        AppendHead(sb);
        sb.Append('"').Append(Value).Append('"');
        AppendTail(sb);
        return sb.ToString();
    }
}
