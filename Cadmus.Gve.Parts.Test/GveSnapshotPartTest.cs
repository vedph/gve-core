using System;
using Xunit;
using Cadmus.Core;
using Cadmus.Seed.Gve.Parts;
using System.Collections.Generic;
using System.Linq;
using Gve.Text.DTO;

namespace Cadmus.Gve.Parts.Test;

public sealed class GveSnapshotPartTest
{
    private static GveSnapshotPart GetPart()
    {
        GveSnapshotPartSeeder seeder = new();
        IItem item = new Item
        {
            FacetId = "default",
            CreatorId = "zeus",
            UserId = "zeus",
            Description = "Test item",
            Title = "Test Item",
            SortKey = ""
        };
        return (GveSnapshotPart)seeder.GetPart(item, null, null)!;
    }

    private static GveSnapshotPart GetEmptyPart()
    {
        return new GveSnapshotPart
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
        GveSnapshotPart part = GetPart();

        string json = TestHelper.SerializePart(part);
        GveSnapshotPart part2 = TestHelper.DeserializePart<GveSnapshotPart>(json)!;

        Assert.Equal(part.Id, part2.Id);
        Assert.Equal(part.TypeId, part2.TypeId);
        Assert.Equal(part.ItemId, part2.ItemId);
        Assert.Equal(part.RoleId, part2.RoleId);
        Assert.Equal(part.CreatorId, part2.CreatorId);
        Assert.Equal(part.UserId, part2.UserId);
    }

    [Fact]
    public void GetDataPins_Empty()
    {
        GveSnapshotPart part = GetEmptyPart();

        Assert.Equal(2, part.GetDataPins().Count());
    }

    [Fact]
    public void GetDataPins_NonEmpty()
    {
        GveSnapshotPart part = GetEmptyPart();
        part.Value = new Snapshot
        {
            Text =
            [
                new() {
                    Index = 0,
                    Id = 1,
                    Data = 'A',
                }
            ],
            Operations =
            [
                new(),
                new()
            ]
        };

        List<DataPin> pins = part.GetDataPins(null).ToList();
        Assert.Equal(2, pins.Count);

        DataPin? pin = pins.Find(p => p.Name == "node-count" && p.Value == "1");
        Assert.NotNull(pin);
        TestHelper.AssertPinIds(part, pin!);

        pin = pins.Find(p => p.Name == "op-count" && p.Value == "2");
        Assert.NotNull(pin);
        TestHelper.AssertPinIds(part, pin!);
    }
}