using System;
using System.Collections.Generic;

namespace Gve.Text;

public record AnnotatedText
{
    /// <summary>
    /// The key used for generic notes in annotations.
    /// </summary>
    public const string NOTE_KEY = "note";

    public string Value { get; }
    public IDictionary<string, string> Annotations { get; }

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
