using System.Collections.Generic;
using Xunit;

namespace Gve.Text.Test;

public sealed class CharChainAnnotateOperationTest
{
    [Fact]
    public void ParseOperation_At0()
    {
        CharChainAnnotateOperation op = CharChainOperation
            .Parse<CharChainAnnotateOperation>("0: [note=sample]");

        Assert.Equal(OperationType.Annotate, op.Type);
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

        Assert.Equal(OperationType.Annotate, op.Type);
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
        context.Chain.AppendRange("v0", "v1", TestHelper.GetNodesFrom("ABC"));
        CharChainAnnotateOperation op = new()
        {
            At = 0,
            InputTag = "v0",
            OutputTag = "v1",
            Features =
            [
                OperationFeature.Parse("*log:=\"context annotation\""),
            ]
        };

        op.Execute(context);

        Assert.Equal("ABC", TestHelper.GetVersionString(
            context.Chain.GetTagNodes("v1")));
        // context tag
        Assert.Equal("v1", context.LastTag);
        // context features
        Assert.Single(context.Features);
        Assert.Equal("log", context.Features[0].Name);
        Assert.Equal("context annotation", context.Features[0].Value);
        // node features
        Assert.Empty(context.NodeFeatures);
        // context steps
        Assert.Single(context.Steps);
        Assert.Equal("v0", context.Steps[0].InputTag);
        Assert.Equal("v1", context.Steps[0].OutputTag);
        Assert.Same(op, context.Steps[0].Operation);
        Assert.Single(context.Steps[0].FeatureSet.Features);
        Assert.Empty(context.Steps[0].FeatureSet.NodeFeatures);
    }

    [Fact]
    public void Execute_At1Run2()
    {
        ChainOperationContext<char> context = new();
        context.Chain.AppendRange("v0", "v1", TestHelper.GetNodesFrom("ABC"));
        CharChainAnnotateOperation op = new()
        {
            At = 1,
            Run = 2,
            InputTag = "v0",
            OutputTag = "v1",
            Features =
            [
                OperationFeature.Parse("*log:=\"context annotation\""),
                new OperationFeature("x", "y"),
            ],
        };

        op.Execute(context);

        Assert.Equal("ABC", TestHelper.GetVersionString(
            context.Chain.GetTagNodes("v1")));
        // context tag
        Assert.Equal("v1", context.LastTag);
        // context features
        Assert.Single(context.Features);
        Assert.Equal("log", context.Features[0].Name);
        Assert.Equal("context annotation", context.Features[0].Value);
        // node features
        Assert.Equal(2, context.NodeFeatures.Count);
        List<OperationFeature> nodeFeatures = context.NodeFeatures["v1_1"];
        Assert.Equal(2, nodeFeatures.Count);
        string tagName = op.GetTracingFeatureName(CharChainOperation.F_TRACE_TAG);
        Assert.Contains(nodeFeatures, f => f.Name == tagName && f.Value == "v0:v1");
        Assert.Contains(nodeFeatures, f => f.Name == "x" && f.Value == "y");

        nodeFeatures = context.NodeFeatures["v1_2"];
        Assert.Single(nodeFeatures);
        Assert.Contains(nodeFeatures, f => f.Name == tagName && f.Value == "v0:v1");

        // context steps
        Assert.Single(context.Steps);
        Assert.Equal("v0", context.Steps[0].InputTag);
        Assert.Equal("v1", context.Steps[0].OutputTag);
        Assert.Same(op, context.Steps[0].Operation);
        Assert.Single(context.Steps[0].FeatureSet.Features);
        Assert.Equal(2, context.Steps[0].FeatureSet.NodeFeatures.Count);
    }
}
