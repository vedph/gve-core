using Gve.Text;
using System;
using System.Collections.Generic;

namespace Gve.Api.Models;

/// <summary>
/// Represents a data transfer object for a single step within a chain operation
/// context, containing operation details, input and output tags, result information,
/// feature set, and reference node identifiers.
/// </summary>
public sealed class ChainOperationContextStepDto
{
    /// <summary>
    /// Gets the operation.
    /// </summary>
    public CharChainOperationDto Operation { get; }

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
    /// Gets or sets the reference node IDs.
    /// </summary>
    public IList<int> RefNodeIds { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ChainOperationContextStep"/>
    /// class.
    /// </summary>
    /// <param name="source">The source step.</param>
    /// <exception cref="ArgumentNullException">step</exception>
    public ChainOperationContextStepDto(ChainOperationContextStep source)
    {
        ArgumentNullException.ThrowIfNull(source);

        Operation = new CharChainOperationDto(
            (CharChainOperation)source.Operation);
        InputTag = source.InputTag;
        OutputTag = source.OutputTag;
        Result = source.Result;
        FeatureSet = source.FeatureSet;
        RefNodeIds = source.RefNodeIds;
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
