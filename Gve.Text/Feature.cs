using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Gve.Text;

/// <summary>
/// A generic name=value feature.
/// </summary>
public partial class Feature
{
    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the value.
    /// </summary>
    public string Value { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the feature is unique in a set
    /// i.e. if it already exists in a set, it should be replaced rather than
    /// just added.
    /// </summary>
    /// <value>
    ///   <c>true</c> if this annotation is unique; otherwise, <c>false</c>.
    /// </value>
    public bool IsUnique { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Feature"/> class.
    /// </summary>
    public Feature()
    {
        Name = "";
        Value = "";
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Feature"/> class.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="value">The value.</param>
    /// <param name="isUnique"><c>true</c> if feature is unique.</param>
    /// <exception cref="ArgumentNullException">name or value</exception>
    public Feature(string name, string value, bool isUnique = false)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Value = value ?? throw new ArgumentNullException(nameof(value));
        IsUnique = isUnique;
    }

    [GeneratedRegex(@"^(?<n>[^=]+)(?<u>:)=(?<v>(?:""[^""]+"")|(?:[^\s]+))$")]
    private static partial Regex NameValueRegex();

    /// <summary>
    /// Parses the specified text representing a feature, with syntax
    /// <c>name=value or name="value"</c>, where <c>=</c> can be replaced by
    /// <c>:=</c> for a unique feature.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <returns>Feature.</returns>
    /// <exception cref="ArgumentNullException">text</exception>
    /// <exception cref="FormatException">Invalid feature</exception>
    public static Feature Parse(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        Match m = NameValueRegex().Match(text);
        if (!m.Success) throw new FormatException($"Invalid feature: {text}");

        return new()
        {
            Name = m.Groups["n"].Value,
            Value = m.Groups["v"].Value.Trim('"'),
            IsUnique = m.Groups["u"].Success
        };
    }

    /// <summary>
    /// Tries parsing the specified feature.
    /// </summary>
    /// <param name="text">The text representing the feature.</param>
    /// <param name="feature">The feature.</param>
    /// <returns>True if success.</returns>
    /// <exception cref="ArgumentNullException">text</exception>
    public static bool TryParse(string text, out Feature? feature)
    {
        ArgumentNullException.ThrowIfNull(text);

        feature = null;
        try
        {
            feature = Parse(text);
            return true;
        }
        catch (FormatException)
        {
            return false;
        }
    }

    /// <summary>
    /// Converts to string.
    /// </summary>
    /// <returns>
    /// A <see cref="string" /> that represents this instance.
    /// </returns>
    public override string ToString()
    {
        string op = IsUnique? ":=" : "=";
        return Value.Any(c => char.IsWhiteSpace(c))
            ? Name + op + Value
            : $"{Name}{op}\"{Value}\"";
    }
}
