namespace Proteus.Rendering;

/// <summary>
/// Interface implemented by composite components which wrap an inner
/// component of the same type.
/// </summary>
public interface IHasCompositeComponent
{
    /// <summary>
    /// Sets the inner component.
    /// </summary>
    /// <param name="component">The inner component.</param>
    void SetInnerComponent(object component);
}
