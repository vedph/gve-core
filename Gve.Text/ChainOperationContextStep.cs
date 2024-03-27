using System;

namespace Gve.Text;

/// <summary>
/// A step in a <see cref="ChainOperationContext{T}"/>.
/// </summary>
public class ChainOperationContextStep
{
    /// <summary>
    /// Gets the operation.
    /// </summary>
    public VarOperationMetadata Operation { get; }

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
    /// Initializes a new instance of the <see cref="ChainOperationContextStep"/>
    /// class.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <param name="inputTag">The input tag.</param>
    /// <param name="outputTag">The output tag.</param>
    /// <param name="features">The features.</param>
    /// <exception cref="ArgumentNullException">operation or tags</exception>
    public ChainOperationContextStep(VarOperationMetadata operation,
        string inputTag, string outputTag, ChainOperationContextFeats features)
    {
        Operation = operation ?? throw new ArgumentNullException(nameof(operation));
        InputTag = inputTag ?? throw new ArgumentNullException(nameof(inputTag));
        OutputTag = outputTag ?? throw new ArgumentNullException(nameof(outputTag));
        FeatureSet = features ?? throw new ArgumentNullException(nameof(features));
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
