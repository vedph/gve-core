using System.Collections.Generic;
using Xunit;

namespace Gve.Text.Test;

public sealed class CharChainDeleteOperationTest
{
    #region Parsing
    [Fact]
    public void ParseOperation_At1Run1()
    {
        CharChainDeleteOperation op = CharChainOperation
            .Parse<CharChainDeleteOperation>("1x1-");

        Assert.Equal(OperationType.Delete, op.Type);
        Assert.Null(op.InputTag);
        Assert.Null(op.OutputTag);
        Assert.False(op.AtAsIndex);
        Assert.Equal(1, op.At);
        Assert.Equal(1, op.Run);
        Assert.Null(op.Value);

        Assert.Equal(0, op.Rank);
        Assert.Null(op.GroupId);
        Assert.Empty(op.Features);
    }

    [Fact]
    public void ParseOperation_AtIndex1Run2()
    {
        CharChainDeleteOperation op = CharChainOperation
            .Parse<CharChainDeleteOperation>("@1x2-");

        Assert.Equal(OperationType.Delete, op.Type);
        Assert.Null(op.InputTag);
        Assert.Null(op.OutputTag);
        Assert.True(op.AtAsIndex);
        Assert.Equal(1, op.At);
        Assert.Equal(2, op.Run);
        Assert.Null(op.Value);

        Assert.Equal(0, op.Rank);
        Assert.Null(op.GroupId);
        Assert.Empty(op.Features);
    }

    [Fact]
    public void ParseOperation_VerAt1Run1()
    {
        CharChainDeleteOperation op = CharChainOperation
            .Parse<CharChainDeleteOperation>("(v0:v1)1x1-");

        Assert.Equal(OperationType.Delete, op.Type);
        Assert.Equal("v0", op.InputTag);
        Assert.Equal("v1", op.OutputTag);
        Assert.False(op.AtAsIndex);
        Assert.Equal(1, op.At);
        Assert.Equal(1, op.Run);
        Assert.Null(op.Value);

        Assert.Equal(0, op.Rank);
        Assert.Null(op.GroupId);
        Assert.Empty(op.Features);
    }

    [Fact]
    public void ParseOperation_VerAt1Run1Rank()
    {
        CharChainDeleteOperation op = CharChainOperation
            .Parse<CharChainDeleteOperation>("(v0:v1)1x1-^2");

        Assert.Equal(OperationType.Delete, op.Type);
        Assert.Equal("v0", op.InputTag);
        Assert.Equal("v1", op.OutputTag);
        Assert.False(op.AtAsIndex);
        Assert.Equal(1, op.At);
        Assert.Equal(1, op.Run);
        Assert.Null(op.Value);

        Assert.Equal(2, op.Rank);
        Assert.Null(op.GroupId);
        Assert.Empty(op.Features);
    }

    [Fact]
    public void ParseOperation_VerAt1Run1Group()
    {
        CharChainDeleteOperation op = CharChainOperation
            .Parse<CharChainDeleteOperation>("(v0:v1)1x1-@g1");

        Assert.Equal(OperationType.Delete, op.Type);
        Assert.Equal("v0", op.InputTag);
        Assert.Equal("v1", op.OutputTag);
        Assert.False(op.AtAsIndex);
        Assert.Equal(1, op.At);
        Assert.Equal(1, op.Run);
        Assert.Null(op.Value);

        Assert.Equal(0, op.Rank);
        Assert.Equal("g1", op.GroupId);
        Assert.Empty(op.Features);
    }

    [Fact]
    public void ParseOperation_VerAt1Run1Meta()
    {
        CharChainDeleteOperation op = CharChainOperation
            .Parse<CharChainDeleteOperation>("(v0:v1)1x1- [note=sample]");

        Assert.Equal(OperationType.Delete, op.Type);
        Assert.Equal("v0", op.InputTag);
        Assert.Equal("v1", op.OutputTag);
        Assert.False(op.AtAsIndex);
        Assert.Equal(1, op.At);
        Assert.Equal(1, op.Run);
        Assert.Null(op.Value);

        Assert.Equal(0, op.Rank);
        Assert.Null(op.GroupId);
        Assert.Single(op.Features);
        Assert.Equal("note", op.Features[0].Name);
        Assert.Equal("sample", op.Features[0].Value);
        Assert.Equal(FeatureSetPolicy.Multiple, op.Features[0].SetPolicy);
    }
    #endregion

    [Fact]
    public void Execute_DeleteSingle()
    {
        ChainOperationContext<char> context = new();
        context.Chain.AppendRange("", "v0", TestHelper.GetNodesFrom("ABC"));
        CharChainDeleteOperation op = new()
        {
            At = 1,
            Run = 1,
            InputTag = "v0",
            OutputTag = "v1",
            Features =
            [
                OperationFeature.Parse("*log:=\"deleted A from ABC\""),
                new OperationFeature("reason", "don't like A")
            ],
            NoTraceFeatures = true
        };

        op.Execute(context);

        Assert.Equal("BC", TestHelper.GetVersionString(
            context.Chain.GetTagNodes("v1")));
        // context tag
        Assert.Equal("v1", context.LastTag);
        // context features
        Assert.Single(context.Features);
        Assert.Equal("log", context.Features[0].Name);
        Assert.Equal("deleted A from ABC", context.Features[0].Value);
        // node features
        List<OperationFeature> features = context.NodeFeatures["v1_1"];
        Assert.NotEmpty(features);
        Assert.Equal("reason", features[0].Name);
        Assert.Equal("don't like A", features[0].Value);
        // context steps
        Assert.Single(context.Steps);
        Assert.Equal("v0", context.Steps[0].InputTag);
        Assert.Equal("v1", context.Steps[0].OutputTag);
        Assert.Same(op, context.Steps[0].Operation);
        Assert.Single(context.Steps[0].FeatureSet.Features);
        Assert.Single(context.Steps[0].FeatureSet.NodeFeatures);
    }

