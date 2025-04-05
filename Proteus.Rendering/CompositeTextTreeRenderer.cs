using Microsoft.Extensions.Logging;

namespace Proteus.Rendering;

/// <summary>
/// Base class for composite <see cref="ITextTreeRenderer{T}"/>'s.
/// </summary>
/// <seealso cref="IHasCompositeComponent{T}"/>
public abstract class CompositeTextTreeRenderer<THandledType> :
    TextTreeRenderer<THandledType>,
    IHasCompositeComponent<ITextTreeRenderer<THandledType>>
{
    /// <summary>
    /// Gets or sets the inner component.
    /// </summary>
    public ITextTreeRenderer<THandledType>? Component { get; set; }
}
