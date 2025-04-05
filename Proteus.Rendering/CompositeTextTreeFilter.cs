using Microsoft.Extensions.Logging;

namespace Proteus.Rendering;

/// <summary>
/// Base class for composite <see cref="ITextTreeFilter"/>'s.
/// </summary>
/// <seealso cref="IHasCompositeComponent{T}" />
public abstract class CompositeTextTreeFilter :
    IHasCompositeComponent<ITextTreeFilter>
{
    /// <summary>
    /// Gets the inner component.
    /// </summary>
    public ITextTreeFilter? Component { get; set; }

    /// <summary>
    /// Gets or sets the logger.
    /// </summary>
    public ILogger? Logger { get; set; }
}
