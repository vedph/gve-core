using Bogus;
using Cadmus.Core;
using Cadmus.Gve.Parts;
using Fusi.Tools.Configuration;
using Gve.Text;
using Gve.Text.DTO;
using System;
using System.Collections.Generic;

namespace Cadmus.Seed.Gve.Parts;

/// <summary>
/// Seeder for <see cref="GveSnapshotPart"/>.
/// Tag: <c>seed.it.vedph.gve.snapshot</c>.
/// </summary>
/// <seealso cref="PartSeederBase" />
[Tag("seed.it.vedph.gve.snapshot")]
public sealed class GveSnapshotPartSeeder : PartSeederBase
{
    private static List<CharChainNode> GetNodes(string text)
    {
        List<CharChainNode> nodes = [];

        for (int i = 0; i < text.Length; i++)
        {
            nodes.Add(new CharChainNode
            {
                Index = i,
                Id = i + 1,
                Data = text[i],
                Label = text[i].ToString(),
            });
        }

        return nodes;
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

        GveSnapshotPart part = new Faker<GveSnapshotPart>()
           .RuleFor(p => p.Value, f => new Snapshot
           {
               Size = new Size(800, 600),
               Image = new()
               {
                   Url = "https://via.placeholder.com/800x600",
                   Opacity = 0.8
               },
               Text = GetNodes(f.Lorem.Sentence(5)),
               TextStyle = "font-size: 24px",
               TextOptions = new()
               {
                   Offset = new Point(0, 100),
                   LineHeightOffset = 8,
               },
               Operations =
               [
                   new()
                   {
                       Type = OperationType.Delete,
                       At = 1,
                       Run = 1,
                       Diplomatics = new()
                       {
                           G = "<g id=\"del\"><line x1=\"12\" y1=\"150\" x2=\"24\" y2=\"100\" stroke=\"red\" stroke-width=\"6\"/></g>"
                       }
                   }
               ]
           })
           .Generate();
        SetPartMetadata(part, roleId, item);

        return part;
    }
}
