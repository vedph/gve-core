using System;
using System.Collections.Generic;

namespace Gve.Text;

/// <summary>
/// A result from a variant generation operation.
/// </summary>
public class VarGeneratorResult
{
    /// <summary>
    /// Gets the set of nodes representing the result.
    /// </summary>
    public CharNodeSet Set { get; }

    /// <summary>
    /// Gets the features attached to this result.
    /// </summary>
    public IList<Feature> Features { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="VarGeneratorResult"/> class.
    /// </summary>
    /// <param name="set">The set.</param>
    /// <param name="features">The features.</param>
    /// <exception cref="ArgumentNullException">set or features</exception>
    public VarGeneratorResult(CharNodeSet set, IList<Feature> features)
    {
        Set = set ?? throw new ArgumentNullException(nameof(set));
        Features = features ?? throw new ArgumentNullException(nameof(features));
    }

    /// <summary>
    /// Converts to string.
    /// </summary>
    /// <returns>
    /// A <see cref="string" /> that represents this instance.
    /// </returns>
    public override string ToString()
    {
        return $"{Set} ({Features.Count})";
    }
}
