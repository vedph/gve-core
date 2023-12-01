using System;
using System.Text;
using System.Text.RegularExpressions;
using Gve.Text.Annotations;

namespace Gve.Text;

/// <summary>
/// Annotate annotation operation.
/// Syntax: <c>ATxRUN:NAME="!VALUE" [TAG ...] {NOTE}</c>.
/// </summary>
/// <seealso cref="AnnVarOperation" />
public partial class AnnotateAnnVarOperation : AnnVarOperation
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AnnotateAnnVarOperation"/> class.
    /// </summary>
    public AnnotateAnnVarOperation() : base(VarOperationType.Annotate)
    {
    }

    [GeneratedRegex(@"^(?<name>[^=]+)=""(?<value>.*)""$")]
    private static partial Regex NameValueRegex();

    /// <summary>
    /// Executes this operation on the specified context.
    /// </summary>
    /// <param name="context">The data context.</param>
    /// <exception cref="ArgumentNullException">context</exception>
    public override void Execute(VarOperationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        AddFeatures(context.Features);
    }

    /// <summary>
    /// Parses the operation.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <returns>Operation.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="FormatException">Invalid annotate operation: {text}</exception>
    internal static AnnotateAnnVarOperation ParseOperation(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        AnnotateAnnVarOperation op = new();
        ParseHeadAndTail(text, op);

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
        AppendTail(sb);
        return sb.ToString();
    }
}
