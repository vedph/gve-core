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
    /// which added it to the chain. This is required to supply the version tag
    /// when the node has no links because it is the only one in the chain.
    /// When instead it has no links but nodes are many, this just means that
    /// the node is disconnected because it has been deleted within the
    /// boundaries of the same version.
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
        return $"#{Id}={Label} ({SourceTag})";
    }
}
