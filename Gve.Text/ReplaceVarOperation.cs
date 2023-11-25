using System;
using System.Text;

namespace Gve.Text;

public sealed class ReplaceVarOperation : VarOperation
{
    public ReplaceVarOperation() : base(VarOperationType.Replace)
    {
    }

    public override AnnotatedText Execute(AnnotatedText text)
    {
        ArgumentNullException.ThrowIfNull(text);

        int index = At - 1;
        StringBuilder sb = new(text.Value);

        sb.Remove(index, Run);
        if (!string.IsNullOrEmpty(Value)) sb.Insert(index, Value);

        return CreateResult(sb.ToString(), text.Annotations);
    }

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
