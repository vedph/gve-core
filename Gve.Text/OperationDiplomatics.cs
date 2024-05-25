using System.Collections.Generic;
using System.Text;

namespace Gve.Text;

/// <summary>
/// Diplomatic data for a text variant. This defines the visual representation
/// of the variant, as well as the features attached to it.
/// </summary>
public class OperationDiplomatics
{
    /// <summary>
    /// Gets or sets the SVG <c>g</c> element graphically representing the
    /// operation this diplomatic information is attached to.
    /// </summary>
    public string? G { get; set; }

    /// <summary>
    /// Gets or sets the optional generic set of features attached to the text
    /// variant as a whole (e.g. type, size, etc.).
    /// </summary>
    public IList<Feature>? Features { get; set; }

    /// <summary>
    /// Gets or sets the optional features attached to elements defined in
    /// <see cref="G"/>. Each element is identified by its <c>id</c> attribute,
    /// which is a key in the dictionary.
    /// </summary>
    public IDictionary<string, List<Feature>>? ElementFeatures { get; set; }

    /// <summary>
    /// Converts to string.
    /// </summary>
    /// <returns>
    /// A <see cref="string" /> that represents this instance.
    /// </returns>
    public override string ToString()
    {
        StringBuilder sb = new();

        if (!string.IsNullOrEmpty(G))
            sb.Append(G.Length > 50 ? G[..50] + "..." : G);
        if (Features != null)
            sb.Append(" F=").Append(Features.Count);
        if (ElementFeatures != null)
            sb.Append(" EF=").Append(ElementFeatures.Count);

        return sb.ToString();
    }
}
