using System;
using System.Collections.Generic;
using Xunit;

namespace Gve.Text.Test;

public sealed class CharChainSwapOperationTest
{
    [Fact]
    public void ParseOperation_At1Run1To3()
    {
        CharChainSwapOperation op = CharChainOperation
            .Parse<CharChainSwapOperation>("1<>3");

        Assert.Equal(OperationType.Swap, op.Type);
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
    }

    [Fact]
    public void ParseOperation_At1Run2To3Run3()
    {
        CharChainSwapOperation op = CharChainOperation
            .Parse<CharChainSwapOperation>("1x2<>3x3");

        Assert.Equal(OperationType.Swap, op.Type);
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
                OperationFeature.Parse("*log:=\"swapped A with C\""),
                new OperationFeature("reason", "A and C are jealous")
            ],
            NoTraceFeatures = true
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
        List<OperationFeature> features = context.NodeFeatures["v1_1"];
        Assert.NotEmpty(features);
        Assert.Equal("reason", features[0].Name);
        Assert.Equal("A and C are jealous", features[0].Value);

        features = context.NodeFeatures["v1_3"];
        Assert.NotEmpty(features);
        Assert.Equal("reason", features[0].Name);
        Assert.Equal("A and C are jealous", features[0].Value);

        // context steps
        Assert.Single(context.Steps);
        Assert.Equal("v0", context.Steps[0].InputTag);
        Assert.Equal("v1", context.Steps[0].OutputTag);
        Assert.Same(op, context.Steps[0].Operation);
        Assert.Single(context.Steps[0].FeatureSet.Features);
        Assert.Equal(2, context.Steps[0].FeatureSet.NodeFeatures.Count);
    }

    [Fact]
    public void Execute_At1Run1To3Run1_TraceFeatures()
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
                OperationFeature.Parse("*log:=\"swapped A with C\""),
                new OperationFeature("reason", "A and C are jealous")
            ],
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
        // input:
        // v0_1 (A)
        List<OperationFeature> features = context.NodeFeatures["v0_1"];
        Assert.Single(features);
        // segment in
        Assert.Contains(features, f =>
            f.Name == OperationFeature.F_TRACE_SEG_IN && f.Value == "v0@1");
        // v0_3 (C)
        features = context.NodeFeatures["v0_3"];
        Assert.Single(features);
        // segment in
        Assert.Contains(features, f =>
            f.Name == OperationFeature.F_TRACE_SEG_IN && f.Value == "v0@1");

        // output:
        // v1_1 (C)
        features = context.NodeFeatures["v1_1"];
        Assert.Equal(3, features.Count);
        // reason
        Assert.Contains(features, f => f.Name == "reason" &&
            f.Value == "A and C are jealous");
        // segment in (inherited)
        Assert.Contains(features, f => f.Name == OperationFeature.F_TRACE_SEG_IN &&
            f.Value == "v0@1");
        // segment out
        Assert.Contains(features, f =>
            f.Name == OperationFeature.F_TRACE_SEG_OUT && f.Value == "v1@1");

        // v1_3 (A)
        features = context.NodeFeatures["v1_3"];
        Assert.Equal(3, features.Count);
        // reason
        Assert.Contains(features, f => f.Name == "reason" &&
            f.Value == "A and C are jealous");
        // segment in (inherited)
        Assert.Contains(features, f =>
            f.Name == OperationFeature.F_TRACE_SEG_OUT && f.Value == "v0@1");
        // segment out
        Assert.Contains(features, f =>
            f.Name == OperationFeature.F_TRACE_SEG_OUT && f.Value == "v1@1");

        // context steps
        Assert.Single(context.Steps);
        Assert.Equal("v0", context.Steps[0].InputTag);
        Assert.Equal("v1", context.Steps[0].OutputTag);
        Assert.Same(op, context.Steps[0].Operation);
        Assert.Single(context.Steps[0].FeatureSet.Features);
        Assert.Equal(4, context.Steps[0].FeatureSet.NodeFeatures.Count);
    }
}
