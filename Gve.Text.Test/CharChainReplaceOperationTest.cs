using System.Collections.Generic;
using System;
using Xunit;

namespace Gve.Text.Test;

public sealed class CharChainReplaceOperationTest
{
    #region Parsing
    [Fact]
    public void ParseOperation_At1Run1()
    {
        CharChainReplaceOperation op = CharChainOperation
            .Parse<CharChainReplaceOperation>("1x1=X");

        Assert.Equal(VarOperationType.Replace, op.Type);
        Assert.Null(op.InputTag);
        Assert.Null(op.OutputTag);
        Assert.False(op.AtAsIndex);
        Assert.Equal(1, op.At);
        Assert.Equal(1, op.Run);
        Assert.Equal("X", op.Value);

        Assert.Equal(0, op.Rank);
        Assert.Null(op.GroupId);
        Assert.Empty(op.Features);
    }

    [Fact]
    public void ParseOperation_AtIndex1Run2()
    {
        CharChainReplaceOperation op = CharChainOperation
            .Parse<CharChainReplaceOperation>("@1x2=X");

        Assert.Equal(VarOperationType.Replace, op.Type);
        Assert.Null(op.InputTag);
        Assert.Null(op.OutputTag);
        Assert.True(op.AtAsIndex);
        Assert.Equal(1, op.At);
        Assert.Equal(2, op.Run);
        Assert.Equal("X", op.Value);

        Assert.Equal(0, op.Rank);
        Assert.Null(op.GroupId);
        Assert.Empty(op.Features);
    }

    [Fact]
    public void ParseOperation_VerAt1Run1()
    {
        CharChainReplaceOperation op = CharChainOperation
            .Parse<CharChainReplaceOperation>("(v0:v1)1x1=X");

        Assert.Equal(VarOperationType.Replace, op.Type);
        Assert.Equal("v0", op.InputTag);
        Assert.Equal("v1", op.OutputTag);
        Assert.False(op.AtAsIndex);
        Assert.Equal(1, op.At);
        Assert.Equal(1, op.Run);
        Assert.Equal("X", op.Value);

        Assert.Equal(0, op.Rank);
        Assert.Null(op.GroupId);
        Assert.Empty(op.Features);
    }
    #endregion

    [Fact]
    public void Execute_ReplaceSingle()
    {
        ChainOperationContext<char> context = new();
        context.Chain.AppendRange("", "v0", TestHelper.GetNodesFrom("ABC"));
        CharChainReplaceOperation op = new()
        {
            At = 1,
            Run = 1,
            InputTag = "v0",
            OutputTag = "v1",
            Value = "X",
            Features =
            [
                new Feature("*log", "replaced A with X"),
                new Feature("reason", "like X more")
            ]
        };

        op.Execute(context);

        Assert.Equal("XBC", TestHelper.GetVersionString(
            context.Chain.GetTagNodes("v1")));
        // context tag
        Assert.Equal("v1", context.LastTag);
        // context features
        Assert.Single(context.Features);
        Assert.Equal("log", context.Features[0].Name);
        Assert.Equal("replaced A with X", context.Features[0].Value);
        // node features
        List<Feature> nodeFeatures = context.NodeFeatures[Tuple.Create("v1", 4)];
        Assert.NotEmpty(nodeFeatures);
        Assert.Equal("reason", nodeFeatures[0].Name);
        Assert.Equal("like X more", nodeFeatures[0].Value);
        // context steps
        Assert.Single(context.Steps);
        Assert.Equal("v0", context.Steps[0].InputTag);
        Assert.Equal("v1", context.Steps[0].OutputTag);
        Assert.Same(op, context.Steps[0].Operation);
        Assert.Single(context.Steps[0].FeatureSet.Features);
        Assert.Single(context.Steps[0].FeatureSet.NodeFeatures);
    }

    [Fact]
    public void Execute_ReplaceRange()
    {
        ChainOperationContext<char> context = new();
        context.Chain.AppendRange("", "v0", TestHelper.GetNodesFrom("ABC"));
        CharChainReplaceOperation op = new()
        {
            At = 1,
            Run = 2,
            InputTag = "v0",
            OutputTag = "v1",
            Value = "X",
            Features =
            [
                new Feature("*log", "replaced AB with X"),
                new Feature("reason", "like X more")
            ]
        };

        op.Execute(context);

        Assert.Equal("XC", TestHelper.GetVersionString(
            context.Chain.GetTagNodes("v1")));
        // context tag
        Assert.Equal("v1", context.LastTag);
        // context features
        Assert.Single(context.Features);
        Assert.Equal("log", context.Features[0].Name);
        Assert.Equal("replaced AB with X", context.Features[0].Value);
        // node features
        List<Feature> nodeFeatures = context.NodeFeatures[Tuple.Create("v1", 4)];
        Assert.NotEmpty(nodeFeatures);
        Assert.Equal("reason", nodeFeatures[0].Name);
        Assert.Equal("like X more", nodeFeatures[0].Value);

        // context steps
        Assert.Single(context.Steps);
        Assert.Equal("v0", context.Steps[0].InputTag);
        Assert.Equal("v1", context.Steps[0].OutputTag);
        Assert.Same(op, context.Steps[0].Operation);
        Assert.Single(context.Steps[0].FeatureSet.Features);
        Assert.Single(context.Steps[0].FeatureSet.NodeFeatures);
    }
}
