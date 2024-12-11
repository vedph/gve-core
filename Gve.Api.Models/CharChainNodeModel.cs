using Gve.Text;

namespace Gve.Api.Models;

/// <summary>
/// Model representing a node in a <see cref="Chain{T}"/> of characters.
/// Used by <see cref="CharChainModel"/>.
/// </summary>
public class CharChainNodeModel(ChainNode<char> node)
{
    /// <summary>
    /// The node's identifier.
    /// </summary>
    public int Id { get; } = node.Id;

    /// <summary>
    /// The node's label.
    /// </summary>
    public string Label { get; } = node.Label;

    /// <summary>
    /// The node's character.
    /// </summary>
    public char Data { get; } = node.Data;

    /// <summary>
    /// The node's source version tag.
    /// </summary>
    public string SourceTag { get; } = node.SourceTag;

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
