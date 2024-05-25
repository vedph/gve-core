using Gve.Api.Models;
using Gve.Text;

namespace Gve.Api.Services;

internal static class CharChainOperationMapper
{
    public static CharChainOperation Map(CharChainOperationModel model)
    {
        ArgumentNullException.ThrowIfNull(model);
        CharChainOperation op = model.Type switch
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

        op.Rank = model.Rank;
        op.GroupId = model.GroupId;
        if (model.Features?.Count > 0) op.Features = model.Features;
        if (model.Sources?.Count > 0) op.Sources = model.Sources;
        op.Diplomatics = model.Diplomatic;
        op.Id = model.Id;
        op.InputTag = model.InputTag;
        op.OutputTag = model.OutputTag;
        op.AtAsIndex = model.AtAsIndex;
        op.At = model.At;
        op.Run = model.Run;
        op.Value = model.Value;

        if (op is IHasToArgument toOp)
        {
            toOp.ToAsIndex = model.ToAsIndex;
            toOp.To = model.To;
            if (op is CharChainSwapOperation swapOp)
            {
                swapOp.ToRun = model.ToRun;
            }
        }

        return op;
    }
}
