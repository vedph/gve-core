using System;
using System.Collections.Generic;

namespace Gve.Text;

/// <summary>
/// A text with name=value annotations.
/// </summary>
public record AnnotatedText
{
    /// <summary>
    /// The key used for generic notes in annotations.
    /// </summary>
    public const string NOTE_KEY = "note";

    /// <summary>
    /// Gets the text value.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Gets the annotations.
    /// </summary>
    public IDictionary<string, string> Annotations { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AnnotatedText"/> class.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="annotations">The annotations.</param>
    /// <exception cref="ArgumentNullException">value</exception>
    public AnnotatedText(string value,
        IDictionary<string, string>? annotations = null)
    {
        Value = value ?? throw new ArgumentNullException(nameof(value));
        Annotations = new Dictionary<string, string>();
        if (annotations != null)
        {
            foreach (KeyValuePair<string, string> kvp in annotations)
                Annotations.Add(kvp.Key, kvp.Value);
        }
    }
}
