using Bogus;
using Cadmus.Core;
using Cadmus.Gve.Parts;
using Fusi.Tools.Configuration;
using System;
using System.Collections.Generic;

namespace Cadmus.Seed.Gve.Parts;

/// <summary>
/// Seeder for <see cref="GveHandsPart"/>.
/// Tag: <c>seed.it.vedph.gve.hands</c>.
/// </summary>
/// <seealso cref="PartSeederBase" />
[Tag("seed.it.vedph.gve.hands")]
public sealed class GveHandsPartSeeder : PartSeederBase
{
    private static List<GveHand> GetHands(int count)
    {
        List<GveHand> hands = new(count);
        for (int i = 0; i < count; i++)
        {
            hands.Add(new Faker<GveHand>()
                .RuleFor(h => h.Eid, $"h{i + 1}")
                .RuleFor(h => h.Tool, f => f.PickRandom("pen", "pencil"))
                .RuleFor(h => h.Color, f => f.PickRandom("black", "blue"))
                .RuleFor(h => h.Notes, f => f.Random.Bool(0.25F)
                    ? new Dictionary<string, string>
                    {
                        ["note"] = f.Lorem.Sentence()
                    }
                    : null));
        }
        return hands;
    }

    /// <summary>
    /// Creates and seeds a new part.
    /// </summary>
    /// <param name="item">The item this part should belong to.</param>
    /// <param name="roleId">The optional part role ID.</param>
    /// <param name="factory">The part seeder factory. This is used
    /// for layer parts, which need to seed a set of fragments.</param>
    /// <returns>A new part or null.</returns>
    /// <exception cref="ArgumentNullException">item or factory</exception>
    public override IPart? GetPart(IItem item, string? roleId,
        PartSeederFactory? factory)
    {
        ArgumentNullException.ThrowIfNull(item);

        GveHandsPart part = new Faker<GveHandsPart>()
           .RuleFor(p => p.Hands, f => GetHands(f.Random.Number(1, 3)))
           .Generate();
        SetPartMetadata(part, roleId, item);

        return part;
    }
}
