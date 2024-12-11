using System;
using System.Linq;
using Gve.Text;

namespace Gve.Api.Models;

/// <summary>
/// Model representing a <see cref="Chain{T}"/> of characters.
/// </summary>
public class CharChainModel
{
    /// <summary>
    /// The nodes in the chain.
    /// </summary>
    public CharChainNodeModel[] Nodes { get; }

    /// <summary>
    /// The links in the chain.
    /// </summary>
    public CharChainLinkModel[] Links { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CharChainModel"/> class.
    /// </summary>
    /// <param name="chain">The chain.</param>
    /// <exception cref="ArgumentNullException">chain</exception>
    public CharChainModel(Chain<char> chain)
    {
        ArgumentNullException.ThrowIfNull(chain);
        Nodes = chain.Nodes.Select(n => new CharChainNodeModel(n)).ToArray();
        Links = chain.Links.Select(l => new CharChainLinkModel(l)).ToArray();
    }
}
