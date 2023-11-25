using System;
using System.Text.RegularExpressions;

namespace Gve.Text;

public sealed partial class AnnotateVarOperation : VarOperation
{
    public string Name { get; set; }

    public AnnotateVarOperation() : base(VarOperationType.Annotate)
    {
        Name = "";
        Value = "";
    }

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
