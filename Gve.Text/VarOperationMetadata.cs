using System.Collections.Generic;

namespace Gve.Text;

/// <summary>
/// Generic metadata about a text variant operation.
/// </summary>
public class VarOperationMetadata
{
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
    /// Gets or sets the tags identifying the text versions the result of this
    /// operation will be linked to.
    /// </summary>
    public HashSet<string> VersionTags { get; set; }

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
        VersionTags = [];
        Features = [];
        Sources = [];
    }
}
