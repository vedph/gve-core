using Gve.Text;
using Proteus.Rendering;
using System.Collections.Generic;

namespace Gve.Migration;

/// <summary>
/// Augmented context for GVE operations.
/// </summary>
/// <seealso cref="RendererContext" />
public class GveRendererContext : RendererContext
{
    /// <summary>
    /// Gets or sets the GVE operations execution context.
    /// </summary>
    public ChainOperationContext<char>? OpContext { get; set; }

    /// <summary>
    /// Gets or sets the operations executed on the GVE chain.
    /// </summary>
    public List<CharChainOperation>? Operations { get; set; }

    /// <summary>
    /// Clears this context.
    /// </summary>
    /// <param name="seeds">if set to <c>true</c> also reset the ID maps
    /// seeds.</param>
    public override void Clear(bool seeds = false)
    {
        base.Clear(seeds);

        OpContext = null;
        Operations = null;
    }
}
