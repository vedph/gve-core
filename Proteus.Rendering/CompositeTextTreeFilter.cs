using Microsoft.Extensions.Logging;
using System;

namespace Proteus.Rendering;

/// <summary>
/// Base class for composite <see cref="ITextTreeFilter"/>'s.
/// </summary>
/// <seealso cref="IHasCompositeComponent" />
public abstract class CompositeTextTreeFilter : IHasCompositeComponent
{
    /// <summary>
    /// Gets the inner component.
    /// </summary>
    protected ITextTreeFilter? Component { get; private set; }

    /// <summary>
    /// Gets or sets the logger.
    /// </summary>
    public ILogger? Logger { get; set; }

    /// <summary>
    /// Sets the inner component.
    /// </summary>
    /// <param name="component">The inner component.</param>
    /// <exception cref="ArgumentNullException">inner</exception>
    /// <exception cref="InvalidOperationException">Type mismatch</exception>
    public void SetInnerComponent(object component)
    {
        ArgumentNullException.ThrowIfNull(component);

        Component = component as ITextTreeFilter ??
            throw new InvalidOperationException(
                "Inner component type mismatch for " +
                $"{nameof(CompositeTextTreeFilter)}: {component.GetType()}");
    }
}
