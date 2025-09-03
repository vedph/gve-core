using Cadmus.Core;
using Cadmus.Seed.Gve.Parts;
using System;
using System.Collections.Generic;
using Xunit;

namespace Cadmus.Gve.Parts.Test;

public sealed class GveHandsPartTest
{
    private static GveHandsPart GetPart()
    {
        GveHandsPartSeeder seeder = new();
        IItem item = new Item
        {
            FacetId = "default",
            CreatorId = "zeus",
            UserId = "zeus",
            Description = "Test item",
            Title = "Test Item",
            SortKey = ""
        };
        return (GveHandsPart)seeder.GetPart(item, null, null)!;
    }

    private static GveHandsPart GetEmptyPart()
    {
        return new GveHandsPart
        {
            ItemId = Guid.NewGuid().ToString(),
            RoleId = "some-role",
            CreatorId = "zeus",
            UserId = "another",
        };
    }

    [Fact]
    public void Part_Is_Serializable()
    {
        GveHandsPart part = GetPart();

        string json = TestHelper.SerializePart(part);
        GveHandsPart part2 =
            TestHelper.DeserializePart<GveHandsPart>(json)!;

        Assert.Equal(part.Id, part2.Id);
        Assert.Equal(part.TypeId, part2.TypeId);
        Assert.Equal(part.ItemId, part2.ItemId);
        Assert.Equal(part.RoleId, part2.RoleId);
        Assert.Equal(part.CreatorId, part2.CreatorId);
        Assert.Equal(part.UserId, part2.UserId);

        Assert.Equal(part.Hands.Count, part2.Hands.Count);
    }

    [Fact]
    public void GetDataPins_NoEntries_Ok()
    {
        GveHandsPart part = GetPart();
        part.Hands.Clear();

        List<DataPin> pins = [.. part.GetDataPins(null)];

        Assert.Single(pins);
        DataPin pin = pins[0];
        Assert.Equal("tot-count", pin.Name);
        TestHelper.AssertPinIds(part, pin);
        Assert.Equal("0", pin.Value);
    }

    [Fact]
    public void GetDataPins_Entries_Ok()
    {
        GveHandsPart part = GetEmptyPart();

        for (int n = 1; n <= 3; n++)
        {
            part.Hands.Add(new GveHand
            {
                Eid = $"h{n}",
                Tag = $"t{n}",
                Color = n == 2 ? "red" : "black"
            });
        }

        List<DataPin> pins = [.. part.GetDataPins(null)];

        Assert.Equal(9, pins.Count);

        DataPin? pin = pins.Find(p => p.Name == "tot-count");
        Assert.NotNull(pin);
        TestHelper.AssertPinIds(part, pin!);
        Assert.Equal("3", pin!.Value);

        for (int n = 1; n <= 3; n++)
        {
            // eid
            pin = pins.Find(p => p.Name == "eid" && p.Value == $"h{n}");
            Assert.NotNull(pin);
            TestHelper.AssertPinIds(part, pin!);
            // tag
            pin = pins.Find(p => p.Name == "tag" && p.Value == $"t{n}");
            Assert.NotNull(pin);
            TestHelper.AssertPinIds(part, pin!);
        }

        // color
        pin = pins.Find(p => p.Name == "color" && p.Value == "black");
        Assert.NotNull(pin);
        TestHelper.AssertPinIds(part, pin!);

        pin = pins.Find(p => p.Name == "color" && p.Value == "red");
        Assert.NotNull(pin);
        TestHelper.AssertPinIds(part, pin!);
    }
}