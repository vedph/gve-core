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
    private Dictionary<string, IList<VarGenerationStep>>? _steps;

    /// <summary>
    /// Gets or sets a value indicating whether this generator should track
    /// its generation steps into <see cref="VarGenerationStep"/> objects.
    /// </summary>
    public bool IsTrackingEnabled { get; set; }

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

    private void AddStep(VarGenerationStep step)
    {
        _steps ??= [];

        if (!_steps.TryGetValue(step.VersionTag,
            out IList<VarGenerationStep>? steps))
        {
            steps = new List<VarGenerationStep>();
            _steps[step.VersionTag] = steps;
        }

        steps.Add(step);
    }

    /// <summary>
    /// Gets the specified steps.
    /// </summary>
    /// <param name="tag">The version tag, or null to get all the steps at once.
    /// </param>
    /// <returns>Steps.</returns>
    public IList<VarGenerationStep> GetSteps(string? tag)
    {
        if (_steps == null) return [];

        // if tag is null, get all the steps, else get only thos from tag
        return tag == null
            ? _steps.Values.SelectMany(s => s).ToList()
            : _steps[tag];
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

    private static void ReplayAscendants(TreeNode<AnnVarOperation> node,
        VarOperationContext context, short at, short run)
    {
        // collect ascendants up to root in a stack
        Stack<TreeNode<AnnVarOperation>> stack = new();
        TreeNode<AnnVarOperation>? parent = node.Parent;
        while (parent != null)
        {
            if (parent.Payload != null) stack.Push(parent);
            parent = parent.Parent;
        }

        // execute all the operations from stack on context
        if (stack.Count == 0) return;

        context.SetAnnNodes(at, run);
        while (stack.Count > 0)
        {
            TreeNode<AnnVarOperation> n = stack.Pop();
            n.Payload?.Execute(context);
        }
    }

    private bool ProcessNextEntry(VarGeneratorContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        _logger?.LogInformation("-- EntryIndex {index}", context.EntryIndex);

        // get the entry to process: if there are no more entries, we're done
        if (context.EntryIndex >= context.Entries.Count)
        {
            _logger?.LogInformation("+Adding to results: {result} ({count})",
                context.OpContext.Set.GetText(true),
                context.Results.Count + 1);
            context.Results.Add(context.OpContext.Set.Clone());
            return false;
        }
        VarOperationEntry entry = context.Entries[context.EntryIndex];
        _logger?.LogInformation("B-Operation {op}{tree}",
            entry.Operation, entry.Root != null ? "..." : "");

        // if we just have a simple base operation, execute it if matches version
        if (entry.Root == null)
        {
            if (entry.Operation.TaggedOrdinals.ContainsKey(context.VersionTag))
            {
                entry.Operation.Execute(context.OpContext);
                _logger?.LogInformation("B-Result: {text}",
                    context.OpContext.Set.GetText(true));

                // track if required
                if (IsTrackingEnabled)
                {
                    AddStep(new VarGenerationStep(
                        context.VersionTag,
                        entry,
                        entry.Operation,
                        context.OpContext.Clone()));
                }
            }
            else
            {
                _logger?.LogInformation("Skipping B-operation {op}",
                    entry.Operation);
            }
        }

        // otherwise, we have a tree of operations to process recursively
        else
        {
            // traverse the tree
            _logger?.LogInformation("Traversing tree");
            entry.Root.Traverse((node) =>
            {
                _logger?.LogInformation(":A-Node with {payload}", node.Payload);
                // just continue if no operation for this node
                if (node.Payload == null)
                {
                    _logger?.LogInformation("Skipping empty A-node");
                    return true;
                }

                // replay ascendants operations on context to get in synch
                // with the position of the current node in tree
                _logger?.LogInformation("Replaying ascendants");
                context.OpContext.SetAnnNodes(
                    entry.Operation.At, entry.Operation.Run);

                ReplayAscendants(node, context.OpContext,
                    entry.Operation.At, entry.Operation.Run);

                _logger?.LogInformation("A-Nodes: {text}",
                    context.OpContext.GetAnnText());

                // operation's tag must match
                _logger?.LogInformation("A-Operation {op}", node.Payload);

                if (!node.Payload.TaggedOrdinals.ContainsKey(
                    context.VersionTag))
                {
                    _logger?.LogInformation("Skipping A-operation {op}",
                        node.Payload);
                    return true;
                }

                // create a new context for this node
                VarGeneratorContext newContext = new(context);
                newContext.OpContext.SetAnnNodes(
                    entry.Operation.At, entry.Operation.Run,
                    context.OpContext.AnnNodes);
                _logger?.LogInformation("Branch: A-Nodes: {text}",
                    newContext.OpContext.GetAnnText());

                // execute the operation
                node.Payload.Execute(newContext.OpContext);
                _logger?.LogInformation("A-Result: {text}",
                    newContext.OpContext.GetAnnText());

                // merge results back into base set
                newContext.OpContext.MergeAnnNodes();
                _logger?.LogInformation("Merged set: {text}",
                    newContext.OpContext.Set.GetText(true));

                // track if required
                if (IsTrackingEnabled)
                {
                    AddStep(new VarGenerationStep(
                        newContext.VersionTag,
                        entry,
                        entry.Operation,
                        newContext.OpContext.Clone()));
                }

                newContext.EntryIndex++;

                // recursively process the next entry on the new branch
                _logger?.LogInformation("Recursing...");
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
            _logger?.LogInformation("== Version {tag}", tag);

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
