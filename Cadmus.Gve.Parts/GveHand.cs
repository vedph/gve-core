using Cadmus.Refs.Bricks;
using System.Collections.Generic;
using System.Text;

namespace Cadmus.Gve.Parts;

/// <summary>
/// A hand for <see cref="GveHandsPart"/>.
/// </summary>
public class GveHand
{
    /// <summary>
    /// The entity ID (EID) of the hand.
    /// </summary>
    public string? Eid { get; set; }

    /// <summary>
    /// The link to the hand's owner, when identified or not default. The default
    /// owner here is Goethe, so this is used only other persons.
    /// </summary>
    public AssertedCompositeId? OwnerId { get; set; }

    /// <summary>
    /// A generic tag which can be used to group hands by some criterion
    /// (e.g. "main", "secondary", etc.). Usually from thesaurus
    /// <c>gve-hand-tags</c>.
    /// </summary>
    public string? Tag { get; set; }

    /// <summary>
    /// The tool used by the hand (e.g. pen, pencil, etc.), usually from
    /// thesaurus <c>gve-hand-tools</c>.
    /// </summary>
    public string? Tool { get; set; }

    /// <summary>
    /// The color used by the hand (e.g. black, red, etc.), usually from
    /// thesaurus <c>gve-hand-colors</c>.
    /// </summary>
    public string? Color { get; set; }

    /// <summary>
    /// A dictionary of notes, keyed by string. This allow adding free-text
    /// notes to many different features, e.g.initials, corrections,
    /// abbreviations, single signs, etc. without forcing us into a preset
    /// structure.
    /// </summary>
    public Dictionary<string, string>? Notes { get; set; }

    /// <summary>
    /// Returns a string that represents the current object.
    /// </summary>
    /// <returns>A string that represents the current object.</returns>
    public override string ToString()
    {
        StringBuilder sb = new();
        if (!string.IsNullOrEmpty(Eid)) sb.Append('#').Append(Eid);
        if (OwnerId != null) sb.Append(": ").Append(OwnerId);
        return sb.ToString();
    }
}
