using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Gve.Text.Test;

public sealed class CharChainSwapOperationTest
{
    [Fact]
    public void ParseOperation_At1Run1To3()
    {
        CharChainSwapOperation op = CharChainOperation
            .Parse<CharChainSwapOperation>("1<>3");

        Assert.Equal(VarOperationType.Swap, op.Type);
        Assert.Null(op.InputTag);
        Assert.Null(op.OutputTag);
        Assert.False(op.AtAsIndex);
        Assert.Equal(1, op.At);
        Assert.Equal(1, op.Run);
        Assert.Equal(3, op.To);
        Assert.Equal(1, op.ToRun);
        Assert.Null(op.Value);

        Assert.Equal(0, op.Rank);
        Assert.Null(op.GroupId);
        Assert.Empty(op.Features);
        Assert.Empty(op.TaggedOrdinals);
    }

    [Fact]
    public void ParseOperation_At1Run2To3Run3()
    {
        CharChainSwapOperation op = CharChainOperation
            .Parse<CharChainSwapOperation>("1x2<>3x3");

        Assert.Equal(VarOperationType.Swap, op.Type);
        Assert.Null(op.InputTag);
        Assert.Null(op.OutputTag);
        Assert.False(op.AtAsIndex);
        Assert.Equal(1, op.At);
        Assert.Equal(2, op.Run);
        Assert.Equal(3, op.To);
        Assert.Equal(3, op.ToRun);
        Assert.Null(op.Value);

        Assert.Equal(0, op.Rank);
        Assert.Null(op.GroupId);
        Assert.Empty(op.Features);
        Assert.Empty(op.TaggedOrdinals);
    }

    [Fact]
    public void Execute_At1Run1To3Run1()
    {
        ChainOperationContext<char> context = new();
        context.Chain.AppendRange("", "v0", TestHelper.GetNodesFrom("ABC"));
        CharChainSwapOperation op = new()
        {
            At = 1,
            To = 3,
            InputTag = "v0",
            OutputTag = "v1",
            Features =
            [
                new Feature("*log", "swapped A with C"),
                new Feature("reason", "A and C are jealous")
            ]
        };

        op.Execute(context);

        Assert.Equal("CBA", TestHelper.GetVersionString(
            context.Chain.GetTagNodes("v1")));
        // context tag
        Assert.Equal("v1", context.LastTag);
        // context features
        Assert.Single(context.Features);
        Assert.Equal("log", context.Features[0].Name);
        Assert.Equal("swapped A with C", context.Features[0].Value);
        // node features
        List<Feature> nodeFeatures = context.NodeFeatures[Tuple.Create("v1", 1)];
        Assert.NotEmpty(nodeFeatures);
        Assert.Equal("reason", nodeFeatures[0].Name);
        Assert.Equal("A and C are jealous", nodeFeatures[0].Value);

        nodeFeatures = context.NodeFeatures[Tuple.Create("v1", 3)];
        Assert.NotEmpty(nodeFeatures);
        Assert.Equal("reason", nodeFeatures[0].Name);
        Assert.Equal("A and C are jealous", nodeFeatures[0].Value);

        // context steps
        Assert.Single(context.Steps);
        Assert.Equal("v0", context.Steps[0].InputTag);
        Assert.Equal("v1", context.Steps[0].OutputTag);
        Assert.Same(op, context.Steps[0].Operation);
        Assert.Single(context.Steps[0].FeatureSet.Features);
        Assert.Equal(2, context.Steps[0].FeatureSet.NodeFeatures.Count);
    }
}
