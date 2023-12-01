using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gve.Text.Annotations;

/// <summary>
/// Replacement annotation operation.
/// </summary>
/// <seealso cref="AnnVarOperation" />
public sealed class ReplaceAnnVarOperation : AnnVarOperation
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ReplaceAnnVarOperation"/> class.
    /// </summary>
    public ReplaceAnnVarOperation() : base(VarOperationType.Replace)
    {
    }

    /// <summary>
    /// Executes this operation on the specified context.
    /// </summary>
    /// <param name="context">The data context.</param>
    /// <exception cref="ArgumentNullException">context</exception>
    /// <exception cref="InvalidOperationException">At must be between 1 and
    /// the length of the text plus 1</exception>
    public override void Execute(VarOperationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (At < 1 || At > context.TreeNodes.Count + 1)
        {
            throw new InvalidOperationException("At must be between 1 and " +
                "the length of the text plus 1");
        }

        AddFeatures(context.Features);

        int index = At - 1;
        if (index < context.TreeNodes.Count)
        {
            int count = Math.Min(Run, context.TreeNodes.Count - index);
            context.TreeNodes.RemoveRange(index, count);
        }

        if (!string.IsNullOrEmpty(Value))
        {
            List<CharNode> newNodes = Value.Select(
                c => new CharNode(c, 0)).ToList();
            context.TreeNodes.InsertRange(index, newNodes);
        }
    }

    /// <summary>
    /// Parses the operation.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <returns>Operation.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    internal static ReplaceAnnVarOperation ParseOperation(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        ReplaceAnnVarOperation op = new();
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
