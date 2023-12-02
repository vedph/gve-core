using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gve.Text;

/// <summary>
/// Text variants generator.
/// </summary>
public sealed class VarGenerator
{
    private readonly ILogger? _logger;

    /// <summary>
    /// Gets the entries.
    /// </summary>
    public IList<VarOpEntry> Entries { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="VarGenerator"/> class.
    /// </summary>
    /// <param name="logger">The optional logger.</param>
    public VarGenerator(ILogger? logger = null)
    {
        _logger = logger;
        Entries = new List<VarOpEntry>();
    }

    /// <summary>
    /// Gets an ordered list of all the distinct version tags in the generator
    /// entries.
    /// </summary>
    /// <returns>List.</returns>
    public IList<string> GetVersionTags()
    {
        HashSet<string> tags = [];

        foreach (VarOpEntry entry in Entries)
        {
            // base operation
            foreach (string tag in entry.Operation.TaggedOrdinals.Keys)
                tags.Add(tag);

            // tree operations
            entry.Root?.Traverse((node) =>
                {
                    if (node.Payload != null)
                    {
                        foreach (string tag in node.Payload.TaggedOrdinals.Keys)
                            tags.Add(tag);
                    }
                    return true;
                });
        }
        return tags.OrderBy(s => s).ToList();
    }

    public TreeNode<VarOperationResult> Generate(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        TreeNode<VarOperationResult> result = new();
        foreach (string tag in GetVersionTags())
        {
            _logger?.LogInformation("Generating variants for tag {tag}", tag);
            // TODO
        }

        return result;
    }
}
