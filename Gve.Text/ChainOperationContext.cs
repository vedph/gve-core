using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Gve.Text;

/// <summary>
/// The data context for a chain operation.
/// </summary>
/// <typeparam name="T">The type of chain node payload data.</typeparam>
public class ChainOperationContext<T> : ChainOperationContextFeats
{
    /// <summary>
    /// Gets the chain. This is the target of all the operations for this
    /// context.
    /// </summary>
    public Chain<T> Chain { get; }

    /// <summary>
    /// Gets or sets the version tag of the last operation executed on this
    /// context.
    /// </summary>
    public string LastTag { get; set; }

    /// <summary>
    /// Gets the steps executed on the chain, each consisting of an operation
    /// and input and output version tags.
    /// </summary>
    public List<ChainOperationContextStep> Steps { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ChainOperationContext{T}"/>
    /// class.
    /// </summary>
    public ChainOperationContext()
    {
        Chain = new Chain<T>();
        LastTag = "";
        Steps = [];
    }

    /// <summary>
    /// Gets the maximum tag number among version tags in this context.
    /// </summary>
    /// <returns>Max number.</returns>
    public int GetMaxTagNumber()
    {
        int max = 0;
        foreach (string tag in Chain.GetDistinctTags())
        {
            // parse the tail of tag when ending with digits
            int i = tag.Length - 1;
            while (i >= 0 && char.IsDigit(tag[i]))
            {
                i--;
            }
            if (i < tag.Length - 1)
            {
                int n = int.Parse(tag[(i + 1)..], CultureInfo.InvariantCulture);
                if (n > max) max = n;
            }
        }

        return max;
    }

    /// <summary>
    /// Gets the deleted nodes present in the specified version.
    /// </summary>
    /// <param name="tag">The version tag.</param>
    /// <param name="operationId">The operation identifier, to filter the results
    /// so that only nodes deleted by that operation are returned.</param>
    /// <returns>Nodes.</returns>
    /// <exception cref="ArgumentNullException">tag</exception>
    public IList<ChainNode<T>> GetDeletedNodes(string tag,
        string? operationId = null)
    {
        ArgumentNullException.ThrowIfNull(tag);

        // get all the nodes with feature KEY_DELETED having output version
        // tag equal to tag, sorted by operation ID and node ordinal
        string prefix = tag + "_";
        Dictionary<Tuple<string, int>, ChainNode<T>> nodes = [];
        foreach (string key in NodeFeatures.Keys.Where(k => k.StartsWith(prefix)))
        {
            OperationFeature? del = NodeFeatures[key].Find(
                f => f.Name == OperationMetadata.DEL_KEY);
            if (del == null) continue;

            // extract from del value the operation ID and node ordinal
            // (value has format OPID TAGIN:TAGOUT NR)
            ReadOnlySpan<char> span = del.Value.AsSpan();
            string opId = span[..span.IndexOf(' ')].ToString();
            if (operationId != null && operationId != opId) continue;

            int ordinal = int.Parse(span[(span.LastIndexOf(' ') + 1)..],
                CultureInfo.InvariantCulture);
            nodes[Tuple.Create(opId, ordinal)] = Chain.FindNode(int.Parse(
                key[(prefix.Length)..], CultureInfo.InvariantCulture))!;
        }

        // return nodes sorted by operation ID and node ordinal
        return [.. nodes.Keys.OrderBy(k => k.Item1)
            .ThenBy(k => k.Item2)
            .Select(k => nodes[k])];
    }

    private static void DumpFeatures(List<OperationFeature> features,
        StringBuilder sb, int indent = 2)
    {
        string indentStr = new(' ', indent);
        int totalFeatures = features.Count;
        int maxDigits = totalFeatures.ToString().Length;
        int n = 0;

        foreach (OperationFeature feature in features)
        {
            if (indent > 0) sb.Append(indentStr);
            sb.Append("- ")
              .AppendFormat("{0:D" + maxDigits + "}", ++n)
              .Append('/')
              .AppendFormat("{0:D" + maxDigits + "}", totalFeatures)
              .Append(' ').AppendLine(feature.ToString());
        }
    }

    /// <summary>
    /// Dumps the context or node features in this context into a multine string.
    /// </summary>
    /// <param name="node">if set to <c>true</c> dump node features; else
    /// dump context features.</param>
    /// <returns>Dump string.</returns>
    public string DumpFeatures(bool node = false)
    {
        StringBuilder sb = new();

        if (node)
        {
            // get keys sorted by version (value) and node ID (number)
            var sortedKeys = NodeFeatures.Keys
                .Select(key => (
                    Value: key,
                    Prefix: key.AsSpan(0, key.LastIndexOf('_')).ToString(),
                    Number: int.Parse(key.AsSpan(key.LastIndexOf('_') + 1),
                        CultureInfo.InvariantCulture)
                ))
                .OrderBy(x => x.Prefix, StringComparer.Ordinal)
                .ThenBy(x => x.Number);

            string? prefix = null;
            foreach (var key in sortedKeys)
            {
                if (prefix != null && key.Prefix != prefix) sb.AppendLine();
                prefix = key.Prefix;

                // feature
                sb.Append("- ").Append(key.Value);

                // read node ID from string after the last underscore
                ReadOnlySpan<char> span = key.Value.AsSpan();
                int i = span.LastIndexOf('_');
                int nodeId = int.Parse(span[(i + 1)..],
                    CultureInfo.InvariantCulture);

                // find the node in the chain and dump it
                ChainNode<T>? target = Chain.FindNode(nodeId);
                if (target != null)
                    sb.Append(" (").Append(target.Label).Append(')');

                // append features
                sb.AppendLine(":");
                DumpFeatures(NodeFeatures[key.Value], sb);
            }
        }
        else
        {
            DumpFeatures(Features, sb, 0);
        }

        return sb.ToString();
    }

    /// <summary>
    /// Converts to string.
    /// </summary>
    /// <returns>String.</returns>
    public override string ToString()
    {
        return $"{Chain} ({Steps.Count})";
    }
}
