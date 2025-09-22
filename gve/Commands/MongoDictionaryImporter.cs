using Cadmus.Core;
using Cadmus.General.Parts;
using Cadmus.Mongo;
using Fusi.Tools.Text;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Gve.Cli.Commands;

/// <summary>
/// MongoDB dictionary-based data importer.
/// </summary>
public sealed class MongoDictionaryImporter
{
    private readonly MongoCadmusRepository _repository;
    private readonly MongoDictionaryImporterOptions _options;
    private readonly StandardItemSortKeyBuilder _sortKeyBuilder;

    public MongoDictionaryImporter(MongoCadmusRepository repository,
        MongoDictionaryImporterOptions options)
    {
        _repository = repository
            ?? throw new ArgumentNullException(nameof(repository));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _sortKeyBuilder = new();
    }

    private static string ResolveTemplate(string template,
        Dictionary<string, string> data)
    {
        return TextTemplate.FillTemplate(template,
            (IDictionary<string,object>)data, "{{", "}}");
    }

    public IItem Import(Dictionary<string, string> data)
    {
        ArgumentNullException.ThrowIfNull(data);

        DateTime now = DateTime.Now;
        int flags = string.IsNullOrEmpty(_options.Flags)
            ? 0
            : int.Parse(_options.Flags, NumberStyles.HexNumber);

        // create the item
        Item item = new()
        {
            FacetId = _options.FacetId,
            GroupId = string.IsNullOrEmpty(_options.GroupId)?
                null : ResolveTemplate(_options.GroupId, data),
            Title = ResolveTemplate(_options.Title, data),
            Description = ResolveTemplate(_options.Description, data),
            Flags = flags,
            CreatorId = _options.UserId ?? "zeus",
            UserId = _options.UserId ?? "zeus",
            TimeCreated = now,
            TimeModified = now,
        };
        item.SortKey = _sortKeyBuilder.BuildKey(item, null);

        // create the metadata if required
        if (data.Keys.Any(k => !k.StartsWith('_')))
        {
            MetadataPart part = new()
            {
                ItemId = item.Id,
                CreatorId= item.CreatorId,
                UserId= item.UserId,
                TimeCreated = now,
                TimeModified = now,
            };
            part.Metadata.AddRange(data
                .Where(p => !p.Key.StartsWith('_'))
                .Select(p => new Metadatum { Name = p.Key, Value = p.Value }));
            item.Parts.Add(part);
            _repository.AddPart(part, true);
        }

        _repository.AddItem(item, true);

        return item;
    }
}
