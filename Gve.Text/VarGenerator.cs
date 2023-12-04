using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Gve.Text;

/// <summary>
/// Text variants generator.
/// </summary>
public sealed class VarGenerator
{
    private readonly ILogger? _logger;

    /// <summary>
    /// Gets the unsorted base operation entries to apply to the base text
    /// to generate its variants.
    /// </summary>
    public IList<VarOperationEntry> Entries { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="VarGenerator"/> class.
    /// </summary>
    /// <param name="logger">The optional logger.</param>
    public VarGenerator(ILogger? logger = null)
    {
        _logger = logger;
        Entries = new List<VarOperationEntry>();
    }

    /// <summary>
    /// Gets an ordered list of all the distinct version tags in the generator
    /// entries.
    /// </summary>
    /// <returns>List.</returns>
    public IList<string> GetVersionTags()
    {
        HashSet<string> tags = [];

        foreach (VarOperationEntry entry in Entries)
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

    /// <summary>
    /// Gets the operation entries for the specified <paramref name="tag"/>,
    /// sorted by their ordinal for that tag.
    /// </summary>
    /// <param name="tag">The tag.</param>
    /// <returns>The entries.</returns>
    /// <exception cref="ArgumentNullException">tag</exception>
    public IEnumerable<VarOperationEntry> GetVersionEntries(string tag)
    {
        ArgumentNullException.ThrowIfNull(tag);

        return Entries.Where(e => e.Operation.TaggedOrdinals.ContainsKey(tag))
            .OrderBy(e => e.Operation.TaggedOrdinals[tag]);
    }

    private void ProcessNextEntry(VarGeneratorContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        _logger?.LogInformation("EntryIndex {index}", context.EntryIndex);

        // get the entry to process: if there are no more entries, we're done
        if (context.EntryIndex >= context.Entries.Count)
        {
            _logger?.LogInformation("Result {count}", context.Results.Count);
            context.Results.Add(context.OpContext.Set.Clone());
            return;
        }
        VarOperationEntry entry = context.Entries[context.EntryIndex];

        // if we just have a simple base operation, execute it if matches version
        if (entry.Root == null)
        {
            _logger?.LogInformation("Operation {op}", entry.Operation);

            if (entry.Operation.TaggedOrdinals.ContainsKey(context.VersionTag))
            {
                entry.Operation.Execute(context.OpContext);
            }
            else
            {
                _logger?.LogInformation("Skipping operation {op}", entry.Operation);
            }

            context.EntryIndex++;
        }
        // otherwise, we have a tree of operations to process recursively
        else
        {
            _logger?.LogInformation("Tree {root}", entry.Root);
            entry.Root.Traverse((node) =>
            {
                _logger?.LogInformation("Node {node}", node);
                if (node.Payload != null)
                {
                    if (!node.Payload.TaggedOrdinals.ContainsKey(context.VersionTag))
                    {
                        _logger?.LogInformation("Skipping node {node}", node);
                        return true;
                    }

                    // create a new context for this node
                    VarGeneratorContext newContext = new(context);

                    // execute the operation
                    node.Payload.Execute(newContext.OpContext);

                    // process the next entry
                    newContext.EntryIndex++;
                    ProcessNextEntry(newContext);
                }
                return true;
            });
        }
    }

    /// <summary>
    /// Generates variants from the specified text.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <returns>Root node result.</returns>
    public IDictionary<string, IList<CharNodeSet>> Generate(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        Dictionary<string, IList<CharNodeSet>> results = [];

        // for each version tag
        foreach (string tag in GetVersionTags())
        {
            _logger?.LogInformation("Version {tag}", tag);

            // create a new context for this tag
            VarGeneratorContext context = new(
                text, tag, GetVersionEntries(tag).ToList());

            // process all the entries recursively
            ProcessNextEntry(context);

            // store the results
            results[tag] = context.Results;
        }

        return results;
    }
}
