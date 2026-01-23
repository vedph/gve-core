using Gve.Text;
using System;

namespace Gve.Api.Models;

/// <summary>
/// Common operation DTO used for most char-chain operations.
/// </summary>
public class CharChainOperationDto : CharChainOperation
{
    /// <summary>
    /// Gets the domain-specific language (DSL) representation of the operation.
    /// Used for DTO.
    /// </summary>
    public string Dsl { get; }

    /// <summary>
    /// Gets or sets a value indicating whether the target position should be
    /// interpreted as an index.
    /// </summary>
    public bool ToAsIndex { get; set; }

    /// <summary>
    /// Gets or sets the target position.
    /// </summary>
    public int To { get; set; }

    /// <summary>
    /// Gets or sets the to-count.
    /// </summary>
    public int ToRun { get; set; }

    /// <summary>
    /// Initializes a new instance of the CharChainOperationDto class by copying
    /// relevant data from the specified CharChainOperation source.
    /// </summary>
    /// <param name="source">The CharChainOperation instance from which to copy
    /// operation data. Cannot be null. The specific type of the
    /// source determines which additional properties are set.</param>
    public CharChainOperationDto(CharChainOperation source) : base(source.Type)
    {
        Id = source.Id;
        Rank = source.Rank;
        GroupId = source.GroupId;
        Features = source.Features;
        Sources = source.Sources;

        InputTag = source.InputTag;
        OutputTag = source.OutputTag;
        AtAsIndex = source.AtAsIndex;
        At = source.At;
        Run = source.Run;
        Value = source.Value;
        NoTraceFeatures = source.NoTraceFeatures;

        if (source is CharChainMoveAfterOperation moveAfterOp)
        {
            ToAsIndex = moveAfterOp.ToAsIndex;
            To = moveAfterOp.To;
        }
        else if (source is CharChainMoveBeforeOperation moveBeforeOp)
        {
            ToAsIndex = moveBeforeOp.ToAsIndex;
            To = moveBeforeOp.To;
        }
        else if (source is CharChainSwapOperation swapOp)
        {
            ToAsIndex = swapOp.ToAsIndex;
            To = swapOp.To;
            ToRun = swapOp.ToRun;
        }

        Dsl = source.ToString();
    }

    /// <summary>
    /// Not used.
    /// </summary>
    /// <param name="context">context</param>
    /// <exception cref="NotImplementedException">not implemented</exception>
    public override void Execute(ChainOperationContext<char> context)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Returns a string representing this object.
    /// </summary>
    /// <returns>The DSL representing this operation.</returns>
    public override string ToString() => Dsl;
}
