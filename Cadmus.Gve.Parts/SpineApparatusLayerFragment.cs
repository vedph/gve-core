using System;
using System.Collections.Generic;
using Fusi.Tools.Configuration;
using Cadmus.Core.Layers;
using Cadmus.Core;
using Gve.Text.Base;
using Gve.Text;

namespace Cadmus.Gve.Parts;

/// <summary>
/// GVE spine-based apparatus layer fragment. This is a spine-modeled apparatus
/// layer fragment including a base variant-generation base operation. In turn,
/// this operation can include a tree of variant-generation annotation
/// operations.
/// Tag: <c>fr.it.vedph.gve.spine-apparatus</c>.
/// </summary>
/// <seealso cref="ITextLayerFragment" />
[Tag("fr.it.vedph.gve.spine-apparatus")]
public sealed class SpineApparatusLayerFragment : ITextLayerFragment
{
    /// <summary>
    /// Gets or sets the location of this fragment.
    /// </summary>
    /// <remarks>
    /// The location can be expressed in different ways according to the
    /// text coordinates system being adopted. For instance, it might be a
    /// simple token-based coordinates system (e.g. 1.2=second token of
    /// first block), or a more complex system like an XPath expression.
    /// </remarks>
    public string Location { get; set; }

    /// <summary>
    /// Gets or sets the base variant-generation operation at the location
    /// specified by this fragment.
    /// </summary>
    public BaseVarOperation? Operation { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SpineApparatusLayerFragment">
    /// class.
    /// </summary>
    public SpineApparatusLayerFragment()
    {
        Location = "";
    }

    /// <summary>
    /// Get all the key=value pairs exposed by the implementor.
    /// </summary>
    /// <param name="item">The optional item. The item with its parts
    /// can optionally be passed to this method for those parts requiring
    /// to access further data.</param>
    /// <returns>The pins: <c>fr.tag</c>=tag if any.</returns>
    public IEnumerable<DataPin> GetDataPins(IItem? item = null)
    {
        DataPinBuilder builder = new (DataPinHelper.DefaultFilter);

        if (Operation != null)
        {
            // op type
            builder.AddValue(PartBase.FR_PREFIX + "op-type",
                Enum.GetName(Operation.Type));

            // op value
            builder.AddValue(PartBase.FR_PREFIX + "op-value",
                Operation.Value);

            // op tags
            if (Operation.TaggedOrdinals?.Count > 0)
            {
                builder.AddValues(PartBase.FR_PREFIX + "op-tag",
                    Operation.TaggedOrdinals.Keys);
            }

            // op group ID
            if (Operation.GroupId != null)
            {
                builder.AddValue(PartBase.FR_PREFIX + "op-group-id",
                    Operation.GroupId);
            }

            // op rank if different from 0
            if (Operation.Rank != 0)
            {
                builder.AddValue(PartBase.FR_PREFIX + "op-rank", Operation.Rank);
            }

            // op features
            if (Operation.Features?.Count > 0)
            {
                foreach (Feature feature in Operation.Features)
                {
                    builder.AddValue(PartBase.FR_PREFIX +
                        $"op-f-{feature.Name.ToLowerInvariant()}",
                        feature.Value);
                }
            }

            // op sources: type and ID for each source
            if (Operation.Sources?.Count > 0)
            {
                foreach (VarSource source in Operation.Sources)
                {
                    builder.AddValue(PartBase.FR_PREFIX +
                        $"op-source-{source.Type.ToLowerInvariant()}",
                        source.Id);
                }
            }
        }

        return builder.Build(null);
    }

    /// <summary>
    /// Gets the definitions of data pins used by the implementor.
    /// </summary>
    /// <returns>Data pins definitions.</returns>
    public IList<DataPinDefinition> GetDataPinDefinitions()
    {
        return new List<DataPinDefinition>(
        [
            new DataPinDefinition(DataPinValueType.String,
                PartBase.FR_PREFIX + "op-type", "The operation type."),
            new DataPinDefinition(DataPinValueType.String,
                PartBase.FR_PREFIX + "op-value", "The operation value."),
            new DataPinDefinition(DataPinValueType.String,
                PartBase.FR_PREFIX + "op-tag", "The operation tag.", "M"),
            new DataPinDefinition(DataPinValueType.String,
                PartBase.FR_PREFIX + "op-group-id", "The operation group ID."),
            new DataPinDefinition(DataPinValueType.Integer,
                PartBase.FR_PREFIX + "op-rank", "The operation rank."),
            new DataPinDefinition(DataPinValueType.String,
                PartBase.FR_PREFIX + "op-f-<feature>",
                "The operation feature value.", "M"),
            new DataPinDefinition(DataPinValueType.String,
                PartBase.FR_PREFIX + "op-source-<type>",
                "The operation source ID.", "M")
        ]);
    }

    /// <summary>
    /// Returns a <see cref="string" /> that represents this instance.
    /// </summary>
    /// <returns>
    /// A <see cref="string" /> that represents this instance.
    /// </returns>
    public override string ToString()
    {
        return Operation != null
            ? "[SpineApparatus] " + Operation
            : "[SpineApparatus]";
    }
}