using System;
using System.Collections.Generic;

namespace Gve.Text;

/// <summary>
/// Result of a <see cref="VarOperation"/> operation.
/// </summary>
public class VarOperationResult
{
    /// <summary>
    /// Gets the operation.
    /// </summary>
    public VarOperation Operation { get; }

    /// <summary>
    /// Gets the nodes.
    /// </summary>
    public IList<CharNode> Nodes { get; }

    /// <summary>
    /// Gets the features.
    /// </summary>
    public IList<Feature> Features { get; }

    /// <summary>
    /// Gets the tags.
    /// </summary>
    public HashSet<string> Tags { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="VarOperationResult"/> class.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <param name="nodes">The nodes.</param>
    /// <param name="features">The features.</param>
    /// <param name="tags">The tags.</param>
    /// <exception cref="ArgumentNullException">operation or nodes or features
    /// </exception>
    public VarOperationResult(VarOperation operation, IList<CharNode> nodes,
        IList<Feature> features, IList<string> tags)
    {
        ArgumentNullException.ThrowIfNull(tags);

        Operation = operation ?? throw new ArgumentNullException(nameof(operation));
        Nodes = nodes ?? throw new ArgumentNullException(nameof(nodes));
        Features = features ?? throw new ArgumentNullException(nameof(features));
        Tags = new(tags);
    }

    /// <summary>
    /// Converts to string.
    /// </summary>
    /// <returns>
    /// A <see cref="string" /> that represents this instance.
    /// </returns>
    public override string ToString()
    {
        return $"{Operation} {new CharNodeSet(Nodes)}";
    }
}
