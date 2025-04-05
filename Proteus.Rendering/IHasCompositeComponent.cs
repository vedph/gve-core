namespace Proteus.Rendering;

/// <summary>
/// Interface implemented by composite components which wrap an inner
/// component of the same type.
/// </summary>
public interface IHasCompositeComponent<T> where T : class
{
    /// <summary>
    /// Gets or sets the inner component.
    /// </summary>
    T? Component { get; set; }
}
