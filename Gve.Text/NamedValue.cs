using System;
using System.Linq;
using System.Text;

namespace Gve.Text;

/// <summary>
/// A named string value.
/// </summary>
public record NamedValue
{
    /// <summary>
    /// Gets the name.
    /// </summary>
    public string Name { get; init; } = "";

    /// <summary>
    /// Gets the value. Usually there is a value, but in some cases a feature
    /// is used just as a flag, in which case the value is null.
    /// </summary>
    public string? Value { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="NamedValue"/> class.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="value">The value.</param>
    /// <exception cref="ArgumentNullException">name</exception>
    public NamedValue(string name, string? value)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Value = value;
    }

    /// <summary>
    /// Converts to a parsable string with format <c>NAME=VALUE</c> where
    /// operator is <c>=</c> for multiple, <c>:=</c> for single, and
    /// <c>==</c> for single-first.
    /// Value is enclosed in double quotes if it contains white spaces.
    /// </summary>
    /// <returns>
    /// A <see cref="string" /> that represents this instance.
    /// </returns>
    public override string ToString()
    {
        StringBuilder sb = new();
        sb.Append(Name);

        if (Value != null)
        {
            sb.Append('=');

            if (Value.Any(char.IsWhiteSpace))
                sb.Append('"').Append(Value).Append('"');
            else
                sb.Append(Value);
        }

        return sb.ToString();
    }
}
