namespace Gve.Text;

/// <summary>
/// A node in a <see cref="Chain{T}"/> data structure.
/// </summary>
/// <typeparam name="T">The node data type.</typeparam>
public class ChainNode<T>
{
    /// <summary>
    /// Gets or sets the node identifier. This must be unique in the scope
    /// of the <see cref="Chain{T}"/> this node belongs to.
    /// </summary>
    public string Id { get; set; } = "";

    /// <summary>
    /// Gets or sets the ordinal number assigned to this node when it belonged
    /// to an original sequence.
    /// </summary>
    public int Ordinal { get; set; }

    /// <summary>
    /// Gets or sets the node label.
    /// </summary>
    public string Label { get; set; } = "";

    /// <summary>
    /// Gets or sets the data attached to this node.
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// Converts to string.
    /// </summary>
    /// <returns>
    /// A <see cref="string" /> that represents this instance.
    /// </returns>
    public override string ToString()
    {
        return $"#{Id} {Label}";
    }
}
