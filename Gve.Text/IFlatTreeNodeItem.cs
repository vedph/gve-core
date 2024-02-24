namespace Gve.Text;

/// <summary>
/// A flat tree node item to use with <see cref="TreeNode{T}"/>.
/// </summary>
/// <typeparam name="T">The type of the data payload.</typeparam>
public interface IFlatTreeNodeItem<out T>
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
    T? Data { get; }
}
