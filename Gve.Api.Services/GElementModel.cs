using Gve.Text;
using System.Collections.Generic;

namespace Gve.Api.Services;

/// <summary>
/// Model for a <c>g</c> element in an SVG document.
/// </summary>
public class GElementModel
{
    /// <summary>
    /// Gets or sets the element's identifier.
    /// </summary>
    public string Id { get; set; } = "";

    /// <summary>
    /// Gets or sets the parent element's identifier.
    /// </summary>
    public string? ParentId { get; set; }

    /// <summary>
    /// Gets or sets the element's features.
    /// </summary>
    public IList<Feature>? Features { get; set; }

    /// <summary>
    /// Converts to string.
    /// </summary>
    /// <returns>
    /// A <see cref="string" /> that represents this instance.
    /// </returns>
    public override string ToString()
    {
        return $"{Id} F={Features?.Count ?? 0}";
    }
}
