using System.Collections.Generic;

namespace Gve.Cli.Commands;

/// <summary>
/// Options for <see cref="MongoDictionaryImporter"/>.
/// </summary>
public class MongoDictionaryImporterOptions
{
    /// <summary>
    /// The MongoDB connection string.
    /// </summary>
    public string ConnectionString { get; set; } = "mongodb://localhost:27017";

    /// <summary>
    /// The MongoDB database name.
    /// </summary>
    public string DatabaseName { get; set; } = "cadmus-gve";

    /// <summary>
    /// The reader options.
    /// </summary>
    public required ExcelDictionaryReaderOptions Reader { get; set; }

    /// <summary>
    /// The ID of the facet to which the items will belong.
    /// </summary>
    public string FacetId { get; set; } = "default";

    /// <summary>
    /// The group ID template. Variable names are enclosed in <c>{{}}</c>.
    /// </summary>
    public string? GroupId { get; set; }

    /// <summary>
    /// The item title template. Variable names are enclosed in <c>{{}}</c>.
    /// </summary>
    public string Title { get; set; } = "Item {{row_number}}";

    /// <summary>
    /// The item description template. Variable names are enclosed in <c>{{}}</c>.
    /// </summary>
    public string Description { get; set; } = "";

    /// <summary>
    /// The dictionary keys to include as item's metadata.
    /// </summary>
    public HashSet<string>? MetadataKeys { get; set; }

    /// <summary>
    /// The item's flags, in hexadecimal notation.
    /// </summary>
    public string? Flags { get; set; }

    /// <summary>
    /// The user ID to associate with the imported items.
    /// </summary>
    public string? UserId { get; set; } = "";
}
