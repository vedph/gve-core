using System;
using System.Collections.Generic;
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

        // replace value in existing nodes
        int index = At - 1;
        string value = Value ?? "";

        for (int i = 0; i < Run && i < value.Length; i++)
        {
            CharNode node = context.TreeNodes[index + i];
            node.Value = value[i];
            AddFeatures(node);
        }

        // delete extra nodes if needed
        if (Run > value.Length)
        {
            context.TreeNodes.RemoveRange(index + value.Length,
                Run - value.Length);
        }
        // add new nodes if needed
        else if (Run < value.Length)
        {
            IList<CharNode> newNodes = context.Set.CreateNodesFor(
                value[Run..], Features);
            context.TreeNodes.InsertRange(index + Run, newNodes);
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
