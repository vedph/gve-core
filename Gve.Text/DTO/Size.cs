namespace Gve.Text.DTO;

/// <summary>
/// A 2D size.
/// </summary>
public class Size
{
    /// <summary>
    /// Gets or sets the width.
    /// </summary>
    public double Width { get; set; }

    /// <summary>
    /// Gets or sets the height.
    /// </summary>
    public double Height { get; set; }

    /// <summary>
    /// Converts to string.
    /// </summary>
    /// <returns>
    /// A <see cref="string" /> that represents this instance.
    /// </returns>
    override public string ToString()
    {
        return $"{Width}×{Height}";
    }
}
