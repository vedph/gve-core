using System.Collections.Generic;
using System;
using Xunit;

namespace Gve.Text.Test;

public sealed class CharChainAnnotateOperationTest
{
    [Fact]
    public void ParseOperation_At0()
    {
        CharChainAnnotateOperation op = CharChainOperation
            .Parse<CharChainAnnotateOperation>("0: [note=sample]");

        Assert.Equal(VarOperationType.Annotate, op.Type);
        Assert.Null(op.InputTag);
        Assert.Null(op.OutputTag);
        Assert.False(op.AtAsIndex);
        Assert.Equal(0, op.At);
        Assert.Equal(1, op.Run);
        Assert.Null(op.Value);
        Assert.Equal(0, op.Rank);
        Assert.Null(op.GroupId);
        Assert.Single(op.Features);
    }

    [Fact]
    public void ParseOperation_At3Run2()
    {
        CharChainAnnotateOperation op = CharChainOperation
            .Parse<CharChainAnnotateOperation>("3x2: [note=sample]");

        Assert.Equal(VarOperationType.Annotate, op.Type);
        Assert.Null(op.InputTag);
        Assert.Null(op.OutputTag);
        Assert.False(op.AtAsIndex);
        Assert.Equal(3, op.At);
        Assert.Equal(2, op.Run);
        Assert.Null(op.Value);
        Assert.Equal(0, op.Rank);
        Assert.Null(op.GroupId);
        Assert.Single(op.Features);
    }

    [Fact]
    public void Execute_At0()
    {
        ChainOperationContext<char> context = new();
        context.Chain.AppendRange("", "v0", TestHelper.GetNodesFrom("ABC"));
        CharChainAnnotateOperation op = new()
        {
            At = 0,
            InputTag = "v0",
            OutputTag = "v0",
            Features =
            [
                OperationFeature.Parse("*log:=\"context annotation\""),
            ]
        };

        op.Execute(context);

        Assert.Equal("ABC", TestHelper.GetVersionString(
            context.Chain.GetTagNodes("v0")));
        // context tag
        Assert.Equal("v0", context.LastTag);
        // context features
        Assert.Single(context.Features);
        Assert.Equal("log", context.Features[0].Name);
        Assert.Equal("context annotation", context.Features[0].Value);
        // node features
        Assert.Empty(context.NodeFeatures);
        // context steps
        Assert.Single(context.Steps);
        Assert.Equal("v0", context.Steps[0].InputTag);
        Assert.Equal("v0", context.Steps[0].OutputTag);
        Assert.Same(op, context.Steps[0].Operation);
        Assert.Single(context.Steps[0].FeatureSet.Features);
        Assert.Empty(context.Steps[0].FeatureSet.NodeFeatures);
    }

    [Fact]
    public void Execute_At1Run2()
    {
        ChainOperationContext<char> context = new();
        context.Chain.AppendRange("", "v0", TestHelper.GetNodesFrom("ABC"));
        CharChainAnnotateOperation op = new()
        {
            At = 1,
            Run = 2,
            InputTag = "v0",
            OutputTag = "v0",
            Features =
            [
                OperationFeature.Parse("*log:=\"context annotation\""),
                new OperationFeature("x", "y"),
            ]
        };

        op.Execute(context);

        Assert.Equal("ABC", TestHelper.GetVersionString(
            context.Chain.GetTagNodes("v0")));
        // context tag
        Assert.Equal("v0", context.LastTag);
        // context features
        Assert.Single(context.Features);
        Assert.Equal("log", context.Features[0].Name);
        Assert.Equal("context annotation", context.Features[0].Value);
        // node features
        Assert.Equal(2, context.NodeFeatures.Count);
        List<OperationFeature> nodeFeatures =
            context.NodeFeatures[Tuple.Create("v0", 1)];
        Assert.NotEmpty(nodeFeatures);
        Assert.Equal("x", nodeFeatures[0].Name);
        Assert.Equal("y", nodeFeatures[0].Value);

        nodeFeatures = context.NodeFeatures[Tuple.Create("v0", 2)];
        Assert.NotEmpty(nodeFeatures);
        Assert.Equal("x", nodeFeatures[0].Name);
        Assert.Equal("y", nodeFeatures[0].Value);

        // context steps
        Assert.Single(context.Steps);
        Assert.Equal("v0", context.Steps[0].InputTag);
        Assert.Equal("v0", context.Steps[0].OutputTag);
        Assert.Same(op, context.Steps[0].Operation);
        Assert.Single(context.Steps[0].FeatureSet.Features);
        Assert.Equal(2, context.Steps[0].FeatureSet.NodeFeatures.Count);
    }
}
