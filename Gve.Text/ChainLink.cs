using System;

namespace Gve.Text;

/// <summary>
/// A link between two nodes in a <see cref="Chain{T}"/>.
/// </summary>
/// <typeparam name="T">The node data type.</typeparam>
public class ChainLink<T>
{
    /// <summary>
    /// Gets or sets the version tag for this link.
    /// </summary>
    public string Tag { get; set; } = "";

    /// <summary>
    /// Gets or sets the source node.
    /// </summary>
    public ChainNode<T>? Source { get; set; }

    /// <summary>
    /// Gets or sets the target node.
    /// </summary>
    public ChainNode<T>? Target { get; set; }

    /// <summary>
    /// Gets a value indicating whether this link is a head link.
    /// </summary>
    public bool IsHead => Source is null;

    /// <summary>
    /// Gets a value indicating whether this link is a tail link.
    /// </summary>
    public bool IsTail => Target is null;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChainLink{T}"/> class.
    /// </summary>
    public ChainLink()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ChainLink{T}"/> class.
    /// </summary>
    /// <param name="tag">The tag.</param>
    /// <param name="source">The source.</param>
    /// <param name="target">The target.</param>
    /// <exception cref="ArgumentNullException">tag</exception>
    /// <exception cref="ArgumentException">Both source and target are null.
    /// </exception>"
    public ChainLink(string tag, ChainNode<T>? source, ChainNode<T>? target)
    {
        ArgumentNullException.ThrowIfNull(tag);
        if (source is null && target is null)
        {
            throw new ArgumentException("Both source and target in a ChainLink " +
                "cannot be null.");
        }

        Tag = tag;
        Source = source;
        Target = target;
    }

    /// <summary>
    /// Converts to string.
    /// </summary>
    /// <returns>string</returns>
    public override string ToString()
    {
        if (IsHead) return $"({Tag}) |{Target}";
        if (IsTail) return $"({Tag}) {Source}|";
        return $"({Tag}) {Source} => {Target}";
    }
}
