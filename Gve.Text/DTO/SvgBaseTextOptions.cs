using System.Collections.Generic;

namespace Gve.Text.DTO;

/// <summary>
/// SVG base text options for a <see cref="Snapshot"/>.
/// </summary>
public class SvgBaseTextOptions
{
    /// <summary>
    /// Gets or sets the line height offset to add to the calculated line
    /// height.
    /// </summary>
    public double LineHeightOffset { get; set; }

    /// <summary>
    /// Gets or sets the character spacing offset to add between each pair
    /// of characters.
    /// </summary>
    public double CharSpacingOffset { get; set; }

    /// <summary>
    /// Gets or sets the space width offset to add to the calculated width
    /// of each space character.
    /// </summary>
    public double SpcWidthOffset { get; set; }

    /// <summary>
    /// Gets or sets the offset of the top-left origin of the base text.
    /// </summary>
    public Point? Offset { get; set; }

    /// <summary>
    /// Gets or sets the minimum line heights for each line number (1-N).
    /// You can specify a minimum line height for each line number to ensure
    /// that its height is at least the specified value.This is useful for
    /// making room between lines for annotations that may exceed the base text
    /// height.
    /// </summary>
    public Dictionary<short, double>? MinLineHeights { get; set; }
}
