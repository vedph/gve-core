using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Gve.Text.Base;

/// <summary>
/// Move base variant operation.
/// </summary>
/// <seealso cref="BaseVarOperation" />
public sealed class MoveBaseVarOperation : BaseVarOperation
{
    /// <summary>
    /// Gets or sets the target position.
    /// </summary>
    public short To { get; set; }


    /// <summary>
    /// Initializes a new instance of the <see cref="MoveBaseVarOperation"/>
    /// class.
    /// </summary>
    /// <param name="set">The set.</param>
    public MoveBaseVarOperation(CharNodeSet set) :
        base(VarOperationType.Move, set)
    {
    }

    /// <summary>
    /// Executes this operation on the specified nodes, updating the received
    /// <paramref name="annotations" /> if required.
    /// </summary>
    /// <param name="nodes">The nodes.</param>
    /// <param name="annotations">The annotations.</param>
    /// <exception cref="ArgumentNullException">nodes or annotations</exception>
    public override void Execute(IList<CharNode> nodes,
        IList<Feature> annotations)
    {
        ArgumentNullException.ThrowIfNull(nodes);
        ArgumentNullException.ThrowIfNull(annotations);

        AddFeatures(annotations);
        Set.Move(At, Run, To);
    }

    /// <summary>
    /// Parses the operation.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <param name="set">The nodes set.</param>
    /// <returns>Operation.</returns>
    /// <exception cref="ArgumentNullException">text or set</exception>
    /// <exception cref="FormatException">Invalid move operation: {text}</exception>
    internal static MoveBaseVarOperation ParseOperation(string text,
        CharNodeSet set)
    {
        ArgumentNullException.ThrowIfNull(text);
        ArgumentNullException.ThrowIfNull(set);

        MoveBaseVarOperation op = new(set);
        // AT
        string body = ParseHeadAndTail(text, op);
        Match m = AtRunRegex().Match(body);
        if (!m.Success)
            throw new FormatException($"Invalid move operation: {text}");
        op.To = short.Parse(m.Groups["at"].Value);

        return op;
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
        // TO
        sb.Append(To);
        AppendTail(sb);
        return sb.ToString();
    }
}
