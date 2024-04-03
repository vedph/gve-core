using System;
using System.Collections.Generic;
using Xunit;

namespace Gve.Text.Test;

public sealed class CharChainMoveBeforeOperationTest
{
    [Fact]
    public void ParseOperation_At1Run1To3()
    {
        CharChainMoveBeforeOperation op = CharChainOperation
            .Parse<CharChainMoveBeforeOperation>("1>[3");

        Assert.Equal(VarOperationType.MoveBefore, op.Type);
        Assert.Null(op.InputTag);
        Assert.Null(op.OutputTag);
        Assert.False(op.AtAsIndex);
        Assert.Equal(1, op.At);
        Assert.Equal(1, op.Run);
        Assert.Equal(3, op.To);
        Assert.Null(op.Value);

        Assert.Equal(0, op.Rank);
        Assert.Null(op.GroupId);
        Assert.Empty(op.Features);
    }

    [Fact]
    public void ParseOperation_At1Run2To3()
    {
        CharChainMoveBeforeOperation op = CharChainOperation
            .Parse<CharChainMoveBeforeOperation>("1x2>[3");

        Assert.Equal(VarOperationType.MoveBefore, op.Type);
        Assert.Null(op.InputTag);
        Assert.Null(op.OutputTag);
        Assert.False(op.AtAsIndex);
        Assert.Equal(1, op.At);
        Assert.Equal(2, op.Run);
        Assert.Equal(3, op.To);
        Assert.Null(op.Value);

        Assert.Equal(0, op.Rank);
        Assert.Null(op.GroupId);
        Assert.Empty(op.Features);
    }

    [Fact]
    public void Execute_At1Run1To3()
    {
        ChainOperationContext<char> context = new();
        context.Chain.AppendRange("", "v0", TestHelper.GetNodesFrom("ABC"));
        CharChainMoveBeforeOperation op = new()
        {
            At = 1,
            To = 3,
            InputTag = "v0",
            OutputTag = "v1",
            Features =
            [
                OperationFeature.Parse("*log:=\"moved A before C\""),
                new OperationFeature("reason", "A likes C")
            ]
        };

        op.Execute(context);

        Assert.Equal("BAC", TestHelper.GetVersionString(
            context.Chain.GetTagNodes("v1")));
        // context tag
        Assert.Equal("v1", context.LastTag);
        // context features
        Assert.Single(context.Features);
        Assert.Equal("log", context.Features[0].Name);
        Assert.Equal("moved A before C", context.Features[0].Value);
        // node features
        List<OperationFeature> nodeFeatures = context.NodeFeatures["v1_1"];
        Assert.NotEmpty(nodeFeatures);
        Assert.Equal("reason", nodeFeatures[0].Name);
        Assert.Equal("A likes C", nodeFeatures[0].Value);
        // context steps
        Assert.Single(context.Steps);
        Assert.Equal("v0", context.Steps[0].InputTag);
        Assert.Equal("v1", context.Steps[0].OutputTag);
        Assert.Same(op, context.Steps[0].Operation);
        Assert.Single(context.Steps[0].FeatureSet.Features);
        Assert.Single(context.Steps[0].FeatureSet.NodeFeatures);
    }
}
