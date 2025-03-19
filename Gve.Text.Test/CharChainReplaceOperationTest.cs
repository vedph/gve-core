using System.Collections.Generic;
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

        Assert.Equal(OperationType.Replace, op.Type);
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

        Assert.Equal(OperationType.Replace, op.Type);
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
    public void ParseOperation_AtIndex1Run2Feats()
    {
        CharChainReplaceOperation op = CharChainOperation
            .Parse<CharChainReplaceOperation>("@1x2=X [*g=1 *x:=2 !*z]");

        Assert.Equal(OperationType.Replace, op.Type);
        Assert.Null(op.InputTag);
        Assert.Null(op.OutputTag);
        Assert.True(op.AtAsIndex);
        Assert.Equal(1, op.At);
        Assert.Equal(2, op.Run);
        Assert.Equal("X", op.Value);

        Assert.Equal(0, op.Rank);
        Assert.Null(op.GroupId);
        Assert.Equal(3, op.Features.Count);
        Assert.Equal("g", op.Features[0].Name);
        Assert.True(op.Features[0].IsGlobal);
        Assert.Equal("1", op.Features[0].Value);
        Assert.Equal("x", op.Features[1].Name);
        Assert.True(op.Features[1].IsGlobal);
        Assert.Equal("2", op.Features[1].Value);
        Assert.Equal("z", op.Features[2].Name);
        Assert.True(op.Features[2].IsNegated);
        Assert.True(op.Features[2].IsGlobal);
        Assert.Null(op.Features[2].Value);
    }

    [Fact]
    public void ParseOperation_VerAt1Run1()
    {
        CharChainReplaceOperation op = CharChainOperation
            .Parse<CharChainReplaceOperation>("(v0:v1)1x1=X");

        Assert.Equal(OperationType.Replace, op.Type);
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
                OperationFeature.Parse("*log:=\"replaced A with X\""),
                new OperationFeature("reason", "like X more")
            ],
            NoTraceFeatures = true
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
        List<OperationFeature> nodeFeatures = context.NodeFeatures["v1_4"];
        Assert.Equal(2, nodeFeatures.Count);
        // opid
        OperationFeature? f = nodeFeatures.Find(f => f.Name ==
            OperationMetadata.OPID_KEY);
        Assert.NotNull(f);
        Assert.Equal(op.Id, f.Value);
        // reason
        f = nodeFeatures.Find(f => f.Name == "reason");
        Assert.NotNull(f);
        Assert.Equal("like X more", f.Value);
        // context steps
        Assert.Single(context.Steps);
        Assert.Equal("v0", context.Steps[0].InputTag);
        Assert.Equal("v1", context.Steps[0].OutputTag);
        Assert.Same(op, context.Steps[0].Operation);
        Assert.Single(context.Steps[0].FeatureSet.Features);
        Assert.Single(context.Steps[0].FeatureSet.NodeFeatures);
    }

    [Fact]
    public void Execute_ReplaceSingleWithFeats()
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
                OperationFeature.Parse("*log:=\"replaced A with X\""),
                OperationFeature.Parse("x:=\"25\""),
                OperationFeature.Parse("y:=\"50\""),
            ],
            NoTraceFeatures = true
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
        List<OperationFeature> nodeFeatures = context.NodeFeatures["v1_4"];
        Assert.Equal(3, nodeFeatures.Count);
        // opid
        OperationFeature? f = nodeFeatures.Find(f => f.Name ==
            OperationMetadata.OPID_KEY);
        Assert.NotNull(f);
        Assert.Equal(op.Id, f.Value);
        // x
        f = nodeFeatures.Find(f => f.Name == "x");
        Assert.NotNull(f);
        Assert.Equal("25", f.Value);
        // y
        f = nodeFeatures.Find(f => f.Name == "y");
        Assert.NotNull(f);
        Assert.Equal("50", f.Value);
        // context steps
        Assert.Single(context.Steps);
        Assert.Equal("v0", context.Steps[0].InputTag);
        Assert.Equal("v1", context.Steps[0].OutputTag);
        Assert.Same(op, context.Steps[0].Operation);
        Assert.Single(context.Steps[0].FeatureSet.Features);
        Assert.Single(context.Steps[0].FeatureSet.NodeFeatures);
    }

    [Fact]
    public void Execute_ReplaceRange_Shorter()
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
                OperationFeature.Parse("*log:=\"replaced AB with X\""),
                new OperationFeature("reason", "like X more")
            ],
            NoTraceFeatures = true
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
        List<OperationFeature> nodeFeatures = context.NodeFeatures["v1_4"];
        Assert.Equal(2, nodeFeatures.Count);
        // opid
        OperationFeature? f = nodeFeatures.Find(f => f.Name ==
            OperationMetadata.OPID_KEY);
        Assert.NotNull(f);
        Assert.Equal(op.Id, f.Value);
        // reason
        f = nodeFeatures.Find(f => f.Name == "reason");
        Assert.NotNull(f);
        Assert.Equal("like X more", f.Value);

        // context steps
        Assert.Single(context.Steps);
        Assert.Equal("v0", context.Steps[0].InputTag);
        Assert.Equal("v1", context.Steps[0].OutputTag);
        Assert.Same(op, context.Steps[0].Operation);
        Assert.Single(context.Steps[0].FeatureSet.Features);
        Assert.Single(context.Steps[0].FeatureSet.NodeFeatures);
    }

    [Fact]
    public void Execute_ReplaceRange_Longer()
    {
        ChainOperationContext<char> context = new();
        context.Chain.AppendRange("", "v0", TestHelper.GetNodesFrom("ABC"));
        CharChainReplaceOperation op = new()
        {
            At = 2,
            Run = 1,
            InputTag = "v0",
            OutputTag = "v1",
            Value = "XY",
            Features =
            [
                OperationFeature.Parse("*log:=\"replaced B with XY\""),
                new OperationFeature("reason", "like XY more")
            ],
            NoTraceFeatures = true
        };

        op.Execute(context);

        Assert.Equal("AXYC", TestHelper.GetVersionString(
            context.Chain.GetTagNodes("v1")));
        // context tag
        Assert.Equal("v1", context.LastTag);
        // context features
        Assert.Single(context.Features);
        Assert.Equal("log", context.Features[0].Name);
        Assert.Equal("replaced B with XY", context.Features[0].Value);

        // node features
        List<OperationFeature> nodeFeatures = context.NodeFeatures["v1_4"];
        Assert.Equal(2, nodeFeatures.Count);
        // opid
        OperationFeature? f = nodeFeatures.Find(f => f.Name ==
            OperationMetadata.OPID_KEY);
        Assert.NotNull(f);
        Assert.Equal(op.Id, f.Value);
        // reason
        f = nodeFeatures.Find(f => f.Name == "reason");
        Assert.NotNull(f);
        Assert.Equal("like XY more", f.Value);

        nodeFeatures = context.NodeFeatures["v1_5"];
        Assert.Equal(2, nodeFeatures.Count);
        // opid
        f = nodeFeatures.Find(f => f.Name == OperationMetadata.OPID_KEY);
        Assert.NotNull(f);
        Assert.Equal(op.Id, f.Value);
        // reason
        f = nodeFeatures.Find(f => f.Name == "reason");
        Assert.NotNull(f);
        Assert.Equal("like XY more", f.Value);

        // context steps
        Assert.Single(context.Steps);
        Assert.Equal("v0", context.Steps[0].InputTag);
        Assert.Equal("v1", context.Steps[0].OutputTag);
        Assert.Same(op, context.Steps[0].Operation);
        Assert.Single(context.Steps[0].FeatureSet.Features);
        Assert.Equal(2, context.Steps[0].FeatureSet.NodeFeatures.Count);
    }

    [Fact]
    public void Execute_ReplaceRange_TraceFeatures()
    {
        // replace B with XY in ABC
        ChainOperationContext<char> context = new();
        context.Chain.AppendRange("", "v0", TestHelper.GetNodesFrom("ABC"));
        CharChainReplaceOperation op = new()
        {
            At = 2,
            Run = 1,
            InputTag = "v0",
            OutputTag = "v1",
            Value = "XY",
            Features =
            [
                OperationFeature.Parse("*log:=\"replaced B with XY\""),
                    new OperationFeature("reason", "like XY more")
            ]
        };

        op.Execute(context);

        Assert.Equal("AXYC", TestHelper.GetVersionString(
            context.Chain.GetTagNodes("v1")));
        // context tag
        Assert.Equal("v1", context.LastTag);
        // context features
        Assert.Single(context.Features);
        Assert.Equal("log", context.Features[0].Name);
        Assert.Equal("replaced B with XY", context.Features[0].Value);

        // node features
        // input nodes
        // v0_2 (B)
        List<OperationFeature> features = context.NodeFeatures["v0_2"];
        Assert.Single(features);
        // segment
        Assert.Contains(features, f =>
            f.Name == OperationFeature.F_TRACE_SEG_IN &&
            f.Value == $"{op.Id} v0:v1 1");

        // output nodes
        // v1_4 (X)
        features = context.NodeFeatures["v1_4"];
        Assert.Equal(3, features.Count);
        // opid
        OperationFeature? f = features.Find(f => f.Name ==
            OperationMetadata.OPID_KEY);
        Assert.NotNull(f);
        Assert.Equal(op.Id, f.Value);
        // reason
        f = features.Find(f => f.Name == "reason");
        Assert.NotNull(f);
        Assert.Equal("like XY more", f.Value);
        // segment
        Assert.Contains(features, f =>
            f.Name == OperationFeature.F_TRACE_SEG_OUT &&
            f.Value == $"{op.Id} v0:v1 1");

        // v1_5 (Y)
        features = context.NodeFeatures["v1_5"];
        Assert.Equal(3, features.Count);
        // opid
        f = features.Find(f => f.Name == OperationMetadata.OPID_KEY);
        Assert.NotNull(f);
        Assert.Equal(op.Id, f.Value);
        // reason
        f = features.Find(f => f.Name == "reason");
        Assert.NotNull(f);
        Assert.Equal("like XY more", f.Value);
        // segment
        Assert.Contains(features, f =>
            f.Name == OperationFeature.F_TRACE_SEG_OUT &&
            f.Value == $"{op.Id} v0:v1 2");

        // context steps
        Assert.Single(context.Steps);
        Assert.Equal("v0", context.Steps[0].InputTag);
        Assert.Equal("v1", context.Steps[0].OutputTag);
        Assert.Same(op, context.Steps[0].Operation);
        Assert.Single(context.Steps[0].FeatureSet.Features);
        Assert.Equal(3, context.Steps[0].FeatureSet.NodeFeatures.Count);
    }

    [Fact]
    public void Execute_Index_Linear_Equal()
    {
        ChainOperationContext<char> context = new();
        context.Chain.AppendRange("", "v0", TestHelper.GetNodesFrom("ABC"));
        CharChainReplaceOperation op = new()
        {
            At = 1,
            AtAsIndex = true,
            Run = 1,
            InputTag = "v0",
            OutputTag = "v1",
            Value = "X",
            Features =
            [
                OperationFeature.Parse("*log:=\"replaced B with X\""),
                new OperationFeature("reason", "like X more")
            ]
        };

        op.Execute(context);

        Assert.Equal("AXC", TestHelper.GetVersionString(
            context.Chain.GetTagNodes("v1")));
    }

    [Fact]
    public void Execute_Index_Linear_Shorter()
    {
        ChainOperationContext<char> context = new();
        context.Chain.AppendRange("", "v0", TestHelper.GetNodesFrom("ABC"));
        CharChainReplaceOperation op = new()
        {
            At = 1,
            AtAsIndex = true,
            Run = 2,
            InputTag = "v0",
            OutputTag = "v1",
            Value = "X",
            Features =
            [
                OperationFeature.Parse("*log:=\"replaced BC with X\""),
                new OperationFeature("reason", "like X more")
            ]
        };

        op.Execute(context);

        Assert.Equal("AX", TestHelper.GetVersionString(
            context.Chain.GetTagNodes("v1")));
    }

    [Fact]
    public void Execute_Index_Linear_Longer()
    {
        ChainOperationContext<char> context = new();
        context.Chain.AppendRange("", "v0", TestHelper.GetNodesFrom("ABC"));
        CharChainReplaceOperation op = new()
        {
            At = 1,
            AtAsIndex = true,
            Run = 1,
            InputTag = "v0",
            OutputTag = "v1",
            Value = "XY",
            Features =
            [
                OperationFeature.Parse("*log:=\"replaced B with XY\""),
                new OperationFeature("reason", "like XY more")
            ]
        };

        op.Execute(context);

        Assert.Equal("AXYC", TestHelper.GetVersionString(
            context.Chain.GetTagNodes("v1")));
    }

    [Fact]
    public void Execute_Index_NonLinear_Equal()
    {
        ChainOperationContext<char> context = new();
        context.Chain.AppendRange("", "v0", TestHelper.GetNodesFrom("ABC"));
        CharChainReplaceOperation op1 = new()
        {
            At = 2,
            Run = 1,
            InputTag = "v0",
            OutputTag = "v1",
            Value = "X",
            Features =
            [
                OperationFeature.Parse("*log:=\"replaced B with X\""),
            ]
        };
        op1.Execute(context);
        Assert.Equal("AXC", TestHelper.GetVersionString(
            context.Chain.GetTagNodes("v1")));

        CharChainReplaceOperation op2 = new()
        {
            At = 0,
            AtAsIndex = true,
            Run = 2,
            InputTag = "v1",
            OutputTag = "v2",
            Value = "ax",
            Features =
            [
                OperationFeature.Parse("*log:=\"replaced AX with ax\""),
            ]
        };
        op2.Execute(context);
        Assert.Equal("axC", TestHelper.GetVersionString(
            context.Chain.GetTagNodes("v2")));
    }

    [Fact]
    public void Execute_Index_NonLinear_Shorter()
    {
        ChainOperationContext<char> context = new();
        context.Chain.AppendRange("", "v0", TestHelper.GetNodesFrom("ABC"));
        CharChainReplaceOperation op1 = new()
        {
            At = 2,
            Run = 1,
            InputTag = "v0",
            OutputTag = "v1",
            Value = "X",
            Features =
            [
                OperationFeature.Parse("*log:=\"replaced B with X\""),
            ]
        };
        op1.Execute(context);
        Assert.Equal("AXC", TestHelper.GetVersionString(
            context.Chain.GetTagNodes("v1")));

        CharChainReplaceOperation op2 = new()
        {
            At = 0,
            AtAsIndex = true,
            Run = 2,
            InputTag = "v1",
            OutputTag = "v2",
            Value = "x",
            Features =
            [
                OperationFeature.Parse("*log:=\"replaced AX with x\""),
            ]
        };
        op2.Execute(context);
        Assert.Equal("xC", TestHelper.GetVersionString(
            context.Chain.GetTagNodes("v2")));
    }

    [Fact]
    public void Execute_Index_NonLinear_Longer()
    {
        ChainOperationContext<char> context = new();
        context.Chain.AppendRange("", "v0", TestHelper.GetNodesFrom("ABC"));
        CharChainReplaceOperation op1 = new()
        {
            At = 2,
            Run = 1,
            InputTag = "v0",
            OutputTag = "v1",
            Value = "X",
            Features =
            [
                OperationFeature.Parse("*log:=\"replaced B with X\""),
            ]
        };
        op1.Execute(context);
        Assert.Equal("AXC", TestHelper.GetVersionString(
            context.Chain.GetTagNodes("v1")));

        CharChainReplaceOperation op2 = new()
        {
            At = 1,
            AtAsIndex = true,
            Run = 2,
            InputTag = "v1",
            OutputTag = "v2",
            Value = "xy",
            Features =
            [
                OperationFeature.Parse("*log:=\"replaced X with xy\""),
            ]
        };
        op2.Execute(context);
        Assert.Equal("Axy", TestHelper.GetVersionString(
            context.Chain.GetTagNodes("v2")));
    }

    [Fact]
    public void Execute_Replacing_Tail()
    {
        ChainOperationContext<char> context = new();
        context.Chain.AppendRange("", "v0", TestHelper.GetNodesFrom("say"));
        CharChainReplaceOperation op = new()
        {
            At = 3,
            Run = 1,
            InputTag = "v0",
            OutputTag = "v1",
            Value = "id",
            Features =
            [
                OperationFeature.Parse("*log:=\"replaced say with said\""),
            ]
        };
        op.Execute(context);
        Assert.Equal("said", TestHelper.GetVersionString(
            context.Chain.GetTagNodes("v1")));
    }

    [Fact]
    public void Execute_RealWorld()
    {
        // v0
        ChainOperationContext<char> context = new();
        context.Chain.AppendRange("", "v0",
            //                       123456789-123456789-123456789-12345678
            TestHelper.GetNodesFrom("Seyd Vernünftige mir g'rade Mitte gest"));

        // v1
        CharChainReplaceOperation op1 = new()
        {
            At = 27,
            Run = 1,
            InputTag = "v0",
            OutputTag = "v1",
            Value = "' in die",
            Features =
            [
                OperationFeature.Parse("*log:=\"replaced e with ' in die\""),
            ]
        };
        op1.Execute(context);
        //            0123456789-123456789-123456789-123456789-1234
        Assert.Equal("Seyd Vernünftige mir g'rad' in die Mitte gest",
            TestHelper.GetVersionString(context.Chain.GetTagNodes("v1")));

        // v2
        CharChainReplaceOperation op2 = new()
        {
            At = 21,
            AtAsIndex = true,
            Run = 24,
            InputTag = "v1",
            OutputTag = "v2",
            Value = "grad gegen über gestellt",
            Features =
            [
                OperationFeature.Parse("*log:=\"replaced g'rad' in die Mitte gest " +
                "with grad gegen über gestellt\""),
            ]
        };
        op2.Execute(context);
        Assert.Equal("Seyd Vernünftige mir grad gegen über gestellt",
            TestHelper.GetVersionString(context.Chain.GetTagNodes("v2")));
    }
}
