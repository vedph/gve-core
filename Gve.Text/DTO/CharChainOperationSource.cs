using System;
using System.Collections.Generic;

namespace Gve.Text.DTO;

/// <summary>
/// Source data for building a character-based chain operation.
/// </summary>
public class CharChainOperationSource
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
    /// Initializes a new instance of the <see cref="CharChainOperationSource"/>
    /// class.
    /// </summary>
    public CharChainOperationSource()
    {
        Id = "";
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CharChainOperationSource"/>
    /// class.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public CharChainOperationSource(CharChainOperation operation)
    {
        ArgumentNullException.ThrowIfNull(operation);

        Rank = operation.Rank;
        GroupId = operation.GroupId;
        Features = operation.Features;
        Sources = operation.Sources;
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

    /// <summary>
    /// Converts this source into the corresponding operation.
    /// </summary>
    /// <returns>Operation.</returns>
    public CharChainOperation ToOperation()
    {
        CharChainOperation op = Type switch
        {
            OperationType.Delete => new CharChainDeleteOperation(),
            OperationType.AddBefore => new CharChainAddBeforeOperation(),
            OperationType.AddAfter => new CharChainAddAfterOperation(),
            OperationType.MoveBefore => new CharChainMoveBeforeOperation(),
            OperationType.MoveAfter => new CharChainMoveAfterOperation(),
            OperationType.Swap => new CharChainSwapOperation(),
            OperationType.Annotate => new CharChainAnnotateOperation(),
            _ => new CharChainReplaceOperation(),
        };

        op.Rank = Rank;
        op.GroupId = GroupId;
        if (Features?.Count > 0) op.Features = Features;
        if (Sources?.Count > 0) op.Sources = Sources;
        op.Id = Id;
        op.InputTag = InputTag;
        op.OutputTag = OutputTag;
        op.AtAsIndex = AtAsIndex;
        op.At = At;
        op.Run = Run;
        op.Value = Value;

        if (op is IHasToArgument toOp)
        {
            toOp.ToAsIndex = ToAsIndex;
            toOp.To = To;
            if (op is CharChainSwapOperation swapOp)
            {
                swapOp.ToRun = ToRun;
            }
        }

        return op;
    }

    /// <summary>
    /// Converts to string.
    /// </summary>
    /// <returns>
    /// A <see cref="string" /> that represents this instance.
    /// </returns>
    public override string ToString()
    {
        return ToOperation().ToString();
    }
}
