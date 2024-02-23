using System;

namespace Gve.Text;

/// <summary>
/// Basic implementation of <see cref="IFlatTreeNodeItem"/>.
/// </summary>
/// <seealso cref="IFlatTreeNodeItem" />
public class FlatTreeNodeItem : IFlatTreeNodeItem
{
    /// <summary>
    /// Gets the node identifier.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Gets the parent node identifier (null for root node).
    /// </summary>
    public string? ParentId { get; }

    /// <summary>
    /// Gets or sets the optional payload data.
    /// </summary>
    public object? Data { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="FlatTreeNodeItem"/> class.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="parentId">The parent identifier.</param>
    /// <param name="data">The data.</param>
    /// <exception cref="ArgumentNullException">id</exception>
    public FlatTreeNodeItem(string id, string? parentId, object? data)
    {
        ArgumentNullException.ThrowIfNull(Id);
        Id = id;
        ParentId = parentId;
        Data = data;
    }

    /// <summary>
    /// Converts to string.
    /// </summary>
    /// <returns>String.</returns>
    public override string ToString()
    {
        return $"[{Id}]" + (ParentId != null? $"<[{ParentId}]" : "") +
            (Data != null ? $": {Data}" : "");
    }
}
