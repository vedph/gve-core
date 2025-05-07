using Fusi.Tools.Text;
using System.Text;

namespace Gve.Demo.Services;

/// <summary>
/// Headered texts reader.
/// </summary>
public sealed class HeaderedTextReader
{
    /// <summary>
    /// The text header's prefix. Default is <c>:</c>. Any text section in the
    /// source starts after a header whose line starts with this prefix.
    /// </summary>
    public string HeaderPrefix { get; set; } = ":";

    /// <summary>
    /// Reads the headered text from the specified <see cref="StreamReader"/>.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <returns>Headered texts.</returns>
    public IEnumerable<HeaderedText> Read(StreamReader reader)
    {
        ArgumentNullException.ThrowIfNull(reader);

        string? header = null;
        string? line;
        StringBuilder text = new();

        while ((line = reader.ReadLine()) != null)
        {
            line = line.Trim();
            if (line.Length == 0) continue;

            // check if the line starts with the header prefix
            if (line.StartsWith(HeaderPrefix, StringComparison.Ordinal))
            {
                // return the previous headered text if any
                if (header != null)
                    yield return new HeaderedText(header, text.ToString());

                // extract the header and reset text
                header = line[HeaderPrefix.Length..];
                text.Clear();
            }
            else
            {
                // append the line to the text
                text.AppendLine(line);
            }
        }

        // return the last headered text if any
        if (header != null)
            yield return new HeaderedText(header, text.ToString());
    }
}

/// <summary>
/// A headered text.
/// </summary>
/// <param name="header">Header.</param>
/// <param name="text">Text.</param>
public class HeaderedText(string header, string text)
{
    /// <summary>
    /// Header.
    /// </summary>
    public string Header { get; } = header;

    /// <summary>
    /// Text.
    /// </summary>
    public string Text { get; } = text;

    /// <summary>
    /// Converts the headered text to a string.
    /// </summary>
    /// <returns>String.</returns>
    public override string ToString()
    {
        return $"{Header}: {TextCutter.Cut(Text)}";
    }
}
