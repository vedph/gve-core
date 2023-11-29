using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Gve.Text;

/// <summary>
/// A variant generation operation on the annotations plane.
/// </summary>
/// <seealso cref="VarOperationMetadata" />
public abstract partial class AnnOperation : VarOperationMetadata
{
    private const string NOTE_KEY = "note";

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
    public short At { get; set; } = 1;

    /// <summary>
    /// Gets or sets the run of characters to include from <see cref="At"/>.
    /// </summary>
    public short Run { get; set; } = 1;

    /// <summary>
    /// Gets the value argument for this operation. This is the primary argument,
    /// whose meaning varies depending on the operation type.
    /// </summary>
    public string? Value { get; set; }

    /// <summary>
    /// Gets or sets the optional note to be added to the result features.
    /// </summary>
    public string? Note { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="VarOperation"/> class.
    /// </summary>
    /// <param name="type">The operation type.</param>
    protected AnnOperation(VarOperationType type)
    {
        Type = type;
    }

    [GeneratedRegex(@"^(?<at>[0-9]+)(?:[×x](?<run>[0-9]+))?")]
    protected static partial Regex AtRunRegex();

    [GeneratedRegex(@"^(?<at>[0-9]+)(?:[×x](?<run>[0-9]+))?(?<op>=|\+<|\+>|>|<>|:)")]
    private static partial Regex HeadRegex();

    [GeneratedRegex(
        @"(?:\^(?<rank>[0-9]+))?(?:@(?<group>[^[{\s]+))?(?:\s*\[(?<tags>[^\]]+)\])?(?:\s*\{(?<note>[^\}]+)\})?$")]
    private static partial Regex TailRegex();

    [GeneratedRegex("^[^=+<>:]+([=+<>:]+)")]
    private static partial Regex LocOpRegex();

    /// <summary>
    /// Appends the head part of an operation to the specified string builder.
    /// The head is ATxRUN followed by OP.
    /// </summary>
    /// <param name="sb">The target.</param>
    protected void AppendHead(StringBuilder sb)
    {
        sb.Append(At).Append('×').Append(Run).Append(TYPE_SYMBOLS[(int)Type]);
    }

    /// <summary>
    /// Appends the tail part of an operation to the specified string builder.
    /// </summary>
    /// <param name="sb">The target.</param>
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

    /// <summary>
    /// Gets the operation type from the specified symbol.
    /// </summary>
    /// <param name="op">Operator symbol.</param>
    /// <returns>Type.</returns>
    /// <exception cref="FormatException">Invalid operation type.</exception>
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
            _ => throw new FormatException($"Invalid operation type: {op}")
        };
    }

    private static string? GetOptionalArgValue(string? value)
    {
        if (string.IsNullOrEmpty(value)) return null;
        value = value.Trim();
        return value.Length == 0 ? null : value;
    }

    /// <summary>
    /// Parse head and tail parts of the specified text representing an
    /// operation, storing values into <paramref name="operation"/>.
    /// </summary>
    /// <param name="text">Text.</param>
    /// <param name="operation">Target operation.</param>
    /// <returns>The body part of the text.</returns>
    /// <exception cref="FormatException">Invalid text.</exception>
    protected static string ParseHeadAndTail(string text, AnnOperation operation)
    {
        ArgumentNullException.ThrowIfNull(text);

        // head: ATxRUN followed by OP
        Match head = HeadRegex().Match(text);
        if (!head.Success)
            throw new FormatException($"Invalid operation head: {text}");

        operation.At = short.Parse(head.Groups["at"].Value,
            CultureInfo.InvariantCulture);
        if (head.Groups["run"].Success)
        {
            operation.Run = short.Parse(head.Groups["run"].Value,
                CultureInfo.InvariantCulture);
        }

        // tail: ^RANK@GROUP [TAG...] {NOTE}
        Match tail = TailRegex().Match(text);
        if (!tail.Success)
            throw new FormatException($"Invalid operation tail: {text}");

        operation.Rank = tail.Groups["rank"].Length > 0
            ? short.Parse(tail.Groups["rank"].Value, CultureInfo.InvariantCulture)
            : (short)0;
        operation.GroupId = GetOptionalArgValue(tail.Groups["group"].Value);
        operation.VersionTags = tail.Groups["tags"].Value.Split(' ').ToHashSet();
        operation.Note = GetOptionalArgValue(tail.Groups["note"].Value);

        // return text minus head and tail
        return text[head.Length..^tail.Length];
    }

    /// <summary>
    /// Parses the specified text representing an operation.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <returns>Operation.</returns>
    /// <exception cref="ArgumentNullException">text</exception>
    /// <exception cref="FormatException">Parsing error</exception>
    public static T Parse<T>(string text) where T : AnnOperation
    {
        ArgumentNullException.ThrowIfNull(text);

        Match m = LocOpRegex().Match(text);
        if (!m.Success)
            throw new FormatException($"Invalid operation: {text}");

        return GetOperationType(m.Groups[1].Value) switch
        {
            VarOperationType.Replace
                => (ReplaceAnnOperation.ParseOperation(text) as T)!,
            VarOperationType.AddLeft
                => (AddLeftAnnOperation.ParseOperation(text) as T)!,
            VarOperationType.AddRight
                => (AddRightAnnOperation.ParseOperation(text) as T)!,
            VarOperationType.Annotate
                => (AnnotateAnnOperation.ParseOperation(text) as T)!,
            VarOperationType.Move
                => (MoveAnnOperation.ParseOperation(text) as T)!,
            VarOperationType.Swap
                => (SwapAnnOperation.ParseOperation(text) as T)!,
            _ => throw new FormatException(
                $"Invalid operation type: {GetOperationType(text)}")
        };
    }

    /// <summary>
    /// Tries to parsing the specified text representing an operation.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <param name="operation">The parsed operation.</param>
    /// <returns>True if success.</returns>
    /// <exception cref="ArgumentNullException">text</exception>
    public static bool TryParse<T>(string text, out T? operation)
        where T : AnnOperation
    {
        ArgumentNullException.ThrowIfNull(text);

        try
        {
            operation = Parse<T>(text);
            return true;
        }
        catch (FormatException ex)
        {
            Debug.WriteLine(ex.ToString());
            operation = null;
            return false;
        }
    }

    /// <summary>
    /// Deeply clones the nodes into a new list.
    /// </summary>
    /// <param name="nodes">The nodes to clone.</param>
    /// <returns>A new list with cloned nodes.</returns>
    protected static List<CharNode> CloneNodes(IList<CharNode> nodes)
    {
        ArgumentNullException.ThrowIfNull(nodes);

        List<CharNode> newNodes = new(nodes.Count);
        foreach (CharNode node in nodes) newNodes.Add(node.Clone());
        return newNodes;
    }

    /// <summary>
    /// Adds the <see cref="Note"/> property to annotations if it exists.
    /// </summary>
    /// <param name="annotations">The annotations.</param>
    protected void AddNote(IList<Feature> annotations)
    {
        ArgumentNullException.ThrowIfNull(annotations);

        if (!string.IsNullOrEmpty(Note))
            annotations.Add(new Feature { Name = NOTE_KEY, Value = Note });
    }

    /// <summary>
    /// Executes this operation for the specified nodes, returning a copy of
    /// them.
    /// </summary>
    /// <param name="nodes">The nodes.</param>
    /// <param name="annotations">The current annotations.</param>
    /// <returns>The output nodes.</returns>
    public abstract IList<CharNode> Execute(IList<CharNode> nodes,
        IList<Feature> annotations);
}
