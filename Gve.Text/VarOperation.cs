using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Gve.Text;

/// <summary>
/// A variant text editing operation.
/// </summary>
/// <seealso cref="IHasVersionTags" />
public abstract partial class VarOperation : IHasVersionTags
{
    /// <summary>
    /// The symbols corresponding to <see cref="VarOperationType"/>'s.
    /// </summary>
    public static readonly string[] TYPE_SYMBOLS =
    [
        "=",
        "+<",
        "+>",
        ":",
        ">",
        "<>"
    ];

    /// <summary>
    /// Gets or sets the type.
    /// </summary>
    public VarOperationType Type { get; protected set; }

    /// <summary>
    /// Gets or sets the ordinal character number to start at.
    /// </summary>
    public short At { get; set; }

    /// <summary>
    /// Gets or sets the run of characters to include from <see cref="At"/>.
    /// </summary>
    public short Run { get; set; } = 1;

    /// <summary>
    /// Gets or sets the certainty rank for this operation: 0=not specified,
    /// 1=most probable, 2=less probable, etc.
    /// </summary>
    public short Rank { get; set; }

    /// <summary>
    /// Gets the value argument for this operation. This is the primary argument,
    /// whose meaning varies depending on the operation type.
    /// </summary>
    public string? Value { get; set; }

    /// <summary>
    /// Gets or sets the optional group identifier. When an operation is part
    /// of a group, all the operation in it are considered as a unique virtual
    /// operation.
    /// </summary>
    public string? GroupId { get; set; }

    /// <summary>
    /// Gets or sets an optional note.
    /// </summary>
    public string? Note { get; set; }

    /// <summary>
    /// Gets or sets the version tags.
    /// </summary>
    public IList<string> VersionTags { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="VarOperation"/> class.
    /// </summary>
    /// <param name="type">The operation type.</param>
    protected VarOperation(VarOperationType type)
    {
        Type = type;
        VersionTags = new List<string>();
    }

    protected void AppendHead(StringBuilder sb)
    {
        sb.Append(At).Append('×').Append(Run).Append(TYPE_SYMBOLS[(int)Type]);
    }

    protected void AppendTail(StringBuilder sb)
    {
        // ^RANK@GROUP [TAG,...] {NOTE}
        if (GroupId != null || VersionTags.Count > 0 || Note != null)
        {
            if (Rank != 0) sb.Append('^').Append(Rank);

            if (!string.IsNullOrEmpty(GroupId)) sb.Append('@').Append(GroupId);

            if (VersionTags.Count > 0)
                sb.Append(" [").AppendJoin(" ", VersionTags).Append(']');

            if (Note != null)
                sb.Append(" {").Append(Note).Append('}');
        }
    }

    [GeneratedRegex(@"^(?<at>\d+)[×x](?<run>\d+)?(?<op>=|\+<|\+>|>|<>|:)$")]
    private static partial Regex OpHeadRegex();

    protected static Match MatchOperationHead(string text) =>
        OpHeadRegex().Match(text);

    [GeneratedRegex(
        @"(?<rank>\^[0-9]+)?(?:@(?<group>[^[{\s]+))?(?:\s*\[(?<tags>[^\]]+)\])?(?:\s*\{(?<note>[^\}]+)\})?$")]
    private static partial Regex TailRegex();

    protected static Match MatchOperationTail(string text) =>
        TailRegex().Match(text);

    protected static VarOperationType GetOperationType(string op)
    {
        return op switch
        {
            "=" => VarOperationType.Replace,
            "+<" => VarOperationType.AddLeft,
            "+>" => VarOperationType.AddRight,
            ":" => VarOperationType.Annotate,
            ">" => VarOperationType.Move,
            "<>" => VarOperationType.Swap,
            _ => throw new FormatException($"Invalid operation type: {type}")
        };
    }

    private static string? GetOptionalArgValue(string? value)
    {
        if (string.IsNullOrEmpty(value)) return null;
        value = value.Trim();
        return value.Length == 0 ? null : value;
    }

    protected static string ParseHeadAndTail(string text, VarOperation op)
    {
        ArgumentNullException.ThrowIfNull(text);

        // head: ATxRUN followed by OP
        Match head = MatchOperationHead(text);
        if (head.Success)
            throw new FormatException($"Invalid operation head: {text}");

        op.At = short.Parse(head.Groups["at"].Value,
            CultureInfo.InvariantCulture);
        if (head.Groups["run"].Success)
        {
            op.Run = short.Parse(head.Groups["run"].Value,
                CultureInfo.InvariantCulture);
        }

        // tail: ^RANK@GROUP [TAG...] {NOTE}
        Match tail = MatchOperationTail(text);
        if (tail.Success)
            throw new FormatException($"Invalid operation tail: {text}");

        op.Rank = tail.Groups["rank"].Length > 0
            ? short.Parse(tail.Groups["rank"].Value, CultureInfo.InvariantCulture)
            : (short)0;
        op.GroupId = GetOptionalArgValue(tail.Groups["group"].Value);
        op.VersionTags = tail.Groups["tags"].Value.Split(' ').ToList();
        op.Note = GetOptionalArgValue(tail.Groups["note"].Value);

        // return text minus head and tail
        return text.Substring(head.Length, text.Length - tail.Length);
    }

    protected AnnotatedText CreateResult(string text,
        IDictionary<string, string>? annotations = null)
    {
        AnnotatedText result = new(text, annotations);
        if (!string.IsNullOrEmpty(Note))
            result.Annotations[AnnotatedText.NOTE_KEY] = Note;
        return result;
    }

    public abstract AnnotatedText Execute(AnnotatedText text);

    public AnnotatedText Execute(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        return Execute(new AnnotatedText(text));
    }

    public static VarOperation Parse(string text)
    {
        return GetOperationType(text) switch
        {
            VarOperationType.Replace
                => ReplaceVarOperation.ParseOperation(text),
            VarOperationType.AddLeft
                => AddLeftVarOperation.ParseOperation(text),
            VarOperationType.AddRight
                => AddRightVarOperation.ParseOperation(text),
            VarOperationType.Annotate
                => AnnotateVarOperation.ParseOperation(text),
            VarOperationType.Move
                => MoveVarOperation.ParseOperation(text),
            VarOperationType.Swap
                => SwapVarOperation.ParseOperation(text),
            _ => throw new FormatException(
                $"Invalid operation type: {GetOperationType(text)}")
        };
    }

    public static bool TryParse(string text, out VarOperation? op)
    {
        try
        {
            op = Parse(text);
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.ToString());
            op = null;
            return false;
        }
    }
}

/// <summary>
/// Type of <see cref="VarOperation"/>.
/// </summary>
public enum VarOperationType
{
    /// <summary>The replace operation.</summary>
    Replace = 0,
    /// <summary>The add left operation.</summary>
    AddLeft,
    /// <summary>The add right operation.</summary>
    AddRight,
    /// <summary>The annotate operation.</summary>
    Annotate,
    /// <summary>The move operation.</summary>
    Move,
    /// <summary>The swap operation.</summary>
    Swap
}
