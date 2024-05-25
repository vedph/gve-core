using NanoidDotNet;
using System.Collections.Generic;

namespace Gve.Text;

/// <summary>
/// Generic metadata about a text operation.
/// </summary>
public class OperationMetadata
{
    /// <summary>
    /// The key for the rank feature. This is automatically projected from
    /// <see cref="Rank"/>.
    /// </summary>
    public const string RANK_KEY = "rank";

    /// <summary>
    /// The key for the source feature(s). These are automatically projected
    /// from <see cref="Sources"/>.
    /// </summary>
    public const string SOURCE_KEY = "source";

    /// <summary>
    /// The symbols corresponding to <see cref="OperationType"/>'s.
    /// </summary>
    public static readonly string[] TYPE_SYMBOLS =
    [
        "=",
        "-",
        "+[",
        "+]",
        ">[",
        ">]",
        "<>",
         ":",
    ];

    /// <summary>
    /// Gets or sets the operation identifier. This can be used to reference
    /// a specific operation in a transformation, and is automatically
    /// generated using <see cref="Nanoid.Generate(string, int)"/> unless
    /// you want to assign it some human-friendly name.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Gets or sets the certainty rank for this operation: 0=not specified,
    /// 1=most probable, 2=less probable, etc.
    /// </summary>
    public short Rank { get; set; }

    /// <summary>
    /// Gets or sets the optional group identifier. When an operation is part
    /// of a group, all the operation in it are considered as a unique virtual
    /// operation.
    /// </summary>
    public string? GroupId { get; set; }

    /// <summary>
    /// Gets or sets the features to be attached to the result of this operation.
    /// </summary>
    public IList<OperationFeature> Features { get; set; }

    /// <summary>
    /// Gets or sets the sources for this operation. These represent any type
    /// of source for it, such as a manuscript, an author, etc.
    /// </summary>
    public IList<OperationSource> Sources { get; set; }

    /// <summary>
    /// Gets or sets the diplomatic description of this operation, representing
    /// its graphical representation in the original document.
    /// </summary>
    public OperationDiplomatics? Diplomatics { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="OperationMetadata"/> class.
    /// </summary>
    public OperationMetadata()
    {
        Id = GenerateId();

        Features = [];
        Sources = [];
    }

    /// <summary>
    /// Generates a new identifier for an operation.
    /// </summary>
    /// <returns>Identifier.</returns>
    public static string GenerateId() => Nanoid.Generate("1234567890abcdef", 10);
}

/// <summary>
/// Type of operation.
/// </summary>
public enum OperationType
{
    /// <summary>The replace operation.</summary>
    Replace = 0,
    /// <summary>The delete operation.</summary>
    Delete,
    /// <summary>The add before operation.</summary>
    AddBefore,
    /// <summary>The add after operation.</summary>
    AddAfter,
    /// <summary>The move before operation.</summary>
    MoveBefore,
    /// <summary>The move after operation.</summary>
    MoveAfter,
    /// <summary>The swap operation.</summary>
    Swap,
    /// <summary>The annotate operation.</summary>
    Annotate
}
