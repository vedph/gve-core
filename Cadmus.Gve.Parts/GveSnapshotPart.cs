using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cadmus.Core;
using Fusi.Tools.Configuration;
using Fusi.Tools.Text;
using Gve.Text.DTO;

namespace Cadmus.Gve.Parts;

/// <summary>
/// GVE snapshot part.
/// <para>Tag: <c>it.vedph.gve.snapshot</c>.</para>
/// </summary>
[Tag("it.vedph.gve.snapshot")]
public sealed class GveSnapshotPart : PartBase, IHasText
{
    /// <summary>
    /// Gets or sets the snapshot value.
    /// </summary>
    public Snapshot? Value { get; set; }

    /// <summary>
    /// Get all the key=value pairs (pins) exposed by the implementor.
    /// </summary>
    /// <param name="item">The optional item. The item with its parts
    /// can optionally be passed to this method for those parts requiring
    /// to access further data.</param>
    /// <returns>The pins: <c>node-count</c>=count of base text nodes,
    /// <c>op-count</c>=count of operations.</returns>
    public override IEnumerable<DataPin> GetDataPins(IItem? item = null)
    {
        DataPinBuilder builder = new();

        // node-count
        builder.Set("node", Value?.Text?.Count ?? 0, false);
        // ops-count
        builder.Set("op", Value?.Operations?.Count ?? 0, false);

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
             new DataPinDefinition(DataPinValueType.Integer,
                "node-count",
                "The total count of base text nodes."),
             new DataPinDefinition(DataPinValueType.Integer,
                "op-count",
                "The total count of operations.")
        ]);
    }

    /// <summary>
    /// Gets the base text of this snapshot.
    /// </summary>
    /// <returns>Text.</returns>
    public string GetText()
    {
        if (Value == null) return "";

        StringBuilder sb = new();

        foreach (CharChainNode node in Value.Text.OrderBy(n => n.Index))
            sb.Append(node.Data);

        return sb.ToString();
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

        sb.Append("[GveSnapshot] ").Append(TextCutter.Cut(GetText()));

        return sb.ToString();
    }
}
