using System.Collections.Generic;

namespace Gve.Text.DTO;

/// <summary>
/// A GVE snapshot DTO object containing all the data required to represent
/// a snapshot. All its objects are POCO objects without any logic.
/// </summary>
public class Snapshot
{
    /// <summary>
    /// Gets or sets the text nodes.
    /// </summary>
    public List<CharChainNode> Text { get; set; } = [];

    /// <summary>
    /// Gets or sets the snapshot operations.
    /// </summary>
    public IList<CharChainOperationSource> Operations { get; set; } = [];
}
