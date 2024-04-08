namespace Gve.Text;

/// <summary>
/// Interface implemented by operations that have a TO argument.
/// </summary>
public interface IHasToArgument
{
    /// <summary>
    /// Gets or sets a value indicating whether <see cref="To"/> refers to
    /// an index rather than a chain node ID.
    /// </summary>
    bool ToAsIndex { get; set; }

    /// <summary>
    /// Gets or sets the reference position.
    /// </summary>
    int To { get; set; }
}
