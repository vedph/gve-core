using Fusi.Tools.Data;
using Proteus.Rendering;
using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.Json;

namespace Gve.Migration;

/// <summary>
/// Mermaid diagram helper.
/// </summary>
public static class MermaidHelper
{
    private static byte[] ZlibCompress(byte[] data)
    {
        using MemoryStream output = new();
        using (var compressor = new ZLibStream(output, CompressionLevel.Optimal,
            leaveOpen: true))
        {
            compressor.Write(data, 0, data.Length);
        }
        return output.ToArray();
    }

    /// <summary>
    /// Build a Mermaid Live Editor URL from the specified Mermaid code.
    /// </summary>
    /// <param name="mermaidCode">The Mermaid code.</param>
    /// <returns>The URL to the Mermaid Live Editor.</returns>
    /// <exception cref="ArgumentNullException">Thrown if
    /// <paramref name="mermaidCode"/> is null.</exception>
    public static string BuildMermaidLiveUrl(string mermaidCode)
    {
        ArgumentNullException.ThrowIfNull(mermaidCode);

        // build the JSON payload expected by mermaid.live
        var payload = new
        {
            code = mermaidCode,
            mermaid = new { theme = "default" }
        };

        string json = JsonSerializer.Serialize(payload);

        // compress using DEFLATE (raw, no headers)
        byte[] raw = Encoding.UTF8.GetBytes(json);
        byte[] compressed = ZlibCompress(raw);
        string encoded = Convert.ToBase64String(compressed);
        return "https://mermaid.live/edit#pako:" + encoded;
    }

    /// <summary>
    /// Converts a tree of <see cref="ExportedSegment"/> to a Mermaid diagram.
    /// </summary>
    /// <param name="tree">The tree to convert.</param>
    /// <param name="horizontal">Whether the diagram should be horizontal.</param>
    /// <param name="yx">Whether to include YX coordinates in the labels.</param>
    /// <param name="defaultLabel">The default label to use if a node has no label.
    /// </param>
    /// <returns>The Mermaid diagram as a string.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="tree"/>
    /// is null.</exception>
    public static string TreeToMermaid(TreeNode<ExportedSegment> tree,
        bool horizontal = false, bool yx = false, string? defaultLabel = null)
    {
        ArgumentNullException.ThrowIfNull(tree);

        // convert the tree to a Mermaid diagram (flowchart TD)
        StringBuilder sb = new("flowchart");
        sb.Append(horizontal ? " LR" : " TD").AppendLine();

        TreeNode<ExportedSegment> clone = tree.Clone();
        clone.SetAutoIds();
        clone.Traverse(node =>
        {
            if (node.Parent != null)
                sb.Append(node.Parent.Id ?? "root").Append(" --> ");

            (int y, int x) = node.GetYX();
            sb.Append(node.Id ?? "root");

            string? label = yx
                ? $"{y}.{x} {node.Label ?? defaultLabel}"
                : node.Label ?? defaultLabel;
            if (label != null) sb.AppendFormat("[{0}]", label);

            sb.AppendLine();

            return true;
        });

        return sb.ToString();
    }
}
