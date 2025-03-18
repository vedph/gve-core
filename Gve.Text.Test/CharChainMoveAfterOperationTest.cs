using System.Collections.Generic;
using Xunit;

namespace Gve.Text.Test;

public sealed class CharChainMoveAfterOperationTest
{
    [Fact]
    public void ParseOperation_At1Run1To3()
    {
        CharChainMoveAfterOperation op = CharChainOperation
            .Parse<CharChainMoveAfterOperation>("1>]3");

        Assert.Equal(OperationType.MoveAfter, op.Type);
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
        CharChainMoveAfterOperation op = CharChainOperation
            .Parse<CharChainMoveAfterOperation>("1x2>]3");

        Assert.Equal(OperationType.MoveAfter, op.Type);
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
        CharChainMoveAfterOperation op = new()
        {
            At = 1,
            To = 3,
            InputTag = "v0",
            OutputTag = "v1",
            Features =
            [
                OperationFeature.Parse("*log:=\"moved A after C\""),
                new OperationFeature("reason", "A likes C")
            ],
            NoTraceFeatures = true
        };

        op.Execute(context);

        Assert.Equal("BCA", TestHelper.GetVersionString(
            context.Chain.GetTagNodes("v1")));
        // context tag
        Assert.Equal("v1", context.LastTag);
        // context features
        Assert.Single(context.Features);
        Assert.Equal("log", context.Features[0].Name);
        Assert.Equal("moved A after C", context.Features[0].Value);

        // node features
        List<OperationFeature> features = context.NodeFeatures["v1_1"];
        Assert.NotEmpty(features);
        Assert.Equal("reason", features[0].Name);
        Assert.Equal("A likes C", features[0].Value);
        // context steps
        Assert.Single(context.Steps);
        Assert.Equal("v0", context.Steps[0].InputTag);
        Assert.Equal("v1", context.Steps[0].OutputTag);
        Assert.Same(op, context.Steps[0].Operation);
        Assert.Single(context.Steps[0].FeatureSet.Features);
        Assert.Single(context.Steps[0].FeatureSet.NodeFeatures);
    }

    [Fact]
    public void Execute_At1Run1To3_TraceFeatures()
    {
        ChainOperationContext<char> context = new();
        context.Chain.AppendRange("", "v0", TestHelper.GetNodesFrom("ABC"));
        CharChainMoveAfterOperation op = new()
        {
            At = 1,
            To = 3,
            InputTag = "v0",
            OutputTag = "v1",
            Features =
            [
                OperationFeature.Parse("*log:=\"moved A after C\""),
                new OperationFeature("reason", "A likes C")
            ],
        };

        op.Execute(context);

        Assert.Equal("BCA", TestHelper.GetVersionString(
            context.Chain.GetTagNodes("v1")));
        // context tag
        Assert.Equal("v1", context.LastTag);
        // context features
        Assert.Single(context.Features);
        Assert.Equal("log", context.Features[0].Name);
        Assert.Equal("moved A after C", context.Features[0].Value);

        // node features
        // input: v0_3 (C)
        List<OperationFeature> features = context.NodeFeatures["v0_3"];
        Assert.Single(features);
        string anchorFeatName = op.GetTracingFeatureName(
            CharChainOperation.F_TRACE_ANCHOR);
        Assert.Contains(features, f => f.Name == anchorFeatName &&
            f.Value == "v0");

        // output:
        // v0_1 (A)
        // input segment (inherited)
        features = context.NodeFeatures["v0_1"];
        Assert.Single(features);
        string segInFeatName = op.GetTracingFeatureName(
            CharChainOperation.F_TRACE_SEG_IN);
        Assert.Contains(features, f => f.Name == segInFeatName && f.Value == "v0@1");

        // v1_1 (A) 
        features = context.NodeFeatures["v1_1"];
        Assert.Equal(3, features.Count);
        // reason
        Assert.Contains(features, f => f.Name == "reason" &&
            f.Value == "A likes C");
        // input segment (inherited)
        Assert.Contains(features, f => f.Name == segInFeatName && f.Value == "v0@1");
        // output segment
        string segOutFeatName = op.GetTracingFeatureName(
            CharChainOperation.F_TRACE_SEG_OUT);
        Assert.Contains(features, f => f.Name == segOutFeatName && f.Value == "v1@1");

        // context steps
        Assert.Single(context.Steps);
        Assert.Equal("v0", context.Steps[0].InputTag);
        Assert.Equal("v1", context.Steps[0].OutputTag);
        Assert.Same(op, context.Steps[0].Operation);
        Assert.Single(context.Steps[0].FeatureSet.Features);
        Assert.Equal(4, context.Steps[0].FeatureSet.NodeFeatures.Count);
    }
}
