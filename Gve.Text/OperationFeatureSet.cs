using System;
using System.Collections.Generic;
using System.Linq;

namespace Gve.Text;

/// <summary>
/// A versioned set of <see cref="OperationFeature"/>'s, keyed by version tag.
/// Each entry in this set holds the features that apply at a specific version.
/// </summary>
public class OperationFeatureSet
{
    private readonly Dictionary<string, List<OperationFeature>> _byTag = [];

    /// <summary>
    /// Gets the features associated with the specified version tag, or an
    /// empty list when no entry exists for it.
    /// </summary>
    /// <param name="tag">The version tag.</param>
    /// <returns>Features (never null).</returns>
    /// <exception cref="ArgumentNullException">tag</exception>
    public IList<OperationFeature> Get(string tag)
    {
        ArgumentNullException.ThrowIfNull(tag);
        return _byTag.TryGetValue(tag, out List<OperationFeature>? list)
            ? list
            : [];
    }

    /// <summary>
    /// Returns true when this set has an entry for the specified version tag.
    /// </summary>
    /// <param name="tag">The version tag.</param>
    /// <returns>True if entry exists.</returns>
    public bool HasTag(string tag) => _byTag.ContainsKey(tag);

    /// <summary>
    /// Gets all version tags present in this set.
    /// </summary>
    public IEnumerable<string> Tags => _byTag.Keys;

    /// <summary>
    /// Sets (or replaces) the feature list for the specified version tag.
    /// An empty source produces an empty entry, which is still recorded so
    /// that <see cref="HasTag"/> returns true for that tag.
    /// </summary>
    /// <param name="tag">The version tag.</param>
    /// <param name="features">The features to store.</param>
    /// <exception cref="ArgumentNullException">tag or features</exception>
    public void Set(string tag, IEnumerable<OperationFeature> features)
    {
        ArgumentNullException.ThrowIfNull(tag);
        ArgumentNullException.ThrowIfNull(features);
        _byTag[tag] = [.. features];
    }

    /// <summary>
    /// Removes all entries from this set.
    /// </summary>
    public void Clear() => _byTag.Clear();

    /// <summary>
    /// Returns a snapshot of the internal dictionary as a read-only dictionary
    /// of lists (shallow copies of the lists).
    /// </summary>
    public IDictionary<string, IList<OperationFeature>> ToDictionary()
    {
        return _byTag.ToDictionary(
            kvp => kvp.Key,
            kvp => (IList<OperationFeature>)[.. kvp.Value]);
    }
}
