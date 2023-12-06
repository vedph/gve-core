using Gve.Text.Annotations;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gve.Text;

/// <summary>
/// Text variants generator.
/// </summary>
public sealed class VarGenerator
{
    private static readonly string[] _newlines = ["\r\n", "\r", "\n"];

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
    /// Initializes a new instance of the <see cref="VarGenerator"/> class.
    /// </summary>
    /// <param name="entryText">The text representing the entries to be set.</param>
    /// <param name="logger">The optional logger.</param>
    public VarGenerator(string entryText, ILogger? logger = null)
    {
        _logger = logger;
        Entries = new List<VarOperationEntry>();
        SetEntries(entryText);
    }

    /// <summary>
    /// Set the entries from the specified text.
    /// </summary>
    /// <param name="text">The text to parse.</param>
    /// <exception cref="ArgumentNullException">text</exception>
    /// <exception cref="FormatException">Invalid entry.</exception>
    public void SetEntries(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        Entries.Clear();

        // split the text into lines
        string[] lines = text.Split(_newlines,
            StringSplitOptions.RemoveEmptyEntries);

        // for each line, collect it and all the lines that follow until they
        // do not start with a dash, then parse the collected lines as an entry
        // via VarOperationEntry.Parse and add to the entries
        int id = 1;
        StringBuilder sb = new();

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];
            if (line.StartsWith('-'))
            {
                if (sb.Length > 0)
                {
                    VarOperationEntry? entry = VarOperationEntry.Parse(
                        sb.ToString())
                        ?? throw new FormatException("Invalid entry: " + sb);
                    entry.Operation.Id = id++;
                    Entries.Add(entry);
                    sb.Clear();
                }
                sb.AppendLine(line);
            }
            else
            {
                sb.AppendLine(line);
            }
        }

        // last pending operation if any
        if (sb.Length > 0)
        {
            VarOperationEntry? entry = VarOperationEntry.Parse(
                sb.ToString())
                ?? throw new FormatException("Invalid entry: " + sb);
            entry.Operation.Id = id;
            Entries.Add(entry);
        }
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
    /// <param name="tag">The tag(s).</param>
    /// <returns>The entries.</returns>
    /// <exception cref="ArgumentNullException">tag</exception>
    public IList<VarOperationEntry> GetVersionEntries(string tag)
    {
        ArgumentNullException.ThrowIfNull(tag);

        return Entries.Where(e => e.Operation.TaggedOrdinals.ContainsKey(tag))
            .OrderBy(e => e.Operation.TaggedOrdinals[tag])
            .ToList();
    }

    private bool ProcessNextEntry(VarGeneratorContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        _logger?.LogInformation("EntryIndex {index}", context.EntryIndex);

        // get the entry to process: if there are no more entries, we're done
        if (context.EntryIndex >= context.Entries.Count)
        {
            _logger?.LogInformation("Adding to results: {result}",
                context.OpContext.Set.GetText());
            context.Results.Add(context.OpContext.Set.Clone());
            return false;
        }
        VarOperationEntry entry = context.Entries[context.EntryIndex];

        // if we just have a simple base operation, execute it if matches version
        if (entry.Root == null)
        {
            _logger?.LogInformation("B-Operation {op}", entry.Operation);

            if (entry.Operation.TaggedOrdinals.ContainsKey(context.VersionTag))
            {
                entry.Operation.Execute(context.OpContext);
                _logger?.LogInformation("Result: {text}",
                    context.OpContext.Set.GetText());
            }
            else
            {
                _logger?.LogInformation("Skipping operation {op}", entry.Operation);
            }
        }

        // otherwise, we have a tree of operations to process recursively
        else
        {
            // traverse the tree
            _logger?.LogInformation("Traversing tree");
            entry.Root.Traverse((node) =>
            {
                _logger?.LogInformation("Node with {payload}", node.Payload);

                // just continue if no operation for this node
                if (node.Payload == null) return true;

                // operation's tag must match
                _logger?.LogInformation("A-Operation {op}", node.Payload);

                if (!node.Payload.TaggedOrdinals.ContainsKey(
                    context.VersionTag))
                {
                    _logger?.LogInformation("Skipping operation {op}",
                        node.Payload);
                    return true;
                }

                // create a new context for this node
                VarGeneratorContext newContext = new(context);
                newContext.OpContext.SetAnnNodes(newContext.OpContext.Set,
                    entry.Operation.At, entry.Operation.Run);

                // execute the operation
                node.Payload.Execute(newContext.OpContext);
                _logger?.LogInformation("Result: {text}",
                    string.Join("", context.OpContext.AnnNodes.Select(n => n.Value)));

                // merge results back into base set
                newContext.OpContext.MergeAnnNodes();
                _logger?.LogInformation("Merged set: {text}",
                    context.OpContext.Set.GetText());

                newContext.EntryIndex++;

                // recursively process the next entry on the new branch
                while (ProcessNextEntry(newContext))
                {
                    newContext.EntryIndex++;
                }
                return true;
            });

            // once we're done with the tree, we're done with the generation
            // because all the next entries have been processed recursively
            return false;
        }

        return true;
    }

    /// <summary>
    /// Generates variants from the specified text.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <param name="tags">The optional version tags filter.</param>
    /// <returns>Root node result.</returns>
    public IDictionary<string, IList<CharNodeSet>> Generate(string text,
        IList<string>? tags = null)
    {
        ArgumentNullException.ThrowIfNull(text);

        Dictionary<string, IList<CharNodeSet>> results = [];

        // for each version tag
        foreach (string tag in tags ?? GetVersionTags())
        {
            _logger?.LogInformation("---Version {tag}", tag);

            // create a new context for this tag
            VarGeneratorContext context = new(
                text, tag, GetVersionEntries(tag).ToList());

            // process all the entries
            while (ProcessNextEntry(context)) context.EntryIndex++;

            // store the results
            results[tag] = context.Results;
        }

        return results;
    }
}
