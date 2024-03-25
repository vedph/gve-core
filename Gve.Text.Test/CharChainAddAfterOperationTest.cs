using System;
using System.Collections.Generic;
using Xunit;

namespace Gve.Text.Test;

public sealed class CharChainAddAfterOperationTest
{
    [Fact]
    public void ParseOperation_At1Run1()
    {
        CharChainAddAfterOperation op = CharChainOperation
            .Parse<CharChainAddAfterOperation>("1+]X");

        Assert.Equal(VarOperationType.AddAfter, op.Type);
        Assert.Null(op.InputTag);
        Assert.Null(op.OutputTag);
        Assert.False(op.AtAsIndex);
        Assert.Equal(1, op.At);
        Assert.Equal(0, op.Run);
        Assert.Equal("X", op.Value);

        Assert.Equal(0, op.Rank);
        Assert.Null(op.GroupId);
        Assert.Empty(op.Features);
    }

    [Fact]
    public void Excute_Run1_Invalid()
    {
        CharChainAddAfterOperation op = CharChainOperation
            .Parse<CharChainAddAfterOperation>("1x1+]X");

        Assert.Throws<InvalidOperationException>(() =>
            op.Execute(new ChainOperationContext<char>()));
    }

    [Fact]
    public void Execute_At1()
    {
        ChainOperationContext<char> context = new();
        context.Chain.AppendRange("", "v0", TestHelper.GetNodesFrom("ABC"));
        CharChainAddAfterOperation op = new()
        {
            At = 1,
            InputTag = "v0",
            OutputTag = "v1",
            Value = "X",
            Features =
            [
                new Feature("*log", "added X after A"),
                new Feature("reason", "A likes X")
            ]
        };

        op.Execute(context);

        Assert.Equal("AXBC", TestHelper.GetVersionString(
            context.Chain.GetTagNodes("v1")));
        // context tag
        Assert.Equal("v1", context.LastTag);
        // context features
        Assert.Single(context.Features);
        Assert.Equal("log", context.Features[0].Name);
        Assert.Equal("added X after A", context.Features[0].Value);
        // node features
        List<Feature> nodeFeatures = context.NodeFeatures[Tuple.Create("v1", 4)];
        Assert.NotEmpty(nodeFeatures);
        Assert.Equal("reason", nodeFeatures[0].Name);
        Assert.Equal("A likes X", nodeFeatures[0].Value);
        // context steps
        Assert.Single(context.Steps);
        Assert.Equal("v0", context.Steps[0].InputTag);
        Assert.Equal("v1", context.Steps[0].OutputTag);
        Assert.Same(op, context.Steps[0].Operation);
        Assert.Single(context.Steps[0].FeatureSet.Features);
        Assert.Single(context.Steps[0].FeatureSet.NodeFeatures);
    }
}
