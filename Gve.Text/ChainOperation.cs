using NanoidDotNet;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Gve.Text;

/// <summary>
/// Base class for operations on a <see cref="Chain{T}"/>.
/// </summary>
/// <seealso cref="VarOperationMetadata" />
public abstract partial class ChainOperation<T> : VarOperationMetadata
{
    /// <summary>
    /// Gets or sets the operation identifier. This is used to reference
    /// a specific operation in a transformation, essentially for diagnostic
    /// purposes.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Gets or sets the operation type.
    /// </summary>
    public VarOperationType Type { get; protected set; }

    /// <summary>
    /// Gets or sets the input version tag for this operation. If null,
    /// the default tag (=the tag of the last operation executed on the context)
    /// is used.
    /// </summary>
    public string? InputTag { get; set; }

    /// <summary>
    /// Gets or sets the output version tag for this operation. If null,
    /// a value will be automatically generated from the input tag if this
    /// ends with digits; else, it will be equal to the input tag.
    /// </summary>
    public string? OutputTag { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether <see cref="At"/> refers to
    /// an index rather than a chain node ID.
    /// </summary>
    public bool UseIndex { get; set; }

    /// <summary>
    /// Gets or sets the ordinal character number to start at.
    /// </summary>
    public int At { get; set; } = 1;

    /// <summary>
    /// Gets or sets the run of characters to include from <see cref="At"/>.
    /// </summary>
    public int Run { get; set; } = 1;

    /// <summary>
    /// Gets the value argument for this operation. This is the primary argument,
    /// whose meaning varies depending on the operation type.
    /// </summary>
    public string? Value { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="VarOperation"/> class.
    /// </summary>
    /// <param name="type">The operation type.</param>
    protected ChainOperation(VarOperationType type)
    {
        Id = GenerateId();
        Type = type;
    }

    /// <summary>
    /// Generates a new identifier for an operation.
    /// </summary>
    /// <returns>Identifier.</returns>
    public static string GenerateId() => Nanoid.Generate("1234567890abcdef", 10);

    // TODO remove if not used, else update with tags
    // [@]at, [run]
    [GeneratedRegex(@"^(?<ati>@)?(?<at>[0-9]+)(?:[×x](?<run>[0-9]+))?")]
    protected static partial Regex AtRunRegex();

    // [([it]:[ot])] [@]at, [run], op where it=input tag and ot=output tag
    [GeneratedRegex(@"^(?:\((?<it>[^:)]+)?:(?<ot>[^)]+)?\)\s*)?(?<ati>@)?(?<at>[0-9]+)(?:[×x](?<run>[0-9]+))?(?<op>=|\+<|\+>|>|<>|:)")]
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
    /// Gets the input and output tags for this operation with relation to
    /// the received <paramref name="context"/>.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <returns>Input and output tags.</returns>
    /// <exception cref="ArgumentNullException">context</exception>
    protected (string inputTag, string outputTag) GetTags(
        ChainOperationContext<T> context)
    {
        ArgumentNullException.ThrowIfNull(context);

        string inputTag = InputTag ?? context.LastTag;
        string outputTag = OutputTag ?? GetNextTag(inputTag);
        return (inputTag, outputTag);
    }

    /// <summary>
    /// Gets the nodes representing the boundaries for this operation in relation
    /// with the specified <paramref name="chain"/> and <paramref name="tag"/>.
    /// </summary>
    /// <param name="chain">The target chain.</param>
    /// <param name="tag">The version tag.</param>
    /// <returns>Node and optional end node when this operation works on a range.
    /// </returns>
    /// <exception cref="ArgumentNullException">chain or tag</exception>
    /// <exception cref="InvalidOperationException">Node not found</exception>
    protected (ChainNode<T> node, ChainNode<T>? endNode) GetBoundNodes(
               Chain<T> chain, string tag)
    {
        ArgumentNullException.ThrowIfNull(chain);
        ArgumentNullException.ThrowIfNull(tag);

        // find At node
        ChainNode<T>? node = FindNode(chain, At, tag) ??
            throw new InvalidOperationException(
                "Node not found for the specified AT: " + ToString());

        // if Run is 1, return the node only
        if (Run == 1)
        {
            return (node, null);
        }

        // else find end node by index from Run (there must be one)
        ChainNode<T>? endNode = null;
        for (int i = 0; i < Run; i++)
        {
            endNode = chain.GetNextNode(tag, node);
        }
        if (endNode == null)
        {
            throw new InvalidOperationException(
                "End node not found for the specified RUN: " + ToString());
        }

        return (node, endNode);
    }

    /// <summary>
    /// Gets the output tag from the input <paramref name="tag"/>.
    /// This is the input tag with a version number incremented by 1, when
    /// it ends with digits; else, it is the input tag unless empty; when empty,
    /// the next tag is <c>v1</c>.
    /// </summary>
    /// <param name="tag">The tag.</param>
    /// <returns>Tag.</returns>
    /// <exception cref="ArgumentNullException">tag</exception>
    protected string GetNextTag(string tag)
    {
        ArgumentNullException.ThrowIfNull(tag);

        // if tag is empty, return v1
        if (tag.Length == 0) return "v1";

        // if tag does not end with a digit, return it
        if (tag[^1] < '0' || tag[^1] > '9') return tag;

        // parse the number represented by the last digit(s) in tag
        // and increment it
        int i = tag.Length - 1;
        while (i >= 0 && tag[i] >= '0' && tag[i] <= '9') i--;
        return tag[..(i + 1)] +
            (int.Parse(tag[(i + 1)..], CultureInfo.InvariantCulture) + 1);
    }

    /// <summary>
    /// Appends the head part of an operation to the specified string builder.
    /// The head is ATxRUN (optionally preceded by @) followed by OP.
    /// </summary>
    /// <param name="sb">The target.</param>
    protected void AppendHead(StringBuilder sb)
    {
        if (UseIndex) sb.Append('@');
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
                    p.Value != 0 ? $"{p.Key}={p.Value}" : p.Key))
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

    private static void ParseTaggedOrdinals(string text,
        ChainOperation<T> operation)
    {
        operation.TaggedOrdinals.Clear();

        // text is TAG[=ORDINAL] separated by spaces
        foreach (var groups in TaggedOrdinalRegex().Matches(text)
            .Select(m => m.Groups))
        {
            string value = groups["v"].Success ? groups["v"].Value : "0";

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
    protected static string ParseHeadAndTail(string text,
        ChainOperation<T> operation)
    {
        ArgumentNullException.ThrowIfNull(text);

        // head: (IT:OT) @ATxRUN or ATxRUN followed by OP
        Match head = HeadRegex().Match(text);
        if (!head.Success)
            throw new FormatException($"Invalid operation head: {text}");

        operation.InputTag = GetOptionalArgValue(head.Groups["it"].Value);
        operation.OutputTag = GetOptionalArgValue(head.Groups["ot"].Value);

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
                operation.Features.Add(new Feature(groups["n"].Value,
                    groups["v1"].Success
                        ? groups["v1"].Value.Trim()
                        : groups["v2"].Value.Trim(),
                    groups["u"].Success));
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
    /// <remarks>The <see cref="VarOperationMetadata.Rank"/> property when not
    /// 0 is added as a unique feature; anyway, it can be overridden by another
    /// rank feature directly set in <see cref="VarOperationMetadata.Features"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">features</exception>
    protected void AddFeatures(IList<Feature> features)
    {
        ArgumentNullException.ThrowIfNull(features);
        Feature.UpdateFeatures(Rank != 0
            ? [new Feature(RANK_KEY, $"{Rank}", true), .. Features]
            : Features,
            features);
    }

    /// <summary>
    /// Adds <see cref="VarOperationMetadata.Features"/> to the received node.
    /// </summary>
    /// <param name="node">The node.</param>
    /// <remarks>The <see cref="VarOperationMetadata.Rank"/> property when not
    /// 0 is added as a unique feature; anyway, it can be overridden by another
    /// rank feature directly set in <see cref="VarOperationMetadata.Features"/>.
    /// </remarks>
    protected void AddFeatures(CharNode node)
    {
        ArgumentNullException.ThrowIfNull(node);
        Feature.UpdateFeatures(Rank != 0
            ? [new Feature(RANK_KEY, $"{Rank}", true), .. Features]
            : Features,
            node.Features);
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
    /// Finds the node having its ID or index equal to <paramref name="at"/>.
    /// </summary>
    /// <param name="chain">The chain.</param>
    /// <param name="at">The value of node ID or index.</param>
    /// <param name="tag">The tag defining the context for the index. When
    /// null, this implies that <paramref name="at"/> refers to an ID, which
    /// is not dependent on version tags.</param>
    /// <returns>Node, or null if not found.</returns>
    protected ChainNode<T>? FindNode(Chain<T> chain, int at, string? tag = null)
    {
        return tag != null? chain.FindNodeByIndex(tag, at) : chain.FindNode(at);
    }

    /// <summary>
    /// Executes this operation on the specified context.
    /// </summary>
    /// <param name="context">The context.</param>
    public abstract void Execute(ChainOperationContext<T> context);

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
