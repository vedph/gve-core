namespace Gve.Text;

/// <summary>
/// A node in a <see cref="Chain{T}"/> data structure.
/// </summary>
/// <typeparam name="T">The node data type.</typeparam>
public class ChainNode<T>
{
    /// <summary>
    /// Gets or sets the autonumber assigned to this node in the context of
    /// its chain.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the node label.
    /// </summary>
    public string Label { get; set; } = "";

    /// <summary>
    /// Gets or sets the data attached to this node.
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// Gets or sets the source tag for this node, i.e. the tag of the version
    /// which first added it to the chain.
    /// </summary>
    public string SourceTag { get; set; } = "";

    /// <summary>
    /// Initializes a new instance of the <see cref="ChainNode{T}"/> class.
    /// </summary>
    public ChainNode()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ChainNode{T}"/> class.
    /// </summary>
    /// <param name="data">The data.</param>
    /// <param name="label">The label.</param>
    /// <param name="id">The identifier.</param>
    public ChainNode(T? data, string? label = null, int id = 0)
    {
        Data = data;
        Label = label ?? data?.ToString() ?? "";
        Id = id;
    }

    /// <summary>
    /// Converts to string.
    /// </summary>
    /// <returns>
    /// A <see cref="string" /> that represents this instance.
    /// </returns>
    public override string ToString()
    {
        return $"#{Id}={Label}";
    }
}
