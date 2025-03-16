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
/// <seealso cref="OperationMetadata" />
/// <remarks>
/// Initializes a new instance of the <see cref="CharChainOperation"/> class.
/// </remarks>
/// <param name="type">The operation type.</param>
public abstract partial class CharChainOperation(OperationType type) :
    OperationMetadata
{
    /// <summary>
    /// The prefix assigned to all tracing features.
    /// </summary>
    public const string F_TRACE_PREFIX = "$";

    /// <summary>
    /// The suffix for the tag tracing feature.
    /// </summary>
    public const string F_TRACE_TAG = "tag";

    /// <summary>
    /// The suffix for the input left anchor tracing feature.
    /// </summary>
    public const string F_TRACE_ANCHOR_IN_LEFT = "anchor-in-l";

    /// <summary>
    /// The suffix for the input right anchor tracing feature.
    /// </summary>
    public const string F_TRACE_ANCHOR_IN_RIGHT = "anchor-in-r";

    /// <summary>
    /// The suffix for the output left anchor tracing feature.
    /// </summary>
    public const string F_TRACE_ANCHOR_OUT_LEFT = "anchor-out-l";

    /// <summary>
    /// The suffix for the output right anchor tracing feature.
    /// </summary>
    public const string F_TRACE_ANCHOR_OUT_RIGHT = "anchor-out-r";

    /// <summary>
    /// Gets or sets the operation type.
    /// </summary>
    public OperationType Type { get; protected init; } = type;

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

    [GeneratedRegex(@"^(?<tox>@)?(?<to>[0-9]+)")]
    protected static partial Regex ToRegex();

    // [@]to [torun]
    [GeneratedRegex(@"^(?<tox>@)?(?<to>[0-9]+)(?:[×x](?<torun>[0-9]+))?")]
    protected static partial Regex ToRunRegex();

    // [([it]:[ot])] [@]at, [run], op where it=input tag and ot=output tag
    [GeneratedRegex(@"^(?:\((?<it>[^:)]+)?:(?<ot>[^)]+)?\)\s*)?(?<atx>@)?(?<at>[0-9]+)(?:[×x](?<run>[0-9]+))?(?<op>=|-|\+\[|\+\]|>\[|>\]|<>|:)")]
    private static partial Regex HeadRegex();

    // [rank], [group], [feats]
    [GeneratedRegex(
        @"(?:\s*\^(?<rank>[0-9]+))?(?:\s*@(?<group>[^[\s]+))?(?:\s*\[(?<feats>(?:[^]]|""[^""]*"")*)\])?$")]
    private static partial Regex TailRegex();

    // operation feature
    [GeneratedRegex(@"\s*(?:[^:=]+)(?:(?:=|:=|==)(?:(?:""(?:[^""]+)"")|(?:[^\s]+)))?")]
    private static partial Regex FeatureRegex();

    // 1=operator
    [GeneratedRegex(@"^(?:\([^)]*\))?\s*@?[0-9x×]+(=|-|\+\[|\+\]|>\[|>\]|<>|:)")]
    protected static partial Regex LocOpRegex();

    /// <summary>
    /// Gets the input and output tags for this operation with relation to
    /// the received <paramref name="context"/>.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <returns>Input and output tags.</returns>
    /// <exception cref="ArgumentNullException">context</exception>
    public (string inputTag, string outputTag) GetTags(
        ChainOperationContext<char> context)
    {
        ArgumentNullException.ThrowIfNull(context);

        string inputTag = InputTag ?? context.LastTag;
        string outputTag = OutputTag ?? GetNextTag(inputTag, context);
        return (inputTag, outputTag);
    }

    /// <summary>
    /// Gets the nodes referenced by this operation in relation with the
    /// specified <paramref name="chain"/> and <paramref name="tag"/>.
    /// </summary>
    /// <param name="chain">The target chain.</param>
    /// <param name="tag">The version tag.</param>
    /// <param name="at">The optional value of node ID or index. If -1, the
    /// value of <see cref="At"/> will be used.</param>
    /// <param name="run">The optional run of characters to include from
    /// <paramref name="at"/>. If -1, the value of <see cref="Run"/> will be
    /// used.</param>
    /// <returns>Target node(s).</returns>
    /// <exception cref="ArgumentNullException">chain or tag</exception>
    /// <exception cref="InvalidOperationException">Node not found</exception>
    protected IList<ChainNode<char>> GetReferenceNodes(
        Chain<char> chain, string tag, int at = -1, int run = -1)
    {
        ArgumentNullException.ThrowIfNull(chain);
        ArgumentNullException.ThrowIfNull(tag);

        if (at == -1) at = At;
        if (At == 0 && !AtAsIndex) return [];
        if (run == -1) run = Run;

        List<ChainNode<char>> nodes = [];

        // find at node
        ChainNode<char>? node = FindNode(chain, at, AtAsIndex, tag) ??
            throw new InvalidOperationException(
                "Node not found for the specified AT: " + ToString());

        // add the first node
        nodes.Add(node);

        // if run is 1 (or 0, which is used for reference nodes), return
        // the node only
        if (run < 2) return nodes;

        // else find end node by index from run (there must be one)
        ChainNode<char>? next = node;
        for (int i = 1; i < run; i++)
        {
            next = chain.GetNextNode(tag, next);
            if (next == null) break;
            nodes.Add(next);
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
    /// the next tag is <c>v0</c>. If the calculated tag is lower than or
    /// equal to the maximum tag number in the context, it is set to the maximum
    /// tag number + 1. This ensures that we don't overwrite existing versions.
    /// </summary>
    /// <param name="tag">The tag.</param>
    /// <param name="context">The context.</param>
    /// <returns>Tag.</returns>
    /// <exception cref="ArgumentNullException">tag</exception>
    protected static string GetNextTag(string tag,
        ChainOperationContext<char> context)
    {
        ArgumentNullException.ThrowIfNull(tag);

        // if tag is empty, return v0
        if (tag.Length == 0) return "v0";

        // if tag does not end with a digit, return it
        if (tag[^1] < '0' || tag[^1] > '9') return tag;

        // parse the number represented by the last digit(s) in tag
        // and increment it
        int i = tag.Length - 1;
        while (i >= 0 && tag[i] >= '0' && tag[i] <= '9') i--;
        int next = int.Parse(tag[(i + 1)..], CultureInfo.InvariantCulture) + 1;

        // if next is lower than max, set it to max + 1
        int max = context.GetMaxTagNumber();
        if (next <= max) next = max + 1;

        return tag[..(i + 1)] + next;
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
        // ^RANK@GROUP [NAME[=VALUE] or NAME[="VALUE"]]
        // (if NAME starts with !, remove it from the features)
        if (GroupId != null || Features.Count > 0)
        {
            if (Rank != 0) sb.Append('^').Append(Rank);

            if (!string.IsNullOrEmpty(GroupId)) sb.Append('@').Append(GroupId);

            if (Features.Count > 0)
                sb.Append(" [").AppendJoin(" ", Features).Append(']');
        }
    }

    /// <summary>
    /// Gets the operation type from the specified symbol.
    /// </summary>
    /// <param name="op">Operator symbol.</param>
    /// <returns>Type.</returns>
    /// <exception cref="FormatException">Invalid operation type.</exception>
    protected static OperationType GetOperationType(string op)
    {
        return op switch
        {
            "=" => OperationType.Replace,
            "-" => OperationType.Delete,
            "+[" => OperationType.AddBefore,
            "+]" => OperationType.AddAfter,
            ">[" => OperationType.MoveBefore,
            ">]" => OperationType.MoveAfter,
            "<>" => OperationType.Swap,
            ":" => OperationType.Annotate,
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
        operation.At = int.Parse(head.Groups["at"].Value,
            CultureInfo.InvariantCulture);
        if (head.Groups["run"].Success)
        {
            operation.Run = int.Parse(head.Groups["run"].Value,
                CultureInfo.InvariantCulture);
        }

        // tail: ^RANK@GROUP [FEATS]
        Match tail = TailRegex().Match(text[head.Length..]);
        if (!tail.Success)
            throw new FormatException($"Invalid operation tail: {text}");

        operation.Rank = tail.Groups["rank"].Success
            ? short.Parse(tail.Groups["rank"].Value, CultureInfo.InvariantCulture)
            : (short)0;
        operation.GroupId = GetOptionalArgValue(tail.Groups["group"].Value);

        if (tail.Groups["feats"].Success)
        {
            foreach (string feats in FeatureRegex()
                .Matches(tail.Groups["feats"].Value)
                .Select(m => m.Value))
            {
                operation.Features.Add(OperationFeature.Parse(feats.Trim()));
            }
        }

        // return text minus head and tail
        return text[head.Length..^tail.Length];
    }

    /// <summary>
    /// Updates the <see cref="OperationMetadata.Features"/> of the specified
    /// <paramref name="context"/> from this operation.
    /// </summary>
    /// <param name="tag">The output tag of this operation.</param>
    /// <param name="context">The context to update.</param>
    /// <param name="targetNodes">The nodes to be used as targets for features.
    /// If not specified, these will be calculated from the operation's target
    /// range.</param>
    /// <remarks>The <see cref="OperationMetadata.Rank"/> property when not
    /// 0 is added as a unique feature; anyway, it can be overridden by another
    /// rank feature directly set in <see cref="OperationMetadata.Features"/>.
    /// </remarks>
    private void UpdateContextFeatures(string tag,
        ChainOperationContext<char> context,
        IList<ChainNode<char>>? targetNodes = null)
    {
        List<OperationFeature> sources = [..Features];

        // project rank as a unique feature
        if (Rank != 0)
        {
            sources.Add(new OperationFeature(RANK_KEY, $"{Rank}",
                FeatureSetPolicy.Single));
        }

        // project sources as features; if any sources existed, remove them
        // before adding the new ones
        int i = 0;
        foreach (OperationSource vs in Sources)
        {
            sources.Add(new OperationFeature(SOURCE_KEY, vs.ToString(),
                FeatureSetPolicy.SingleFirst,
                i == 0));
            i++;
        }

        // update the context's and/or nodes features
        context.UpdateFeatures(sources, tag,
            targetNodes ?? GetReferenceNodes(context.Chain, tag));
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

    private static string GetVersionString(ChainOperationContext<char> context,
        string tag) =>
        string.Concat(context.Chain.GetTagNodes(tag).Select(n => n.Data));

    /// <summary>
    /// Updates the context metadata after executing the operation.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="inputTag">The input tag.</param>
    /// <param name="outputTag">The output tag.</param>
    /// <param name="refNodes">The nodes referenced by the operation.</param>
    /// <param name="targetNodes">The nodes which should be the target of
    /// node-scoped features, if any.</param>
    protected void UpdateContext(ChainOperationContext<char> context,
        string inputTag, string outputTag,
        IList<ChainNode<char>> refNodes,
        IList<ChainNode<char>> targetNodes)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(inputTag);
        ArgumentNullException.ThrowIfNull(outputTag);
        ArgumentNullException.ThrowIfNull(refNodes);
        ArgumentNullException.ThrowIfNull(targetNodes);

        // update the context's last tag
        context.LastTag = outputTag;

        // update the context's features
        context.CopyTaggedNodeFeatures(inputTag, outputTag);
        UpdateContextFeatures(outputTag, context, targetNodes);

        // add this operation to the context's steps
        context.Steps.Add(new ChainOperationContextStep(
            this, inputTag, outputTag, new ChainOperationContextFeats(context),
            refNodes.Select(n => n.Id).ToList())
        {
            Result = GetVersionString(context, outputTag)
        });
    }

    /// <summary>
    /// Gets the name of the tracing feature with the specified suffix.
    /// </summary>
    /// <param name="suffix">The trace feature suffix. This is the part of the
    /// tracing feature name after the prefix built with <c>$</c> + ID + <c>/</c>.
    /// </param>
    /// <returns>Feature name.</returns>
    protected string GetTracingFeatureName(string suffix)
        => $"{F_TRACE_PREFIX}{Id}/{suffix}";

    /// <summary>
    /// Gets a new tracing feature for this operation.
    /// </summary>
    /// <param name="suffix">The trace feature suffix. This is the part of the
    /// tracing feature name after the prefix built with <c>$</c> + ID + <c>/</c>.
    /// </param>
    /// <param name="value">The optional feature value.</param>
    /// <returns>Feature.</returns>
    public OperationFeature GetTracingFeature(string suffix, string? value = null)
        => new(GetTracingFeatureName(suffix), value, FeatureSetPolicy.Single);

    /// <summary>
    /// Adds trace features to the specified nodes.
    /// </summary>
    /// <param name="inputTag">The input tag.</param>
    /// <param name="outputTag">The output tag.</param>
    /// <param name="afterExecution">if set to <c>true</c>, add the trace
    /// features designed to be added after the operation execution; else,
    /// add those designed to be added before the operation execution.</param>
    /// <param name="nodes">The nodes.</param>
    /// <param name="context">The context.</param>
    protected void AddNodeTraceFeatures(string inputTag, string outputTag,
        bool afterExecution, IList<ChainNode<char>> nodes,
        ChainOperationContext<char> context)
    {
        ArgumentNullException.ThrowIfNull(inputTag);
        ArgumentNullException.ThrowIfNull(outputTag);
        ArgumentNullException.ThrowIfNull(nodes);

        Dictionary<char, IList<OperationFeature>> features = [];

        ChainNode<char>? leftNode = context.Chain.GetPreviousNode(
            inputTag, nodes[0]);
        ChainNode<char>? rightNode = context.Chain.GetNextNode(
            inputTag, nodes[^1]);

        if (afterExecution)
        {
            // output: output anchors for head and tail nodes
            if (leftNode != null)
            {
                features['h'] =
                [
                    GetTracingFeature(F_TRACE_ANCHOR_OUT_LEFT, $"{leftNode.Id}")
                ];
            }
            if (rightNode != null)
            {
                features['t'] =
                [
                    GetTracingFeature(F_TRACE_ANCHOR_OUT_RIGHT, $"{rightNode.Id}")
                ];
            }
        }
        else
        {
            // input: tag for all affected nodes and input anchors for head and
            // tail nodes
            features['b'] =
            [
                GetTracingFeature(F_TRACE_TAG, $"{inputTag}:{outputTag}")
            ];

            if (leftNode != null)
            {
                features['h'] =
                [
                    GetTracingFeature(F_TRACE_ANCHOR_IN_LEFT, $"{leftNode.Id}")
                ];
            }
            if (rightNode != null)
            {
                features['t'] =
                [
                    GetTracingFeature(F_TRACE_ANCHOR_IN_RIGHT, $"{rightNode.Id}")
                ];
            }
        }

        context.UpdateTracingFeatures(features, outputTag, nodes);
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

        OperationType type = GetOperationType(m.Groups[1].Value);
        return type switch
        {
            OperationType.Replace
                => (CharChainReplaceOperation.ParseOperation(text) as O)!,
            OperationType.AddBefore
                => (CharChainAddBeforeOperation.ParseOperation(text) as O)!,
            OperationType.AddAfter
                => (CharChainAddAfterOperation.ParseOperation(text) as O)!,
            OperationType.MoveBefore
                => (CharChainMoveBeforeOperation.ParseOperation(text) as O)!,
            OperationType.MoveAfter
                => (CharChainMoveAfterOperation.ParseOperation(text) as O)!,
            OperationType.Swap
                => (CharChainSwapOperation.ParseOperation(text) as O)!,
            OperationType.Delete
                => (CharChainDeleteOperation.ParseOperation(text) as O)!,
            OperationType.Annotate
                => (CharChainAnnotateOperation.ParseOperation(text) as O)!,
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
