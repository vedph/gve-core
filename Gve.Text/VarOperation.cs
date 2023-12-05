using Microsoft.VisualBasic;
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
    /// Gets or sets the operation identifier. This is used to reference
    /// a specific operation in a transformation, essentially for diagnostic
    /// purposes.
    /// </summary>
    public int Id { get; set; }

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

    // at, [run]
    [GeneratedRegex(@"^(?<at>[0-9]+)(?:[×x](?<run>[0-9]+))?")]
    protected static partial Regex AtRunRegex();

    // at, [run], op
    [GeneratedRegex(@"^(?<at>[0-9]+)(?:[×x](?<run>[0-9]+))?(?<op>=|\+<|\+>|>|<>|:)")]
    private static partial Regex HeadRegex();

    // [rank], [group], [tags], [meta]
    [GeneratedRegex(
        @"(?:\s*\^(?<rank>[0-9]+))?(?:\s*@(?<group>[^[{\s]+))?(?:\s*\[(?<tags>[^]}]+)\])?(?:\s*\{(?<meta>[^}]+)\})?$")]
    private static partial Regex TailRegex();

    // t=tag, [v=value]
    [GeneratedRegex(@"\s*(?<t>[^=]+)\s*(?:=\s*(?<v>[0-9]+))?")]
    private static partial Regex TaggedOrdinalRegex();

    // n=name, [u=unique], [v1 or v2=value]
    // conventionally n starts with ! to remove the feature
    [GeneratedRegex(@"\s*(?<n>[^=]+)\s*(?<u>:)?(?:=\s*(?:(?:""(?<v1>[^""]+)"")|(?<v2>[^\s]+)))?")]
    private static partial Regex FeatureRegex();

    // 1=operator
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
        // ^RANK@GROUP [TAG,...] {NAME[=VALUE] or NAME[="VALUE"]}
        // (if NAME starts with !, remove it from the features)
        if (GroupId != null || TaggedOrdinals.Count > 0 || Features.Count > 0)
        {
            if (Rank != 0) sb.Append('^').Append(Rank);

            if (!string.IsNullOrEmpty(GroupId)) sb.Append('@').Append(GroupId);

            if (TaggedOrdinals.Count > 0)
            {
                sb.Append(" [")
                  .AppendJoin(" ", TaggedOrdinals.Select(p =>
                    p.Value != 0? $"{p.Key}={p.Value}" : p.Key))
                  .Append(']');
            }

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

    private static void ParseTaggedOrdinals(string text, VarOperation operation)
    {
        operation.TaggedOrdinals.Clear();

        // text is TAG[=ORDINAL] separated by spaces
        foreach (var groups in TaggedOrdinalRegex().Matches(text)
            .Select(m => m.Groups))
        {
            string value = groups["v"].Success? groups["v"].Value : "0";

            operation.TaggedOrdinals[groups["t"].Value] =
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

        operation.Rank = tail.Groups["rank"].Success
            ? short.Parse(tail.Groups["rank"].Value, CultureInfo.InvariantCulture)
            : (short)0;
        operation.GroupId = GetOptionalArgValue(tail.Groups["group"].Value);
        ParseTaggedOrdinals(tail.Groups["tags"].Value, operation);

        if (tail.Groups["meta"].Success)
        {
            foreach (var groups in FeatureRegex()
                .Matches(tail.Groups["meta"].Value).Select(m => m.Groups))
            {
                operation.Features.Add(new Feature
                {
                    Name = groups["n"].Value,
                    Value = groups["v1"].Success
                        ? groups["v1"].Value.Trim()
                        : groups["v2"].Value.Trim(),
                    IsUnique = groups["u"].Success
                });
            }
        }

        // return text minus head and tail
        return text[head.Length..^tail.Length];
    }

    /// <summary>
    /// Adds <see cref="VarOperationMetadata.Features"/> to the received
    /// <paramref name="features"/>.
    /// </summary>
    /// <param name="features">The features to update.</param>
    /// <exception cref="ArgumentNullException">features</exception>
    protected void AddFeatures(IList<Feature> features)
    {
        ArgumentNullException.ThrowIfNull(features);
        Feature.UpdateFeatures(Features, features);
    }

    /// <summary>
    /// Adds <see cref="VarOperationMetadata.Features"/> to the received node.
    /// </summary>
    /// <param name="node">The node.</param>
    protected void AddFeatures(CharNode node)
    {
        ArgumentNullException.ThrowIfNull(node);
        Feature.UpdateFeatures(Features, node.Features);
    }

    /// <summary>
    /// Adds <see cref="VarOperationMetadata.Features"/> to the received nodes.
    /// </summary>
    /// <param name="nodes">The nodes.</param>
    protected void AddFeatures(IEnumerable<CharNode> nodes)
    {
        ArgumentNullException.ThrowIfNull(nodes);
        foreach (CharNode node in nodes) AddFeatures(node);
    }

    /// <summary>
    /// Executes this operation on the specified context.
    /// </summary>
    /// <param name="context">The data context.</param>
    public abstract void Execute(VarOperationContext context);

    /// <summary>
    /// Converts to string.
    /// </summary>
    /// <returns>
    /// A <see cref="string" /> that represents this instance.
    /// </returns>
    public override string ToString()
    {
        return $"{At}x{Run}" + TYPE_SYMBOLS[(int)Type] + Value;
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
