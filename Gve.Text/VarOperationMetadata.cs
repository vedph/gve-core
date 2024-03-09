using System.Collections.Generic;

namespace Gve.Text;

/// <summary>
/// Generic metadata about a text variant operation.
/// </summary>
public class VarOperationMetadata
{
    /// <summary>
    /// The key for the rank feature.
    /// </summary>
    public const string RANK_KEY = "rank";

    /// <summary>
    /// The symbols corresponding to <see cref="VarOperationType"/>'s.
    /// </summary>
    public static readonly string[] TYPE_SYMBOLS =
    [
        "=",
        "-",
        "+<",
        "+>",
        ":",
        ">",
        "<>"
    ];

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
    public IList<Feature> Features { get; set; }

    /// <summary>
    /// Gets or sets the mapping between version tags and the ordinal number
    /// which sorts this operation in the context of each version. This is used
    /// to determine the order of operations for each text version. For instance,
    /// an operation might be the 3rd in the context of version A, and the 5th
    /// in the context of version B.
    /// </summary>
    public IDictionary<string, int> TaggedOrdinals { get; set; }

    /// <summary>
    /// Gets or sets the sources for this operation. These represent any type
    /// of source for it, such as a manuscript, an author, etc.
    /// </summary>
    public IList<VarSource> Sources { get; set; }

    /// <summary>
    /// Gets or sets the diplomatic description of this operation, representing
    /// its graphical representation in the original document.
    /// </summary>
    public VarDiplomatic? Diplomatic { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="VarOperationMetadata"/> class.
    /// </summary>
    public VarOperationMetadata()
    {
        TaggedOrdinals = new Dictionary<string, int>();
        Features = [];
        Sources = [];
    }
}

/// <summary>
/// Type of <see cref="VarOperation"/>.
/// </summary>
public enum VarOperationType
{
    /// <summary>The replace operation.</summary>
    Replace = 0,
    /// <summary>The delete operation.</summary>
    Delete,
    /// <summary>The add left operation.</summary>
    AddLeft,
    /// <summary>The add right operation.</summary>
    AddRight,
    /// <summary>The annotate operation.</summary>
    Annotate,
    /// <summary>The move operation.</summary>
    Move,
    /// <summary>The swap operation.</summary>
    Swap
}
