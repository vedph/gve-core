using System;
using System.Collections.Generic;
using System.Linq;

namespace Gve.Text;

/// <summary>
/// Data context for any variant text generation operation.
/// </summary>
public class VarOperationContext
{
    /// <summary>
    /// Gets the set of character nodes to work on for base operations.
    /// </summary>
    public CharNodeSet Set { get; }

    /// <summary>
    /// Gets the tree nodes to work on for annotation operations. These are
    /// initially cloned from the <see cref="Set"/> and then modified by the
    /// annotation operations.
    /// </summary>
    public List<CharNode> TreeNodes { get; }

    /// <summary>
    /// Gets the features attached to the context by the various operations
    /// executed on it.
    /// </summary>
    public IList<Feature> Features { get; }

    /// <summary>
    /// Gets or sets the tags attached to the context by the various operations
    /// executed on it.
    /// </summary>
    public HashSet<string> Tags { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="VarOperationContext"/> class.
    /// </summary>
    /// <param name="set">The set.</param>
    public VarOperationContext(CharNodeSet set)
    {
        Set = set ?? throw new ArgumentNullException(nameof(set));
        TreeNodes = set.Where(n => !n.IsDeleted).Select(n => n.Clone()).ToList();
        Features = new List<Feature>();
        Tags = [];
    }

    /// <summary>
    /// Gets the text represented by <see cref="TreeNodes"/>.
    /// </summary>
    /// <returns>String.</returns>
    public string GetTreeText()
    {
        return string.Join("", TreeNodes.Select(n => n.Value));
    }

    /// <summary>
    /// Converts to string.
    /// </summary>
    /// <returns>String.</returns>
    public override string ToString()
    {
        return $"[{string.Join(" ", Tags)}] {Set.GetText()} ({Features.Count})";
    }
}
