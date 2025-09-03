using System;
using System.Collections.Generic;
using System.Text;
using Cadmus.Core;
using Fusi.Tools.Configuration;

namespace Cadmus.Gve.Parts;

/// <summary>
/// GVE hands part.
/// <para>Tag: <c>it.vedph.gve.hands</c>.</para>
/// </summary>
[Tag("it.vedph.gve.hands")]
public sealed class GveHandsPart : PartBase
{
    /// <summary>
    /// Gets or sets the hands.
    /// </summary>
    public List<GveHand> Hands { get; set; } = [];

    /// <summary>
    /// Get all the key=value pairs (pins) exposed by the implementor.
    /// </summary>
    /// <param name="item">The optional item. The item with its parts
    /// can optionally be passed to this method for those parts requiring
    /// to access further data.</param>
    /// <returns>The pins: <c>tot-count</c> and a collection of pins.</returns>
    public override IEnumerable<DataPin> GetDataPins(IItem? item = null)
    {
        DataPinBuilder builder = new();

        builder.Set("tot", Hands?.Count ?? 0, false);

        if (Hands?.Count > 0)
        {
            foreach (GveHand hand in Hands)
            {
                if (!string.IsNullOrEmpty(hand.Eid))
                    builder.AddValue("eid", hand.Eid);

                if (hand.OwnerId != null)
                    builder.AddValue("owner", hand.OwnerId.ToString());

                if (!string.IsNullOrEmpty(hand.Tag))
                    builder.AddValue("tag", hand.Tag);

                if (!string.IsNullOrEmpty(hand.Tool))
                    builder.AddValue("tool", hand.Tool);

                if (!string.IsNullOrEmpty(hand.Color))
                    builder.AddValue("color", hand.Color);
            }
        }

        return builder.Build(this);
    }

    /// <summary>
    /// Gets the definitions of data pins used by the implementor.
    /// </summary>
    /// <returns>Data pins definitions.</returns>
    public override IList<DataPinDefinition> GetDataPinDefinitions()
    {
        return new List<DataPinDefinition>(
        [
            new DataPinDefinition(DataPinValueType.String,
               "eid",
               "The entry ID(s) (EID).",
               "M"),
            new DataPinDefinition(DataPinValueType.String,
               "owner",
               "The owner ID(s).",
               "M"),
            new DataPinDefinition(DataPinValueType.String,
                "tag",
                "The tag(s).",
                "M"),
            new DataPinDefinition(DataPinValueType.String,
                "tool",
                "The tool(s).",
                "M"),
            new DataPinDefinition(DataPinValueType.String,
                "color",
                "The color(s).",
                "M"),
            new DataPinDefinition(DataPinValueType.Integer,
               "tot-count",
               "The total count of entries.")
        ]);
    }

    /// <summary>
    /// Converts to string.
    /// </summary>
    /// <returns>
    /// A <see cref="string" /> that represents this instance.
    /// </returns>
    public override string ToString()
    {
        StringBuilder sb = new();

        sb.Append("[GveHands]");

        if (Hands?.Count > 0)
        {
            sb.Append(' ');
            int n = 0;
            foreach (var entry in Hands)
            {
                if (++n > 3) break;
                if (n > 1) sb.Append("; ");
                sb.Append(entry);
            }
            if (Hands.Count > 3)
                sb.Append("...(").Append(Hands.Count).Append(')');
        }

        return sb.ToString();
    }
}
