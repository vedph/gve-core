using Cadmus.Core;
using Fusi.Tools.Configuration;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using Xunit;
using Gve.Text.Base;
using Gve.Text;
using System;

namespace Cadmus.Gve.Parts.Test;

public sealed class SpineApparatusLayerFragmentTest
{
    private static SpineApparatusLayerFragment GetFragment()
    {
        SpineApparatusLayerFragment fr = new()
        {
            Location = "1.23",
            Operation = new ReplaceBaseVarOperation
            {
                Id = VarOperation.GenerateId(),
                GroupId = "g",
                Rank = 1,
                At = 2,
                Run = 1,
                Value = "X",
                Diplomatic = new()
                {
                    G = "<g><rect width=\"10\" height=\"5\"></rect></g>",
                },
            }
        };
        fr.Operation.TaggedOrdinals["x"] = 1;
        fr.Operation.TaggedOrdinals["y"] = 2;

        fr.Operation.Features.Add(new Feature("color", "red", true));

        fr.Operation.Sources.Add(new()
        {
            Id = "A",
            Type = "ms",
            Rank = 1,
            Note = "A note",
        });

        return fr;
    }

    [Fact]
    public void Fragment_Has_Tag()
    {
        TagAttribute? attr = typeof(SpineApparatusLayerFragment).GetTypeInfo()
            .GetCustomAttribute<TagAttribute>();
        string? typeId = attr != null ? attr.Tag : GetType().FullName;
        Assert.NotNull(typeId);
        Assert.StartsWith(PartBase.FR_PREFIX, typeId);
    }

    [Fact]
    public void Fragment_Is_Serializable()
    {
        SpineApparatusLayerFragment fragment = GetFragment();

        string json = TestHelper.SerializeFragment(fragment);
        SpineApparatusLayerFragment? fragment2 =
            TestHelper.DeserializeFragment<SpineApparatusLayerFragment>(json);

        Assert.NotNull(fragment2);
        Assert.Equal(fragment.Location, fragment2.Location);

        Assert.NotNull(fragment2.Operation);
    }

    [Fact]
    public void GetDataPins_Ok()
    {
        SpineApparatusLayerFragment fragment = GetFragment();

        List<DataPin> pins = fragment.GetDataPins(null).ToList();

        Assert.Equal(8, pins.Count);

        // fr.op-type
        DataPin? pin = pins.Find(p => p.Name == PartBase.FR_PREFIX + "op-type"
            && p.Value == Enum.GetName(VarOperationType.Replace));
        Assert.NotNull(pin);

        // fr.op-value
        pin = pins.Find(p => p.Name == PartBase.FR_PREFIX + "op-value"
            && p.Value == "X");
        Assert.NotNull(pin);

        // fr.op-tag
        pin = pins.Find(p => p.Name == PartBase.FR_PREFIX + "op-tag"
                   && p.Value == "x");
        Assert.NotNull(pin);
        pin = pins.Find(p => p.Name == PartBase.FR_PREFIX + "op-tag"
                          && p.Value == "y");
        Assert.NotNull(pin);

        // fr.op-group-id
        pin = pins.Find(p => p.Name == PartBase.FR_PREFIX + "op-group-id"
            && p.Value == "g");
        Assert.NotNull(pin);

        // fr.op-rank
        pin = pins.Find(p => p.Name == PartBase.FR_PREFIX + "op-rank"
            && p.Value == "1");
        Assert.NotNull(pin);

        // fr.op-f-color
        pin = pins.Find(p => p.Name == PartBase.FR_PREFIX + "op-f-color"
            && p.Value == "red");
        Assert.NotNull(pin);

        // fr.op-source-ms
        pin = pins.Find(p => p.Name == PartBase.FR_PREFIX + "op-source-ms"
            && p.Value == "A");
        Assert.NotNull(pin);
    }
}