    [Fact]
    public void Execute_DeleteRange()
    {
        ChainOperationContext<char> context = new();
        context.Chain.AppendRange("", "v0", TestHelper.GetNodesFrom("ABC"));
        CharChainDeleteOperation op = new()
        {
            At = 2,
            Run = 2,
            InputTag = "v0",
            OutputTag = "v1",
            Features =
            [
                OperationFeature.Parse("*log:=\"deleted BC from ABC\""),
                new OperationFeature("reason", "don't like BC")
            ],
            NoTraceFeatures = true
        };

        op.Execute(context);

        Assert.Equal("A", TestHelper.GetVersionString(
            context.Chain.GetTagNodes("v1")));
        // context tag
        Assert.Equal("v1", context.LastTag);
        // context features
        Assert.Single(context.Features);
        Assert.Equal("log", context.Features[0].Name);
        Assert.Equal("deleted BC from ABC", context.Features[0].Value);
        // node features
        List<OperationFeature> features = context.NodeFeatures["v1_2"];
        Assert.NotEmpty(features);
        Assert.Equal("reason", features[0].Name);
        Assert.Equal("don't like BC", features[0].Value);

        features = context.NodeFeatures["v1_3"];
        Assert.NotEmpty(features);
        Assert.Equal("reason", features[0].Name);
        Assert.Equal("don't like BC", features[0].Value);

        // context steps
        Assert.Single(context.Steps);
        Assert.Equal("v0", context.Steps[0].InputTag);
        Assert.Equal("v1", context.Steps[0].OutputTag);
        Assert.Same(op, context.Steps[0].Operation);
        Assert.Single(context.Steps[0].FeatureSet.Features);
        Assert.Equal(2, context.Steps[0].FeatureSet.NodeFeatures.Count);
    }

    [Fact]
    public void Execute_DeleteRange_TraceFeatures()
    {
        ChainOperationContext<char> context = new();
        context.Chain.AppendRange("", "v0", TestHelper.GetNodesFrom("ABC"));
        CharChainDeleteOperation op = new()
        {
            At = 2,
            Run = 2,
            InputTag = "v0",
            OutputTag = "v1",
            Features =
            [
                OperationFeature.Parse("*log:=\"deleted BC from ABC\""),
                new OperationFeature("reason", "don't like BC")
            ],
        };

        op.Execute(context);

        Assert.Equal("A", TestHelper.GetVersionString(
            context.Chain.GetTagNodes("v1")));
        // context tag
        Assert.Equal("v1", context.LastTag);
        // context features
        Assert.Single(context.Features);
        Assert.Equal("log", context.Features[0].Name);
        Assert.Equal("deleted BC from ABC", context.Features[0].Value);

        // node features
        Assert.Equal(4, context.NodeFeatures.Count);
        string segInName = op.GetTraceFeatureName(
            CharChainOperation.F_TRACE_SEG_IN);

        // input nodes:
        // v0_2
        List<OperationFeature> features = context.NodeFeatures["v0_2"];
        Assert.Single(features);
        // segment
        Assert.Contains(features, f => f.Name == segInName && f.Value == "v0@1");

        // v0_3
        features = context.NodeFeatures["v0_3"];
        Assert.Single(features);
        // segment
        Assert.Contains(features, f => f.Name == segInName && f.Value == "v0@2");

        // output nodes:
        // v1_2
        features = context.NodeFeatures["v1_2"];
        Assert.Equal(2, features.Count);
        // reason
        Assert.Contains(features, f => f.Name == "reason" && f.Value == "don't like BC");
        // segment (inherited from v0)
        Assert.Contains(features, f => f.Name == segInName && f.Value == "v0@1");

        // v1_3
        features = context.NodeFeatures["v1_3"];
        Assert.Equal(2, features.Count);
        // reason
        Assert.Contains(features, f => f.Name == "reason" && f.Value == "don't like BC");
        // segment (inherited from v0)
        Assert.Contains(features, f => f.Name == segInName && f.Value == "v0@2");

        // context steps
        Assert.Single(context.Steps);
        Assert.Equal("v0", context.Steps[0].InputTag);
        Assert.Equal("v1", context.Steps[0].OutputTag);
        Assert.Same(op, context.Steps[0].Operation);
        Assert.Single(context.Steps[0].FeatureSet.Features);
        Assert.Equal(4, context.Steps[0].FeatureSet.NodeFeatures.Count);
    }
}
