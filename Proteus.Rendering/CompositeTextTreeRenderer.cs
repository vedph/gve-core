using Microsoft.Extensions.Logging;

namespace Proteus.Rendering;

/// <summary>
/// Base class for composite <see cref="ITextTreeRenderer"/>'s.
/// </summary>
/// <seealso cref="IHasCompositeComponent{T}"/>
public abstract class CompositeTextTreeRenderer :
    IHasCompositeComponent<ITextTreeRenderer>
{
    /// <summary>
    /// Gets or sets the inner component.
    /// </summary>
    public ITextTreeRenderer? Component { get; set; }

    /// <summary>
    /// Gets or sets the logger.
    /// </summary>
    public ILogger? Logger { get; set; }
}
