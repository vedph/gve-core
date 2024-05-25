using Gve.Text;

namespace Gve.Api.Models;

/// <summary>
/// Metadata about a character-based chain operation.
/// </summary>
public class CharChainOperationModel
{
    /// <summary>
    /// The certainty rank for this operation: 0=not specified,
    /// 1=most probable, 2=less probable, etc.
    /// </summary>
    public short Rank { get; set; }

    /// <summary>
    /// The optional group identifier. When an operation is part of a group,
    /// all the operation in it are considered as a unique virtual operation.
    /// </summary>
    public string? GroupId { get; set; }

    /// <summary>
    /// The features to be attached to the result of this operation.
    /// </summary>
    public IList<OperationFeature>? Features { get; set; }

    /// <summary>
    /// The sources for this operation. These represent any type
    /// of source for it, such as a manuscript, an author, etc.
    /// </summary>
    public IList<OperationSource>? Sources { get; set; }

    /// <summary>
    /// The diplomatic description of this operation, representing
    /// its graphical representation in the original document.
    /// </summary>
    public OperationDiplomatics? Diplomatic { get; set; }

    /// <summary>
    /// The operation identifier. This can be used to reference
    /// a specific operation in a transformation.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// The operation type.
    /// </summary>
    public OperationType Type { get; set; }

    /// <summary>
    /// The input version tag for this operation. If null, the default tag
    /// (=the tag of the last operation executed on the context)
    /// is used.
    /// </summary>
    public string? InputTag { get; set; }

    /// <summary>
    /// The output version tag for this operation. If null,
    /// a value will be automatically generated from the input tag if this
    /// ends with digits; else, it will be equal to the input tag.
    /// </summary>
    public string? OutputTag { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether <see cref="At"/> refers to
    /// an index rather than a chain node ID.
    /// </summary>
    public bool AtAsIndex { get; set; }

    /// <summary>
    /// The ordinal character number to start at.
    /// </summary>
    public int At { get; set; }

    /// <summary>
    /// The run of characters to include from <see cref="At"/>.
    /// </summary>
    public int Run { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether <see cref="To"/> refers to
    /// an index rather than a chain node ID.
    /// </summary>
    public bool ToAsIndex { get; set; }

    /// <summary>
    /// To-coordinate (for move/swap only).
    /// </summary>
    public int To { get; set; }

    /// <summary>
    /// The run of characters to include from <see cref="To"/>.
    /// </summary>
    public int ToRun { get; set; }

    /// <summary>
    /// Gets the value argument for this operation. This is the primary argument,
    /// whose meaning varies depending on the operation type.
    /// </summary>
    public string? Value { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CharChainOperationModel"/>
    /// class.
    /// </summary>
    public CharChainOperationModel()
    {
        Id = "";
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CharChainOperationModel"/>
    /// class.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public CharChainOperationModel(CharChainOperation operation)
    {
        ArgumentNullException.ThrowIfNull(operation);

        Rank = operation.Rank;
        GroupId = operation.GroupId;
        Features = operation.Features;
        Sources = operation.Sources;
        Diplomatic = operation.Diplomatics;
        Id = operation.Id;
        Type = operation.Type;
        InputTag = operation.InputTag;
        OutputTag = operation.OutputTag;
        AtAsIndex = operation.AtAsIndex;
        At = operation.At;
        Run = operation.Run;
        Value = operation.Value;

        if (operation is IHasToArgument toOp)
        {
            ToAsIndex = toOp.ToAsIndex;
            To = toOp.To;
            if (operation is CharChainSwapOperation swapOp)
            {
                ToRun = swapOp.ToRun;
            }
        }
    }
}
