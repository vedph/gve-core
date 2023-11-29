using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Gve.Text;

/// <summary>
/// Annotate annotation operation.
/// </summary>
/// <seealso cref="AnnOperation" />
public partial class AnnotateAnnOperation : AnnOperation
{
    /// <summary>
    /// Gets or sets the attribute name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AnnotateAnnOperation"/> class.
    /// </summary>
    public AnnotateAnnOperation() : base(VarOperationType.Annotate)
    {
        Name = "";
        Value = "";
    }

    [GeneratedRegex(@"^(?<name>[^=]+)=""(?<value>.*)""$")]
    private static partial Regex NameValueRegex();

    public override IList<CharNode> Execute(IList<CharNode> nodes)
    {
        // TODO
    }

    /// <summary>
    /// Parses the operation.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <returns>Operation.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="FormatException">Invalid annotate operation: {text}</exception>
    internal static AnnotateVarOperation ParseOperation(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        AnnotateVarOperation op = new();
        // NAME="VALUE"
        string body = ParseHeadAndTail(text, op);
        Match m = NameValueRegex().Match(body);
        if (!m.Success)
            throw new FormatException($"Invalid annotate operation: {text}");
        op.Name = m.Groups["name"].Value;
        op.Value = m.Groups["value"].Value;

        return op;
    }
}
