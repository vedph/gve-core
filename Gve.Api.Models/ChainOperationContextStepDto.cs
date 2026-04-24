using Gve.Text;
using System;
using System.Collections.Generic;

namespace Gve.Api.Models;

/// <summary>
/// Data transfer object for a single step within a chain operation context.
/// Records the operation metadata, input/output version tags, the result text,
/// and the IDs of nodes referenced by the operation.
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
    /// Gets the output tag.
    /// </summary>
    public string OutputTag { get; }

    /// <summary>
    /// Gets a string synthetically representing the result (the text produced
    /// by this operation).
    /// </summary>
    public string? Result { get; }

    /// <summary>
    /// Gets the reference node IDs (the nodes involved in this operation,
    /// e.g. deleted nodes for a delete operation).
    /// </summary>
    public IList<int> RefNodeIds { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ChainOperationContextStepDto"/>
    /// class from a source step.
    /// </summary>
    /// <param name="source">The source step.</param>
    /// <exception cref="ArgumentNullException">source</exception>
    public ChainOperationContextStepDto(ChainOperationContextStep source)
    {
        ArgumentNullException.ThrowIfNull(source);

        Operation = new CharChainOperationDto(
            (CharChainOperation)source.Operation);
        InputTag = source.InputTag;
        OutputTag = source.OutputTag;
        Result = source.Result;
        RefNodeIds = source.RefNodeIds;
    }

    /// <summary>
    /// Converts to string.
    /// </summary>
    public override string ToString() => $"[{InputTag}:{OutputTag}] {Operation}";
}
