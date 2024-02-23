namespace Gve.Text;

/// <summary>
/// A flat tree node item to use with <see cref="TreeNode"/>.
/// </summary>
public interface IFlatTreeNodeItem
{
    /// <summary>
    /// Gets the identifier.
    /// </summary>
    string Id { get; }

    /// <summary>
    /// Gets the parent identifier.
    /// </summary>
    string? ParentId { get; }

    /// <summary>
    /// Gets the data.
    /// </summary>
    object? Data { get; }
}
