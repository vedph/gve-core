using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Gve.Text;

/// <summary>
/// The data context for a chain operation.
/// </summary>
/// <typeparam name="T">The type of chain node data.</typeparam>
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

    private static void DumpFeatures(IList<OperationFeature> features,
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
            foreach (string key in NodeFeatures.Keys.Order())
            {
                sb.Append("- ").Append(key);

                // read node ID from string after the last underscore
                ReadOnlySpan<char> span = key.AsSpan();
                int i = span.LastIndexOf('_');
                int nodeId = int.Parse(span[(i + 1)..], CultureInfo.InvariantCulture);

                // find the node in the chain and dump it
                ChainNode<T>? target = Chain.FindNode(nodeId);
                if (target != null)
                    sb.Append(" (").Append(target.Label).Append(')');

                // append features
                sb.AppendLine(":");
                DumpFeatures(NodeFeatures[key], sb);
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
