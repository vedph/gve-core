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
    public void ParseOperation_At1Run2To3Run3Indexes()
    {
        CharChainSwapOperation op = CharChainOperation
            .Parse<CharChainSwapOperation>("@1x2<>3x3");

        Assert.Equal(OperationType.Swap, op.Type);
        Assert.Null(op.InputTag);
        Assert.Null(op.OutputTag);
        Assert.True(op.AtAsIndex);
        Assert.Equal(1, op.At);
        Assert.Equal(2, op.Run);
        Assert.Equal(3, op.To);
        Assert.Equal(3, op.ToRun);
        Assert.Null(op.Value);

        Assert.Equal(0, op.Rank);
        Assert.Null(op.GroupId);
        Assert.Empty(op.Features);
    }

    #region ToAsIndex parsing
    // These cover a DSL parsing bug where a bare "@TO" (index-based TO with
    // nothing following it) was swallowed by the generic "@GROUP" tail
    // marker, since both start with @, leaving nothing for the
    // TO-specific regex to match.
    [Fact]
    public void ParseOperation_ToIndex3Run1_NoTail()
    {
        CharChainSwapOperation op = CharChainOperation
            .Parse<CharChainSwapOperation>("1<>@3");

        Assert.Equal(OperationType.Swap, op.Type);
        Assert.False(op.AtAsIndex);
        Assert.Equal(1, op.At);
        Assert.True(op.ToAsIndex);
        Assert.Equal(3, op.To);
        Assert.Equal(1, op.ToRun);
        Assert.Equal(0, op.Rank);
        Assert.Null(op.GroupId);
        Assert.Empty(op.Features);
    }

    [Fact]
    public void ParseOperation_AtIndexToIndex_BothSides_NoTail()
    {
        CharChainSwapOperation op = CharChainOperation
            .Parse<CharChainSwapOperation>("@8x8<>@17x10");

        Assert.True(op.AtAsIndex);
        Assert.Equal(8, op.At);
        Assert.Equal(8, op.Run);
        Assert.True(op.ToAsIndex);
        Assert.Equal(17, op.To);
        Assert.Equal(10, op.ToRun);
        Assert.Equal(0, op.Rank);
        Assert.Null(op.GroupId);
        Assert.Empty(op.Features);
    }

    [Fact]
    public void ParseOperation_ToIndex3Run2_WithGroupAndFeats()
    {
        // a real @GROUP and [FEATS] tail must still be parsed correctly
        // after an index-based TO
        CharChainSwapOperation op = CharChainOperation
            .Parse<CharChainSwapOperation>("1<>@3x2^2@grp [g=1]");

        Assert.True(op.ToAsIndex);
        Assert.Equal(3, op.To);
        Assert.Equal(2, op.ToRun);
        Assert.Equal(2, op.Rank);
        Assert.Equal("grp", op.GroupId);
        Assert.Single(op.Features);
        Assert.Equal("g", op.Features[0].Name);
    }

    [Fact]
    public void ToString_ToIndex_RoundTrip()
    {
        CharChainSwapOperation op = CharChainOperation
            .Parse<CharChainSwapOperation>("@8x8<>@17x10");

        CharChainSwapOperation op2 = CharChainOperation
            .Parse<CharChainSwapOperation>(op.ToString());

        Assert.Equal(op.AtAsIndex, op2.AtAsIndex);
        Assert.Equal(op.At, op2.At);
        Assert.Equal(op.Run, op2.Run);
        Assert.Equal(op.ToAsIndex, op2.ToAsIndex);
        Assert.Equal(op.To, op2.To);
        Assert.Equal(op.ToRun, op2.ToRun);
    }
    #endregion

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
    }

    [Fact]
    public void Execute_At1Run1To3Run1_TraceFeatures()
    {
        // swap A with C in ABC
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
            f.Name == OperationFeature.F_TRACE_SEG_IN &&
            f.Value == $"{op.Id} v0:v1 1");
        // v0_3 (C)
        features = context.NodeFeatures["v0_3"];
        Assert.Single(features);
        // segment in
        Assert.Contains(features, f =>
            f.Name == OperationFeature.F_TRACE_SEG2_IN &&
            f.Value == $"{op.Id} v0:v1 1");

        // output:
        // v1_1 (C)
        features = context.NodeFeatures["v1_1"];
        Assert.Equal(2, features.Count);
        // reason
        Assert.Contains(features, f => f.Name == "reason" &&
            f.Value == "A and C are jealous");
        // segment out
        Assert.Contains(features, f =>
            f.Name == OperationFeature.F_TRACE_SEG2_OUT &&
            f.Value == $"{op.Id} v0:v1 1");

        // v1_3 (A)
        features = context.NodeFeatures["v1_3"];
        Assert.Equal(2, features.Count);
        // reason
        Assert.Contains(features, f => f.Name == "reason" &&
            f.Value == "A and C are jealous");
        // segment out
        Assert.Contains(features, f =>
            f.Name == OperationFeature.F_TRACE_SEG_OUT &&
            f.Value == $"{op.Id} v0:v1 1");

        // context steps
        Assert.Single(context.Steps);
        Assert.Equal("v0", context.Steps[0].InputTag);
        Assert.Equal("v1", context.Steps[0].OutputTag);
        Assert.Same(op, context.Steps[0].Operation);
    }

    #region ToAsIndex
    // These cover a bug where the TO coordinate of a swap operation was
    // always resolved using AtAsIndex (meant for AT) instead of its own
    // ToAsIndex, so an index-mode TO was silently treated as a node ID
    // (and vice versa).
    [Fact]
    public void Execute_AtById_ToByIndex()
    {
        // ABCD: swap A (id-based AT=1) with D (index-based TO=3)
        ChainOperationContext<char> context = new();
        context.Chain.AppendRange("", "v0", TestHelper.GetNodesFrom("ABCD"));
        CharChainSwapOperation op = new()
        {
            At = 1,
            AtAsIndex = false,
            Run = 1,
            To = 3,
            ToAsIndex = true,
            ToRun = 1,
            InputTag = "v0",
            OutputTag = "v1",
            NoTraceFeatures = true
        };

        op.Execute(context);

        Assert.Equal("DBCA", TestHelper.GetVersionString(
            context.Chain.GetTagNodes("v1")));
    }

    [Fact]
    public void Execute_AtByIndex_ToById_AfterInsertion()
    {
        // AB -> insert X after A => AXB (A=1, B=2, X=3)
        ChainOperationContext<char> context = new();
        context.Chain.AppendRange("", "v0", TestHelper.GetNodesFrom("AB"));
        CharChainAddAfterOperation add = new()
        {
            At = 1,
            InputTag = "v0",
            OutputTag = "v1",
            Value = "X",
            NoTraceFeatures = true
        };
        add.Execute(context);
        Assert.Equal("AXB", TestHelper.GetVersionString(
            context.Chain.GetTagNodes("v1")));

        // swap A (index-based AT=0) with B (id-based TO=2), leaving X in
        // place
        CharChainSwapOperation swap = new()
        {
            At = 0,
            AtAsIndex = true,
            Run = 1,
            To = 2,
            ToAsIndex = false,
            ToRun = 1,
            InputTag = "v1",
            OutputTag = "v2",
            NoTraceFeatures = true
        };
        swap.Execute(context);

        Assert.Equal("BXA", TestHelper.GetVersionString(
            context.Chain.GetTagNodes("v2")));
    }
    #endregion
}
