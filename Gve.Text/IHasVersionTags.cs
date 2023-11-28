using System.Collections.Generic;

namespace Gve.Text;

/// <summary>
/// Interface for objects having version tags.
/// </summary>
public interface IHasVersionTags
{
    /// <summary>
    /// Gets or sets the version tags.
    /// </summary>
    HashSet<string> VersionTags { get; set; }
}
