using Gve.Text;
using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Gve.Api.Services;

/// <summary>
/// The model of a variant's diplomatic information.
/// </summary>
public class VarDiplomaticModel
{
    /// <summary>
    /// Gets or sets the SVG <c>g</c> element graphically representing the
    /// operation this diplomatic information is attached to.
    /// </summary>
    public string? G { get; set; }

    /// <summary>
    /// Gets or sets the list of SVG elements in <see cref="G"/> having an
    /// ID attribute. The list is ordered as a flat tree hierarchy,
    /// with the root element first, and the children elements following
    /// so that each parent element is immediately followed by its children.
    /// </summary>
    public IList<GElementModel>? GElements { get; set; }

    /// <summary>
    /// Gets or sets the optional generic set of features attached to the text
    /// variant as a whole (e.g. type, size, etc.).
    /// </summary>
    public IList<Feature>? Features { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="VarDiplomaticModel"/> class.
    /// </summary>
    public VarDiplomaticModel()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="VarDiplomaticModel"/> class.
    /// </summary>
    /// <param name="d">The source diplomatic info.</param>
    public VarDiplomaticModel(OperationDiplomatics d)
    {
        ArgumentNullException.ThrowIfNull(d);

        G = d.G;
        Features = d.Features;

        // parse G as XML and extract the list of elements from it,
        // in the order they appear in the XML, and with their id attributes
        // and parent IDs
        if (!string.IsNullOrEmpty(G))
        {
            GElements = new List<GElementModel>();
            XElement g = XElement.Parse(G);
            foreach (XElement e in g.DescendantsAndSelf())
            {
                string? id = e.Attribute("id")?.Value;
                if (id != null)
                {
                    string? parentId = e.Parent?.Attribute("id")?.Value;
                    GElementModel em = new()
                    {
                        Id = id,
                        ParentId = parentId,
                    };
                    // if ElementFeatures has a key=id assign features to em
                    if (d.ElementFeatures != null &&
                        d.ElementFeatures.TryGetValue(id, out List<Feature>? features))
                    {
                        em.Features = features;
                    }

                    GElements.Add(em);
                }
            }
        }
    }
}
