using System;
using System.Linq;

namespace Gve.Text;

/// <summary>
/// A generic name=value (or just named) feature.
/// </summary>
public record Feature
{
    /// <summary>
    /// Gets the name.
    /// </summary>
    public string Name { get; init; }

    /// <summary>
    /// Gets the value. Usually there is a value, but in some cases a feature
    /// is used just as a flag, in which case the value is null.
    /// </summary>
    public string? Value { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Feature"/> class.
    /// </summary>
    public Feature()
    {
        Name = "";
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Feature"/> class.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="value">The value.</param>
    /// <exception cref="ArgumentNullException">name</exception>
    public Feature(string name, string? value)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Value = value;
    }

    /// <summary>
    /// Converts to string.
    /// </summary>
    /// <returns>
    /// A <see cref="string" /> that represents this instance.
    /// </returns>
    public override string ToString()
    {
        if (Value == null) return Name;

        return Value.Any(char.IsWhiteSpace)
            ? $"{Name}=\"{Value}\""
            : $"{Name}={Value}";
    }
}
