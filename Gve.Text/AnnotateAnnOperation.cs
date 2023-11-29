using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Gve.Text;

/// <summary>
/// Annotate annotation operation.
/// Syntax: <c>ATxRUN:NAME="!VALUE" [TAG ...] {NOTE}</c>.
/// </summary>
/// <seealso cref="AnnVarOperation" />
public partial class AnnotateAnnVarOperation : AnnVarOperation
{
    /// <summary>
    /// Gets or sets the attribute name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the annotation to add should
    /// be unique, i.e. if it already exists, it should be replaced.
    /// </summary>
    /// <value>
    ///   <c>true</c> if this annotation is unique; otherwise, <c>false</c>.
    /// </value>
    public bool IsUnique { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AnnotateAnnVarOperation"/> class.
    /// </summary>
    public AnnotateAnnVarOperation() : base(VarOperationType.Annotate)
    {
        Name = "";
        Value = "";
    }

    [GeneratedRegex(@"^(?<name>[^=]+)=""(?<value>.*)""$")]
    private static partial Regex NameValueRegex();

    /// <summary>
    /// Executes this operation. This does not modify the input nodes, but
    /// only adds annotations to the output.
    /// </summary>
    /// <param name="nodes">The nodes.</param>
    /// <param name="annotations">The current annotations.</param>
    /// <returns>The output nodes. These are equal to <paramref name="nodes"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">nodes or annotations</exception>
    public override IList<CharNode> Execute(IList<CharNode> nodes,
        IList<Feature> annotations)
    {
        ArgumentNullException.ThrowIfNull(nodes);
        ArgumentNullException.ThrowIfNull(annotations);

        AddNote(annotations);
        if (IsUnique)
        {
            int i = 0;
            while (i < annotations.Count && annotations[i].Name != Name) i++;
            if (i < annotations.Count) annotations.RemoveAt(i);
        }
        annotations.Add(new Feature(Name, Value ?? ""));

        return nodes;
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
        // NAME="VALUE" or NAME="!VALUE" for a unique annotation
        string body = ParseHeadAndTail(text, op);
        Match m = NameValueRegex().Match(body);
        if (!m.Success)
            throw new FormatException($"Invalid annotate operation: {text}");
        op.Name = m.Groups["name"].Value;
        op.Value = m.Groups["value"].Value;
        op.IsUnique = op.Value.StartsWith('!');
        if (op.IsUnique) op.Value = op.Value[1..];

        return op;
    }
}
