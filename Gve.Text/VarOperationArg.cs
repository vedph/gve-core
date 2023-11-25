using System;
using System.Text;
using System.Text.RegularExpressions;

namespace Gve.Text;

/// <summary>
/// An argument of a text variant operation.
/// </summary>
public class VarOperationArg
{
    public const string NAME_VALUE = "value";

    /// <summary>
    /// Gets or sets the name (default is <c>value</c>).
    /// </summary>
    public string Name { get; set; } = NAME_VALUE;

    /// <summary>
    /// Gets or sets the argument value.
    /// </summary>
    public string Value { get; set; } = "";

    /// <summary>
    /// Gets or sets the probability rank (0=unspecified, 1=most probable, etc.).
    /// </summary>
    public short Rank { get; set; }

    /// <summary>
    /// Gets or sets an optional note.
    /// </summary>
    public string? Note { get; set; }

    /// <summary>
    /// Converts to string.
    /// </summary>
    /// <returns>
    /// A <see cref="string" /> that represents this instance.
    /// </returns>
    public override string ToString()
    {
        StringBuilder sb = new(Name);
        sb.Append("=\"").Append(Value).Append('"');
        if (Rank != 0) sb.Append('^').Append(Rank);

        return sb.ToString();
    }

    /// <summary>
    /// Parses the specified text.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <returns>Argument.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="FormatException">Invalid var operation argument</exception>
    public static VarOperationArg Parse(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        Regex regex = new Regex(@"^(?<n>[^=]+)=""(?<v>[^""]+)""(?:\^(?<r>\d+))?$",
            RegexOptions.Compiled);
        Match match = regex.Match(text);
        if (!match.Success)
            throw new FormatException($"Invalid var operation argument: {text}");

        return new VarOperationArg
        {
            Name = match.Groups["n"].Value,
            Value = match.Groups["v"].Value,
            Rank = match.Groups["r"].Success
                ? short.Parse(match.Groups[2].Value)
                : (short)0,
        };
    }

    public static bool TryParse(string text, out VarOperationArg? arg)
    {
        ArgumentNullException.ThrowIfNull(text);

        try
        {
            arg = Parse(text);
            return true;
        }
        catch (FormatException)
        {
            arg = null;
            return false;
        }
    }
}
