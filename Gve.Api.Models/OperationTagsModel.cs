using System;

namespace Gve.Api.Models;

/// <summary>
/// Operation tags model.
/// </summary>
public class OperationTagsModel(string inputTag, string outputTag)
{
    /// <summary>
    /// Gets the input tag.
    /// </summary>
    public string InputTag { get; } =
        inputTag ?? throw new ArgumentNullException(nameof(inputTag));

    /// <summary>
    /// Gets the output tag.
    /// </summary>
    public string OutputTag { get; } =
        outputTag ?? throw new ArgumentNullException(nameof(outputTag));

    /// <summary>
    /// Converts to string.
    /// </summary>
    /// <returns>
    /// A <see cref="string" /> that represents this instance.
    /// </returns>
    public override string ToString()
    {
        return $"{InputTag}:{OutputTag}";
    }
}
