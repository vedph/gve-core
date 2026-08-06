using System;
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
        // annotate A in ABC
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
    }

    [Fact]
    public void Execute_At1Run2_TraceFeatures()
    {
        // annotate BC in ABC
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

        // node features target 4 nodes (2 for input and 2 for output)
        Assert.Equal(4, context.NodeFeatures.Count);

        // input nodes:
        // v0_1 (B)
        List<OperationFeature> features = context.NodeFeatures["v0_1"];
        Assert.Single(features);
        // segment
        Assert.Contains(features, f =>
            f.Name == OperationFeature.F_TRACE_SEG_IN &&
            f.Value == $"{op.Id} v0:v1 1");

        // v0_2 (C)
        features = context.NodeFeatures["v0_2"];
        Assert.Single(features);
        Assert.Contains(features, f =>
            f.Name == OperationFeature.F_TRACE_SEG_IN &&
            f.Value == $"{op.Id} v0:v1 2");

        // output nodes:
        // v1_1 (B)
        features = context.NodeFeatures["v1_1"];
        Assert.Equal(2, features.Count);
        // segment
        Assert.Contains(features, f =>
            f.Name == OperationFeature.F_TRACE_SEG_OUT &&
            f.Value == $"{op.Id} v0:v1 1");
        // x (this gets added only to the first node of the segment)
        Assert.Contains(features, f => f.Name == "x" && f.Value == "y");

        // v1_2 (C)
        features = context.NodeFeatures["v1_2"];
        Assert.Single(features);
        // segment
        Assert.Contains(features, f =>
            f.Name == OperationFeature.F_TRACE_SEG_OUT &&
            f.Value == $"{op.Id} v0:v1 2");

        // context steps
        Assert.Single(context.Steps);
        Assert.Equal("v0", context.Steps[0].InputTag);
        Assert.Equal("v1", context.Steps[0].OutputTag);
        Assert.Same(op, context.Steps[0].Operation);
    }

    [Fact]
    public void Execute_OnDeletedText()
    {
        // 123456789-12
        // The red pen.
        ChainOperationContext<char> context = new();
        context.Chain.AppendRange("", "v0",
            TestHelper.GetNodesFrom("The red pen."));

        // replace "red" (nodes 5,6,7) with "blue"
        CharChainReplaceOperation replaceOp = new()
        {
            At = 5,
            Run = 3,
            InputTag = "v0",
            OutputTag = "v1",
            Value = "blue",
        };
        replaceOp.Execute(context);

        Assert.Equal("The blue pen.", TestHelper.GetVersionString(
            context.Chain.GetTagNodes("v1")));

        // annotate "red" by its original node IDs (5,6,7), without pinning
        // the input tag to v0: it defaults to the context's current tag
        // (v1), where "red" has already been replaced by "blue". This must
        // backtrack to v0 to find the nodes instead of throwing.
        CharChainAnnotateOperation annotateOp = new()
        {
            At = 5,
            Run = 3,
            Features = [new OperationFeature("underline", "true")],
        };

        annotateOp.Execute(context);

        // the input tag defaulted to v1 (context.LastTag), and the output
        // tag was auto-generated as v2
        Assert.Equal("v1", context.Steps[^1].InputTag);
        Assert.Equal("v2", context.Steps[^1].OutputTag);
        Assert.Equal("v2", context.LastTag);

        // the current text (v2) is unaffected: annotate never changes text
        Assert.Equal("The blue pen.", TestHelper.GetVersionString(
            context.Chain.GetTagNodes("v2")));

        // the annotation targeted the original "red" nodes (5,6,7)
        Assert.Equal([5, 6, 7], context.Steps[^1].RefNodeIds);

        // each of the "red" nodes got the annotation feature under v2,
        // even though they are not part of v2's visible text
        foreach (int id in new[] { 5, 6, 7 })
        {
            List<OperationFeature> features = context.NodeFeatures[$"v2_{id}"];
            Assert.Contains(features,
                f => f.Name == "underline" && f.Value == "true");
        }
    }

    [Fact]
    public void Execute_OnDeletedText_MultiLevelBacktrack()
    {
        // 123456789-12
        // The red pen.
        ChainOperationContext<char> context = new();
        context.Chain.AppendRange("", "v0",
            TestHelper.GetNodesFrom("The red pen."));

        // v0 -> v1: replace "red" (5,6,7) with "blue"
        new CharChainReplaceOperation
        {
            At = 5,
            Run = 3,
            InputTag = "v0",
            OutputTag = "v1",
            Value = "blue",
        }.Execute(context);

        // v1 -> v2: replace "pen" with "car" (an unrelated edit, to add
        // another version between v0 and the current one)
        new CharChainReplaceOperation
        {
            At = 9,
            Run = 3,
            InputTag = "v1",
            OutputTag = "v2",
            Value = "car",
        }.Execute(context);

        Assert.Equal("The blue car.", TestHelper.GetVersionString(
            context.Chain.GetTagNodes("v2")));

        // annotate "red" (5,6,7) using the default (current) input tag,
        // two versions removed from where "red" still exists (v0)
        CharChainAnnotateOperation annotateOp = new()
        {
            At = 5,
            Run = 3,
            Features = [new OperationFeature("underline", "true")],
        };

        annotateOp.Execute(context);

        Assert.Equal("v2", context.Steps[^1].InputTag);
        Assert.Equal([5, 6, 7], context.Steps[^1].RefNodeIds);

        // current text still unaffected
        Assert.Equal("The blue car.", TestHelper.GetVersionString(
            context.Chain.GetTagNodes(context.LastTag)));
    }

    [Fact]
    public void Execute_OnNeverExistingNodes_Throws()
    {
        ChainOperationContext<char> context = new();
        context.Chain.AppendRange("", "v0", TestHelper.GetNodesFrom("ABC"));

        // node ID 99 never existed in this chain, so no ancestor can ever
        // resolve it: backtracking must exhaust and rethrow
        CharChainAnnotateOperation annotateOp = new()
        {
            At = 99,
            Run = 1,
            InputTag = "v0",
            Features = [new OperationFeature("note", "x")],
        };

        Assert.Throws<InvalidOperationException>(() => annotateOp.Execute(context));
    }
}
