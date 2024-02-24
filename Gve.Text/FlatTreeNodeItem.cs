using System;

namespace Gve.Text;

/// <summary>
/// Basic implementation of <see cref="IFlatTreeNodeItem{T}"/>.
/// </summary>
/// <seealso cref="IFlatTreeNodeItem{T}" />
public class FlatTreeNodeItem<T> : IFlatTreeNodeItem<T>
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
    public T? Data { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="FlatTreeNodeItem{T}"/> class.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="parentId">The parent identifier.</param>
    /// <param name="data">The data.</param>
    /// <exception cref="ArgumentNullException">id</exception>
    public FlatTreeNodeItem(string id, string? parentId, T? data)
    {
        ArgumentNullException.ThrowIfNull(id);
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
