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

    #region ToAsIndex parsing
    // These cover a DSL parsing bug where a bare "@TO" (index-based TO with
    // nothing following it) was swallowed by the generic "@GROUP" tail
    // marker, since both start with @, leaving nothing for the TO-specific
    // regex to match.
    [Fact]
    public void ParseOperation_At1Run1ToIndex3_NoTail()
    {
        CharChainMoveAfterOperation op = CharChainOperation
            .Parse<CharChainMoveAfterOperation>("1>]@3");

        Assert.Equal(OperationType.MoveAfter, op.Type);
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
        CharChainMoveAfterOperation op = CharChainOperation
            .Parse<CharChainMoveAfterOperation>("1>]@3^2@grp [g=1]");

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
        CharChainMoveAfterOperation op = CharChainOperation
            .Parse<CharChainMoveAfterOperation>("1>]@3");

        CharChainMoveAfterOperation op2 = CharChainOperation
            .Parse<CharChainMoveAfterOperation>(op.ToString());

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
    }

    [Fact]
    public void Execute_At1Run1To3_TraceFeatures()
    {
        // move A after C in ABC
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
        // input:
        // v0_1 (A)
        List<OperationFeature> features = context.NodeFeatures["v0_1"];
        Assert.Single(features);
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

    [Fact]
    public void Execute_At3Run1PastEnd_TraceFeatures()
    {
        // move Z after C in ARZDC
        ChainOperationContext<char> context = new();
        context.Chain.AppendRange("", "v0", TestHelper.GetNodesFrom("ARZDC"));
        CharChainMoveAfterOperation op = new()
        {
            At = 3,
            To = 5,
            InputTag = "v0",
            OutputTag = "v1",
            Features =
            [
                OperationFeature.Parse("*log:=\"moved Z after C\""),
                new OperationFeature("reason", "Z likes C")
            ],
        };

        op.Execute(context);

        Assert.Equal("ARDCZ", TestHelper.GetVersionString(
            context.Chain.GetTagNodes("v1")));
        // context tag
        Assert.Equal("v1", context.LastTag);
        // context features
        Assert.Single(context.Features);
        Assert.Equal("log", context.Features[0].Name);
        Assert.Equal("moved Z after C", context.Features[0].Value);

        // node features
        // input:
        // v0_3 (Z)
        List<OperationFeature> features = context.NodeFeatures["v0_3"];
        Assert.Single(features);
        Assert.Contains(features, f =>
            f.Name == OperationFeature.F_TRACE_SEG_IN &&
            f.Value == $"{op.Id} v0:v1 1");

        // v0_5 (C)
        features = context.NodeFeatures["v0_5"];
        Assert.Single(features);
        Assert.Contains(features, f =>
            f.Name == OperationFeature.F_TRACE_ANCHOR &&
            f.Value == $"{op.Id} v0:v1");

        // output:
        // v1_3 (Z) 
        features = context.NodeFeatures["v1_3"];
        Assert.Equal(2, features.Count);
        // reason
        Assert.Contains(features, f => f.Name == "reason" &&
            f.Value == "Z likes C");
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

    [Fact]
    public void Execute_RealWorld_Ok()
    {
        // init chain with base text (v0)
        ChainOperationContext<char> context = TestHelper.GetContext(
            "41.\n" +
            "So verwirrt mit dumpf willkührlich verwebten Geſtalten ,\n" +
            "Hölliſch und trübe geſinnt , Breughel den ſchwankenden Blick ;\n" +
            "So zerrüttet auch Dürer mit apokalÿptiſchen Bildern ,\n" +
            "Menſchen und Grillen zugleich , unſer geſundes Gehirn ;\n" +
            "So erreget ein Dichter , von Sphinxen , Sirenen , Centauren\n" +
            "Singend , Neugier mit Macht in dem verwunderten Ohr ;\n" +
            "So beweget ein Traum den Sorglichen , wenn er zu greifen ,\n" +
            "Vorwärts glaubt zu gehn , alles veränderlich ſchwebt  ;\n" +
            "So verwirrt uns Bettine , die holden Glieder verwechſelnd ,\n" +
            "Doch erfreut ſie uns gleich , wenn ſie die Sohlen betritt .");

        // the operation to test: move " Neugier" after "Macht"
        CharChainMoveAfterOperation op =
            CharChainOperation.Parse<CharChainMoveAfterOperation>(
                "304×8>]321 [reason=revert accepted-source-of=N7 " +
                "*log:=\"move 'Neugier' after 'Macht' *version^:=AS3]");

        op.Execute(context);

        Assert.Equal(
            "41.\n" +
            "So verwirrt mit dumpf willkührlich verwebten Geſtalten ,\n" +
            "Hölliſch und trübe geſinnt , Breughel den ſchwankenden Blick ;\n" +
            "So zerrüttet auch Dürer mit apokalÿptiſchen Bildern ,\n" +
            "Menſchen und Grillen zugleich , unſer geſundes Gehirn ;\n" +
            "So erreget ein Dichter , von Sphinxen , Sirenen , Centauren\n" +
            "Singend , mit Macht Neugier in dem verwunderten Ohr ;\n" +
            "So beweget ein Traum den Sorglichen , wenn er zu greifen ,\n" +
            "Vorwärts glaubt zu gehn , alles veränderlich ſchwebt  ;\n" +
            "So verwirrt uns Bettine , die holden Glieder verwechſelnd ,\n" +
            "Doch erfreut ſie uns gleich , wenn ſie die Sohlen betritt .",
            TestHelper.GetVersionString(context.Chain.GetTagNodes("v1")));
    }
}
