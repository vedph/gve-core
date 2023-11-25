using System;
using System.Text.RegularExpressions;

namespace Gve.Text;

/// <summary>
/// Annotate text operation.
/// </summary>
/// <remarks>Syntax: <c>ATxRUN:NAME="VALUE" [TAG...] {NOTE}</c>.</remarks>
/// <seealso cref="VarOperation" />
public sealed partial class AnnotateVarOperation : VarOperation
{
    /// <summary>
    /// Gets or sets the attribute name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AnnotateVarOperation"/> class.
    /// </summary>
    public AnnotateVarOperation() : base(VarOperationType.Annotate)
    {
        Name = "";
        Value = "";
    }

    /// <summary>
    /// Execute this operation on the specified text.
    /// </summary>
    /// <param name="text">Input annotated text.</param>
    /// <returns>
    /// Annotated text.
    /// </returns>
    /// <exception cref="ArgumentNullException"></exception>
    public override AnnotatedText Execute(AnnotatedText text)
    {
        ArgumentNullException.ThrowIfNull(text);

        if (!string.IsNullOrEmpty(Note))
            text.Annotations[AnnotatedText.NOTE_KEY] = Note;

        text.Annotations[Name] = Value ?? "";
        return text;
    }

    [GeneratedRegex(@"^(?<name>[^=]+)=""(?<value>.*)""$")]
    private static partial Regex NameValueRegex();

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
