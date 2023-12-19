using System.Collections.Generic;

namespace Gve.Text;

/// <summary>
/// Diplomatic data for a text variant.
/// </summary>
public class VarDiplomatic
{
    /// <summary>
    /// Gets or sets the position.
    /// </summary>
    public TextPosition? Position { get; set; }

    /// <summary>
    /// Gets or sets the optional text representing the variant. This is the
    /// original text found attached to the variant, optionally multiline.
    /// </summary>
    public string? Text { get; set; }

    /// <summary>
    /// Gets or sets the features, keyed by type (e.g. <c>style</c>,
    /// <c>class</c>, <c>shape</c>, etc.). Each value is a dictionary where
    /// the key is the version tag, and the value a list of features.
    /// </summary>
    public Dictionary<string, Dictionary<string, IList<Feature>>>
        Features { get; set; } = [];
}
