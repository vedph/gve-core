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

        Assert.Equal(OperationType.MoveBefore, op.Type);
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

        Assert.Equal(OperationType.MoveBefore, op.Type);
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

    #region ToAsIndex parsing
    // These cover a DSL parsing bug where a bare "@TO" (index-based TO with
    // nothing following it) was swallowed by the generic "@GROUP" tail
    // marker, since both start with @, leaving nothing for the TO-specific
    // regex to match.
    [Fact]
    public void ParseOperation_At1Run1ToIndex3_NoTail()
    {
        CharChainMoveBeforeOperation op = CharChainOperation
            .Parse<CharChainMoveBeforeOperation>("1>[@3");

        Assert.Equal(OperationType.MoveBefore, op.Type);
        Assert.True(op.ToAsIndex);
        Assert.Equal(3, op.To);
        Assert.Equal(0, op.Rank);
        Assert.Null(op.GroupId);
        Assert.Empty(op.Features);
    }

    [Fact]
    public void ParseOperation_At1Run1ToIndex3_WithGroupAndFeats()
    {
        // a real @GROUP and [FEATS] tail must still be parsed correctly
        // after an index-based TO
        CharChainMoveBeforeOperation op = CharChainOperation
            .Parse<CharChainMoveBeforeOperation>("1>[@3^2@grp [g=1]");

        Assert.True(op.ToAsIndex);
        Assert.Equal(3, op.To);
        Assert.Equal(2, op.Rank);
        Assert.Equal("grp", op.GroupId);
        Assert.Single(op.Features);
        Assert.Equal("g", op.Features[0].Name);
    }

    [Fact]
    public void ToString_ToIndex_RoundTrip()
    {
        CharChainMoveBeforeOperation op = CharChainOperation
            .Parse<CharChainMoveBeforeOperation>("1>[@3");

        CharChainMoveBeforeOperation op2 = CharChainOperation
            .Parse<CharChainMoveBeforeOperation>(op.ToString());

        Assert.Equal(op.ToAsIndex, op2.ToAsIndex);
        Assert.Equal(op.To, op2.To);
        Assert.Equal(op.At, op2.At);
        Assert.Equal(op.Run, op2.Run);
        Assert.Equal(op.AtAsIndex, op2.AtAsIndex);
    }
    #endregion

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
            ],
            NoTraceFeatures = true
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
        List<OperationFeature> features = context.NodeFeatures["v1_1"];
        Assert.NotEmpty(features);
        Assert.Equal("reason", features[0].Name);
        Assert.Equal("A likes C", features[0].Value);
        // context steps
        Assert.Single(context.Steps);
        Assert.Equal("v0", context.Steps[0].InputTag);
        Assert.Equal("v1", context.Steps[0].OutputTag);
        Assert.Same(op, context.Steps[0].Operation);
    }

    [Fact]
    public void Execute_At1Run1To3_TraceFeatures()
    {
        // move A before C in ABC
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
        // input:
        // v0_1 (A)
        List<OperationFeature> features = context.NodeFeatures["v0_1"];
        Assert.Single(features);
        // segment
        Assert.Contains(features, f =>
            f.Name == OperationFeature.F_TRACE_SEG_IN &&
            f.Value == $"{op.Id} v0:v1 1");

        // v0_3 (C)
        features = context.NodeFeatures["v0_3"];
        Assert.Single(features);
        Assert.Contains(features, f =>
            f.Name == OperationFeature.F_TRACE_ANCHOR &&
            f.Value == $"{op.Id} v0:v1");

        // output:
        // v1_1 (A)
        features = context.NodeFeatures["v1_1"];
        Assert.Equal(2, features.Count);
        // reason
        Assert.Contains(features, f => f.Name == "reason" &&
            f.Value == "A likes C");
        // output segment
        Assert.Contains(features, f =>
            f.Name == OperationFeature.F_TRACE_SEG_OUT &&
            f.Value == $"{op.Id} v0:v1 1");

        // context steps
        Assert.Single(context.Steps);
        Assert.Equal("v0", context.Steps[0].InputTag);
        Assert.Equal("v1", context.Steps[0].OutputTag);
        Assert.Same(op, context.Steps[0].Operation);
    }
}
