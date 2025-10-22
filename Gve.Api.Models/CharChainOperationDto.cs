using Gve.Text;
using System;

namespace Gve.Api.Models;

/// <summary>
/// Common operation DTO used for most char-chain operations.
/// </summary>
public class CharChainOperationDto : CharChainOperation
{
    // added for DTO, the DSL representing the operation
    public string Dsl { get; }

    // from move/swap
    public bool ToAsIndex { get; set; }
    public int To { get; set; }

    // from swap
    public int ToRun { get; set; }

    public CharChainOperationDto(CharChainOperation source) : base(source.Type)
    {
        Id = source.Id;
        Rank = source.Rank;
        GroupId = source.GroupId;
        Features = source.Features;
        Sources = source.Sources;
        Diplomatics = source.Diplomatics;

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
