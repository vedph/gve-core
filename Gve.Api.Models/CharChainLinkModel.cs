using Gve.Text;

namespace Gve.Api.Models;

/// <summary>
/// Model representing a link in a <see cref="Chain{T}"/> of characters.
/// Used by <see cref="CharChainModel"/>.
/// </summary>
public class CharChainLinkModel(ChainLink<char> link)
{
    /// <summary>
    /// The version tag.
    /// </summary>
    public string Tag { get; } = link.Tag;

    /// <summary>
    /// The source identifier or 0 for none.
    /// </summary>
    public int SourceId { get; } = link.Source?.Id ?? 0;

    /// <summary>
    /// The target identifier or 0 for none.
    /// </summary>
    public int TargetId { get; } = link.Target?.Id ?? 0;

    /// <summary>
    /// Converts to string.
    /// </summary>
    /// <returns>
    /// A <see cref="string" /> that represents this instance.
    /// </returns>
    override public string ToString()
    {
        return $"{Tag}: {SourceId} -> {TargetId}";
    }
}
