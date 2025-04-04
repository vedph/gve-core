using Fusi.Tools.Data;
using Proteus.Rendering;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Gve.Migration;

/// <summary>
/// Tree node payload tagger for <see cref="ExportedSegment"/>.
/// This is used by node version mergers as an adapter for dealing with
/// payload data including a text and any number of tags.
/// </summary>
public class SegmentTreeNodePayloadTagger : ITreeNodePayloadTagger<ExportedSegment>
{
    /// <summary>
    /// Adds <paramref name="tag" /> to the specified payload data.
    /// </summary>
    /// <param name="data">The data to add the tag to.</param>
    /// <param name="tag">The tag to add.</param>
    /// <exception cref="ArgumentNullException">data or tag</exception>
    public void AddTag(ExportedSegment data, string tag)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(tag);

        data.Tags ??= [];
        data.Tags.Add(tag);
    }

    /// <summary>
    /// Clears the tags from the specified data.
    /// </summary>
    /// <param name="data">The data.</param>
    /// <exception cref="ArgumentNullException">data</exception>
    public void ClearTags(ExportedSegment data)
    {
        ArgumentNullException.ThrowIfNull(data);
        data.Tags = null;
    }

    /// <summary>
    /// Clones the payload.
    /// </summary>
    /// <param name="data">The data or null.</param>
    /// <returns>Cloned data or null.</returns>
    [return: NotNullIfNotNull(nameof(data))]
    public ExportedSegment? ClonePayload(ExportedSegment? data)
    {
        return data?.Clone();
    }

    /// <summary>
    /// Gets the tag(s) from the specified payload.
    /// </summary>
    /// <param name="data">The payload data.</param>
    /// <returns>Tags.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public IList<string> GetTags(ExportedSegment data)
    {
        ArgumentNullException.ThrowIfNull(data);
        return data.Tags == null ? [] : data.Tags.ToList();
    }

    /// <summary>
    /// Determines whether the specified payload data has a comparable value
    /// for the purpose of <see cref="MatchPayloadValues" />.
    /// </summary>
    /// <param name="data">The data.</param>
    /// <returns>
    /// <c>true</c> if has payload value; otherwise, <c>false</c>.
    /// </returns>
    /// <exception cref="ArgumentNullException">data</exception>
    public bool HasPayloadValue(ExportedSegment data)
    {
        ArgumentNullException.ThrowIfNull(data);
        return data.Text != null;
    }

    /// <summary>
    /// Determines whether the specified data has the specified tag.
    /// </summary>
    /// <param name="data">The data.</param>
    /// <param name="tag">The tag.</param>
    /// <returns>
    /// <c>true</c> if the specified data has tag; otherwise,
    /// <c>false</c>.
    /// </returns>
    /// <exception cref="ArgumentNullException">data</exception>
    public bool HasTag(ExportedSegment data, string tag)
    {
        ArgumentNullException.ThrowIfNull(data);
        return data.Tags?.Contains(tag) == true;
    }

    /// <summary>
    /// Matches the payload values.
    /// </summary>
    /// <param name="tag">The current tag. This can be used when payload values
    /// depend on tag. Not used in this implementation.</param>
    /// <param name="a">The first payload.</param>
    /// <param name="b">The second payload.</param>
    /// <returns>
    /// True if payload values match.
    /// </returns>
    public bool MatchPayloadValues(string tag, ExportedSegment a,
        ExportedSegment b)
    {
        return a.Text == b.Text;
    }
}
