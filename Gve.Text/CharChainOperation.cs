using NanoidDotNet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Gve.Text;

/// <summary>
/// Base class for operations on a <see cref="Chain{T}"/>.
/// </summary>
/// <seealso cref="VarOperationMetadata" />
public abstract partial class CharChainOperation : VarOperationMetadata
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
    public bool AtAsIndex { get; set; }

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
    protected CharChainOperation(VarOperationType type)
    {
        Id = GenerateId();
        Type = type;
    }

    /// <summary>
    /// Generates a new identifier for an operation.
    /// </summary>
    /// <returns>Identifier.</returns>
    public static string GenerateId() => Nanoid.Generate("1234567890abcdef", 10);

    // [@]at, [run], [@]to [torun]
    [GeneratedRegex(@"^(?<atx>@)?(?<at>[0-9]+)(?:[×x](?<run>[0-9]+))?[=\-\+\[\]<>:]+(?<tox>@)?(?<to>[0-9]+)(?:[×x](?<torun>[0-9]+))?")]
    protected static partial Regex AtRunToRegex();

    // [([it]:[ot])] [@]at, [run], op where it=input tag and ot=output tag
    [GeneratedRegex(@"^(?:\((?<it>[^:)]+)?:(?<ot>[^)]+)?\)\s*)?(?<atx>@)?(?<at>[0-9]+)(?:[×x](?<run>[0-9]+))?(?<op>=|-|\+\[|\+\]|>\[|>\]|<>|:)")]
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
    [GeneratedRegex(@"^(?:\([^)]*\))?@?[0-9x×]+(=|-|\+\[|\+\]|>\[|>\]|<>|:)")]
    protected static partial Regex LocOpRegex();

    /// <summary>
    /// Gets the input and output tags for this operation with relation to
    /// the received <paramref name="context"/>.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <returns>Input and output tags.</returns>
    /// <exception cref="ArgumentNullException">context</exception>
    protected (string inputTag, string outputTag) GetTags(
        ChainOperationContext<char> context)
    {
        ArgumentNullException.ThrowIfNull(context);

        string inputTag = InputTag ?? context.LastTag;
        string outputTag = OutputTag ?? GetNextTag(inputTag);
        return (inputTag, outputTag);
    }

    /// <summary>
    /// Gets the nodes representing targeted by this operation in relation
    /// with the specified <paramref name="chain"/> and <paramref name="tag"/>.
    /// </summary>
    /// <param name="chain">The target chain.</param>
    /// <param name="tag">The version tag.</param>
    /// <param name="at">The optional value of node ID or index. If -1, the
    /// value of <see cref="At"/> will be used.</param>
    /// <param name="run">The optional run of characters to include from
    /// <paramref name="at"/>. If -1, the value of <see cref="Run"/> will be
    /// used.</param>
    /// <returns>Node and optional end node when this operation works on a range.
    /// </returns>
    /// <exception cref="ArgumentNullException">chain or tag</exception>
    /// <exception cref="InvalidOperationException">Node not found</exception>
    protected IList<ChainNode<char>> GetTargetNodes(
        Chain<char> chain, string tag, int at = -1, int run = -1)
    {
        ArgumentNullException.ThrowIfNull(chain);
        ArgumentNullException.ThrowIfNull(tag);

        if (at == -1) at = At;
        if (run == -1) run = Run;

        List<ChainNode<char>> nodes = [];

        // find at node
        ChainNode<char>? node = FindNode(chain, at, AtAsIndex, tag) ??
            throw new InvalidOperationException(
                "Node not found for the specified AT: " + ToString());

        // add the first node
        nodes.Add(node);

        // if run is 1, return the node only
        if (run == 1)
        {
            return nodes;
        }

        // else find end node by index from run (there must be one)
        ChainNode<char>? next = null;
        for (int i = 0; i < run; i++)
        {
            next = chain.GetNextNode(tag, node);
            if (next != null) nodes.Add(next);
        }
        if (next == null)
        {
            throw new InvalidOperationException(
                "End node not found for the specified RUN: " + ToString());
        }

        return nodes;
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
    protected static string GetNextTag(string tag)
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
    /// Gets a new set of nodes representing <paramref name="text"/>.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <returns>Nodes.</returns>
    protected static List<ChainNode<char>> GetNodesFrom(string text)
    {
        return text.Select((c, _) =>
            new ChainNode<char>(c, $"{(char.IsWhiteSpace(c) ? '_' : c)}"))
            .ToList();
    }

    /// <summary>
    /// Appends the head part of an operation to the specified string builder.
    /// The head is ATxRUN (optionally preceded by @) followed by OP.
    /// </summary>
    /// <param name="sb">The target.</param>
    protected void AppendHead(StringBuilder sb)
    {
        if (AtAsIndex) sb.Append('@');
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
            "-" => VarOperationType.Delete,
            "+[" => VarOperationType.AddBefore,
            "+]" => VarOperationType.AddAfter,
            ">[" => VarOperationType.MoveBefore,
            ">]" => VarOperationType.MoveAfter,
            "<>" => VarOperationType.Swap,
            ":" => VarOperationType.Annotate,
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
        CharChainOperation operation)
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
        CharChainOperation operation)
    {
        ArgumentNullException.ThrowIfNull(text);

        // head: (IT:OT) @ATxRUN or ATxRUN followed by OP
        Match head = HeadRegex().Match(text);
        if (!head.Success)
            throw new FormatException($"Invalid operation head: {text}");

        operation.InputTag = GetOptionalArgValue(head.Groups["it"].Value);
        operation.OutputTag = GetOptionalArgValue(head.Groups["ot"].Value);

        operation.AtAsIndex = head.Groups["atx"].Success;
        operation.At = short.Parse(head.Groups["at"].Value,
            CultureInfo.InvariantCulture);
        if (head.Groups["run"].Success)
        {
            operation.Run = short.Parse(head.Groups["run"].Value,
                CultureInfo.InvariantCulture);
        }

        // tail: ^RANK@GROUP [TAG...] {NOTE}
        Match tail = TailRegex().Match(text[head.Length..]);
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
    /// Updates the <see cref="VarOperationMetadata.Features"/> of the specified
    /// <paramref name="context"/> from this operation.
    /// </summary>
    /// <param name="tag">The output tag of this operation.</param>
    /// <param name="context">The context to update.</param>
    /// <remarks>The <see cref="VarOperationMetadata.Rank"/> property when not
    /// 0 is added as a unique feature; anyway, it can be overridden by another
    /// rank feature directly set in <see cref="VarOperationMetadata.Features"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">features</exception>
    private void UpdateContextFeatures(string tag,
        ChainOperationContext<char> context)
    {
        ArgumentNullException.ThrowIfNull(tag);
        ArgumentNullException.ThrowIfNull(context);

        // add rank feature if not 0
        IList<Feature> sources = Rank != 0
            ? [new Feature(RANK_KEY, $"{Rank}", true), .. Features]
            : Features;

        // update the context's and/or nodes features
        context.UpdateFeatures(sources, tag, GetTargetNodes(context.Chain, tag));
    }

    /// <summary>
    /// Finds the node having its ID or index equal to <paramref name="at"/>.
    /// </summary>
    /// <param name="chain">The chain.</param>
    /// <param name="at">The value of node ID or index.</param>
    /// <param name="atAsIndex">if set to <c>true</c>, <paramref name="at"/>
    /// represents an index rather than an ID.</param>
    /// <param name="tag">The tag defining the context for the index. This
    /// can be null when <paramref name="at"/> refers to an ID, which
    /// is not dependent on version tags.</param>
    /// <returns>Node, or null if not found.</returns>
    protected static ChainNode<char>? FindNode(Chain<char> chain, int at,
        bool atAsIndex = false, string? tag = null)
    {
        return atAsIndex
            ? chain.FindNodeByIndex(tag ?? "", at)
            : chain.FindNode(at);
    }

    /// <summary>
    /// Executes this operation on the specified context.
    /// </summary>
    /// <param name="context">The context.</param>
    public abstract void Execute(ChainOperationContext<char> context);

    /// <summary>
    /// Updates the context metadata after executing the operation.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="inputTag">The input tag.</param>
    /// <param name="outputTag">The output tag.</param>
    protected void UpdateContext(ChainOperationContext<char> context,
        string inputTag, string outputTag)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(inputTag);
        ArgumentNullException.ThrowIfNull(outputTag);

        // update the context's last tag
        context.LastTag = outputTag;

        // add features to context/nodes
        UpdateContextFeatures(inputTag, context);

        // add this operation to the context's steps
        context.Steps.Add(new ChainOperationContextStep(
            this, inputTag, outputTag, new ChainOperationContextFeats(context)));
    }

    /// <summary>
    /// Parses the specified text representing an operation.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <returns>Operation.</returns>
    /// <exception cref="ArgumentNullException">text</exception>
    /// <exception cref="FormatException">Parsing error</exception>
    public static O Parse<O>(string text) where O : CharChainOperation
    {
        ArgumentNullException.ThrowIfNull(text);

        Match m = LocOpRegex().Match(text);
        if (!m.Success)
            throw new FormatException($"Invalid operation: {text}");

        VarOperationType type = GetOperationType(m.Groups[1].Value);
        return type switch
        {
            VarOperationType.Replace
                => (CharChainReplaceOperation.ParseOperation(text) as O)!,
            VarOperationType.AddBefore
                => (CharChainAddBeforeOperation.ParseOperation(text) as O)!,
            VarOperationType.AddAfter
                => (CharChainAddAfterOperation.ParseOperation(text) as O)!,
            VarOperationType.MoveBefore
                => (CharChainMoveBeforeOperation.ParseOperation(text) as O)!,
            VarOperationType.MoveAfter
                => (CharChainMoveAfterOperation.ParseOperation(text) as O)!,
            VarOperationType.Swap
                => (CharChainSwapOperation.ParseOperation(text) as O)!,
            VarOperationType.Delete
                => (CharChainDeleteOperation.ParseOperation(text) as O)!,
                // TODO annotate
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
    public static bool TryParse<O>(string text, out O? operation)
        where O : CharChainOperation
    {
        ArgumentNullException.ThrowIfNull(text);

        try
        {
            operation = Parse<O>(text);
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
    /// Converts to string.
    /// </summary>
    /// <returns>
    /// A <see cref="string" /> that represents this instance.
    /// </returns>
    public override string ToString()
    {
        return $"{(AtAsIndex ? "@" : "")}{At}x{Run}" +
            TYPE_SYMBOLS[(int)Type] + Value;
    }
}
