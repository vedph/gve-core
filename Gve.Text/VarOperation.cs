using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Gve.Text;

/// <summary>
/// Base class for text variant generation operations.
/// </summary>
/// <seealso cref="VarOperationMetadata" />
public abstract partial class VarOperation : VarOperationMetadata
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
    /// Initializes a new instance of the <see cref="VarOperation"/> class.
    /// </summary>
    /// <param name="type">The operation type.</param>
    protected VarOperation(VarOperationType type)
    {
        Type = type;
    }

    [GeneratedRegex(@"^(?<at>[0-9]+)(?:[×x](?<run>[0-9]+))?")]
    protected static partial Regex AtRunRegex();

    [GeneratedRegex(@"^(?<at>[0-9]+)(?:[×x](?<run>[0-9]+))?(?<op>=|\+<|\+>|>|<>|:)")]
    private static partial Regex HeadRegex();

    [GeneratedRegex(
        @"(?:\^(?<rank>[0-9]+))?(?:@(?<group>[^[{\s]+))?(?:\s*\[(?<tags>[^\]]+)\])?(?:\s*\{(?<meta>[^\}]+)\})?$")]
    private static partial Regex TailRegex();

    [GeneratedRegex(@"(?<n>[^=]+)=(?<v>(?:""[^""]+"")|(?:[^\s]+))")]
    private static partial Regex MetadataRegex();

    [GeneratedRegex("^[^=+<>:]+([=+<>:]+)")]
    protected static partial Regex LocOpRegex();

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
        // ^RANK@GROUP [TAG,...] {NAME=VALUE or NAME="VALUE"}
        if (GroupId != null || TaggedOrdinals.Count > 0 || Features.Count > 0)
        {
            if (Rank != 0) sb.Append('^').Append(Rank);

            if (!string.IsNullOrEmpty(GroupId)) sb.Append('@').Append(GroupId);

            if (TaggedOrdinals.Count > 0)
                sb.Append(" [").AppendJoin(" ", TaggedOrdinals).Append(']');

            if (Features.Count > 0)
                sb.Append(" {").AppendJoin(" ", Features).Append('}');
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

    [GeneratedRegex(@"\s*(?<n>[^=]+)\s*(?<u>:)?=\s*(?<v1>(?:""[^""]+"")|(?<v2>[^\s]+))")]
    private static partial Regex NameValueRegex();

    private static void ParseTaggedOrdinals(string text, VarOperation operation)
    {
        operation.TaggedOrdinals.Clear();

        // text is TAG=ORDINAL separated by spaces
        foreach (Match m in NameValueRegex().Matches(text))
        {
            string value = m.Groups["v1"].Success
                ? m.Groups["v1"].Value : m.Groups["v2"].Value;

            operation.TaggedOrdinals[m.Groups["n"].Value] =
                int.Parse(value, CultureInfo.InvariantCulture);
        }
    }

    /// <summary>
    /// Parse head and tail parts of the specified text representing an
    /// operation, storing values into <paramref name="operation"/>.
    /// </summary>
    /// <param name="text">Text.</param>
    /// <param name="operation">Target operation.</param>
    /// <returns>The body part of the text.</returns>
    /// <exception cref="FormatException">Invalid text.</exception>
    protected static string ParseHeadAndTail(string text, VarOperation operation)
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
        ParseTaggedOrdinals(tail.Groups["tags"].Value, operation);

        if (tail.Groups["meta"].Success)
        {
            foreach (Match m in MetadataRegex().Matches(tail.Groups["meta"].Value))
            {
                operation.Features.Add(new Feature
                {
                    Name = m.Groups["n"].Value,
                    Value = m.Groups["v"].Value.Trim('"'),
                    IsUnique = m.Groups["u"].Success
                });
            }
        }

        // return text minus head and tail
        return text[head.Length..^tail.Length];
    }

    /// <summary>
    /// Adds <see cref="VarOperationMetadata.Features"/> to the received
    /// <paramref name="annotations"/>.
    /// </summary>
    /// <param name="annotations">The annotations to update.</param>
    protected void AddFeatures(IList<Feature> annotations)
    {
        ArgumentNullException.ThrowIfNull(annotations);

        foreach (Feature feature in Features)
        {
            // if unique, first remove all the features with the same name
            if (feature.IsUnique)
            {
                for (int i = Features.Count - 1; i >= 0; i--)
                {
                    if (Features[i].Name == feature.Name)
                        Features.RemoveAt(i);
                }
            }
            annotations.Add(feature);
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
