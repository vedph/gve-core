using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gve.Text;

/// <summary>
/// Replacement annotation operation.
/// </summary>
/// <seealso cref="AnnOperation" />
public sealed class ReplaceAnnOperation : AnnOperation
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ReplaceAnnOperation"/> class.
    /// </summary>
    public ReplaceAnnOperation() : base(VarOperationType.Replace)
    {
    }

    /// <summary>
    /// Executes this operation for the specified nodes, returning a copy of
    /// them.
    /// </summary>
    /// <param name="nodes">The nodes.</param>
    /// <returns>The output nodes.</returns>
    /// <exception cref="ArgumentNullException">nodes</exception>
    public override IList<CharNode> Execute(IList<CharNode> nodes)
    {
        ArgumentNullException.ThrowIfNull(nodes);

        if (At < 1 || At > nodes.Count + 1)
        {
            throw new InvalidOperationException("At must be between 1 and " +
                "the length of the text plus 1");
        }

        int index = At - 1;
        List<CharNode> text = CloneNodes(nodes);
        if (index < text.Count)
        {
            int count = Math.Min(Run, text.Count - index);
            text.RemoveRange(index, count);
        }

        if (!string.IsNullOrEmpty(Value))
        {
            List<CharNode> newNodes = Value.Select(
                c => new CharNode(c, 0)).ToList();
            text.InsertRange(index, newNodes);
        }

        return text;
    }

    /// <summary>
    /// Parses the operation.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <returns>Operation.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    internal static ReplaceVarOperation ParseOperation(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        ReplaceVarOperation op = new();
        // "VALUE"
        string body = ParseHeadAndTail(text, op);
        op.Value = body.Trim('"');

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
        sb.Append('"').Append(Value).Append('"');
        AppendTail(sb);
        return sb.ToString();
    }
}
