using Cadmus.Core;
using Cadmus.Gve.Parts;
using Fusi.Tools.Configuration;
using System;
using System.Reflection;

namespace Cadmus.Seed.Gve.Parts.Test;

public sealed class GveSnapshotPartSeederTest
{
    private static readonly PartSeederFactory _factory =
        TestHelper.GetFactory();
    private static readonly SeedOptions _seedOptions =
        _factory.GetSeedOptions();
    private static readonly IItem _item =
        _factory.GetItemSeeder().GetItem(1, "facet");

    [Fact]
    public void TypeHasTagAttribute()
    {
        Type t = typeof(GveSnapshotPartSeeder);
        TagAttribute? attr = t.GetTypeInfo().GetCustomAttribute<TagAttribute>();
        Assert.NotNull(attr);
        Assert.Equal("seed.it.vedph.gve.snapshot", attr!.Tag);
    }

    [Fact]
    public void Seed_Ok()
    {
        GveSnapshotPartSeeder seeder = new();
        seeder.SetSeedOptions(_seedOptions);

        IPart? part = seeder.GetPart(_item, null, _factory);

        Assert.NotNull(part);

        GveSnapshotPart? p = part as GveSnapshotPart;
        Assert.NotNull(p);

        TestHelper.AssertPartMetadata(p!);

        Assert.NotNull(p!.Value);
    }
}