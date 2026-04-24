using System;
using System.Collections.Generic;

namespace Gve.Text;

/// <summary>
/// A step in a <see cref="ChainOperationContext{T}"/>. This contains metadata
/// resulting after executing a single operation in the chain: the operation
/// metadata, its input and output version tags, and the features accumulated up
/// to this step, both for the whole context and for specific nodes.
/// </summary>
public class ChainOperationContextStep
{
    /// <summary>
    /// Gets the operation.
    /// </summary>
    public OperationMetadata Operation { get; }

    /// <summary>
    /// Gets the input tag.
    /// </summary>
    public string InputTag { get; }

    /// <summary>
    /// Gets or sets the output tag.
    /// </summary>
    public string OutputTag { get; set; }

    /// <summary>
    /// Gets or sets a string synthetically representing the result.
    /// </summary>
    public string? Result { get; set; }

    /// <summary>
    /// Gets or sets the features emitted up to this step.
    /// </summary>
    public ChainOperationContextFeats FeatureSet { get; set; }

    /// <summary>
    /// Gets or sets the reference node IDs. These are the ID of nodes in the
    /// chain that are involved in the operation which generates this step.
    /// For instance, a delete operation would have the deleted node ID(s) as
    /// its reference. Reference nodes are defined by each operation's type.
    /// </summary>
    public IList<int> RefNodeIds { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ChainOperationContextStep"/>
    /// class.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <param name="inputTag">The input tag.</param>
    /// <param name="outputTag">The output tag.</param>
    /// <param name="features">The features.</param>
    /// <param name="refNodeIds">The reference node IDs.</param>
    /// <exception cref="ArgumentNullException">operation or tags</exception>
    public ChainOperationContextStep(OperationMetadata operation,
        string inputTag, string outputTag, ChainOperationContextFeats features,
        IList<int> refNodeIds)
    {
        Operation = operation ?? throw new ArgumentNullException(nameof(operation));
        InputTag = inputTag ?? throw new ArgumentNullException(nameof(inputTag));
        OutputTag = outputTag ?? throw new ArgumentNullException(nameof(outputTag));
        FeatureSet = features ?? throw new ArgumentNullException(nameof(features));
        RefNodeIds = refNodeIds ?? throw new ArgumentNullException(nameof(refNodeIds));
    }

    /// <summary>
    /// Converts to string.
    /// </summary>
    /// <returns>
    /// A <see cref="string" /> that represents this instance.
    /// </returns>
    public override string ToString()
    {
        return $"[{InputTag}:{OutputTag}] {Operation}";
    }
}
