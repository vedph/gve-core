using System;
using System.Collections.Generic;
using Xunit;

namespace Gve.Text.Test;

public sealed class CharChainOperationTest
{
    private static void AssertDeletedZFeatures(IList<OperationFeature> features,
        string opId)
    {
        Assert.Equal(2, features.Count);
        // reason
        Assert.Contains(features, f =>
            f.Name == "reason" &&
            f.Value == "don't like Z");
        // del
        Assert.Contains(features, f =>
            f.Name == OperationMetadata.DEL_KEY &&
            f.Value == $"{opId} v0:v1 1");
    }

    [Fact]
    public void Execute_MockSample()
    {
        // https://vedph.github.io/gve-doc/model/sample-arzdc.html
        // setup text
        // 12345
        // ARZDC
        ChainOperationContext<char> context = new();
        context.Chain.AppendRange("", "v0", TestHelper.GetNodesFrom("ARZDC"));
        context.LastTag = "v0";
        Assert.Equal("ARZDC", TestHelper.GetVersionString(
            context.Chain.GetTagNodes("v0")));

        // setup operations
        const string operationsText =
            // v0>v1 del Z
            "3- [*log=\"delete Z\" reason:=\"don't like Z\"]\n" +
            // v1>v2 rep R=V (alpha)
            "2=V [*log=\"replace R with V\" *version^:=alpha]\n" +
            // v2>v3 rep V=B
            "6=B [*log=\"replace V with B\" reason:=\"like B\"]\n" +
            // v1>v4 (v1) rep R=P (beta)
            "(v1:) 2=P [*log=\"replace R with P\" *version^:=beta]\n" +
            // v4>v5 (v3) swap D/C
            "(v3:) 4<>5 [*version^:=gamma]";

        List<CharChainOperation> operations = [];
        foreach (string line in operationsText.Split('\n',
            StringSplitOptions.RemoveEmptyEntries))
        {
            CharChainOperation op = CharChainOperation
                .Parse<CharChainOperation>(line);
            operations.Add(op);
        }

        // execute operations
        foreach (CharChainOperation op in operations) op.Execute(context);

        Assert.Equal(5, context.Steps.Count);

        IList<string> tags = context.Chain.GetDistinctTags();
        Assert.Equal(6, tags.Count);

        // v0 ARZDC (base text: Z in)
        Assert.Equal("v0", tags[0]);
        Assert.Equal("ARZDC", TestHelper.GetVersionString(
            context.Chain.GetTagNodes("v0")));

        // v1 ARDC (del Z: Z out)
        Assert.Equal("v1", tags[1]);
        Assert.Equal("ARDC", TestHelper.GetVersionString(
            context.Chain.GetTagNodes("v1")));

        // v2 AVDC
        Assert.Equal("v2", tags[2]);
        Assert.Equal("AVDC", TestHelper.GetVersionString(
            context.Chain.GetTagNodes("v2")));

        // v3 ABDC
        Assert.Equal("v3", tags[3]);
        Assert.Equal("ABDC", TestHelper.GetVersionString(
            context.Chain.GetTagNodes("v3")));

        // v4 APDC
        Assert.Equal("v4", tags[4]);
        Assert.Equal("APDC", TestHelper.GetVersionString(
            context.Chain.GetTagNodes("v4")));

        // v5 ABCD
        Assert.Equal("v5", tags[5]);
        Assert.Equal("ABCD", TestHelper.GetVersionString(
            context.Chain.GetTagNodes("v5")));

        // node features:
        // v0_3 (Z)
        List<OperationFeature> features = context.NodeFeatures["v0_3"];
        Assert.Single(features);
        // $seg-in (Z)
        Assert.Contains(features, f =>
            f.Name == OperationFeature.F_TRACE_SEG_IN &&
            f.Value == $"{operations[0].Id} v0:v1 1");

        IList<ChainNode<char>> deletedNodes = context.GetDeletedNodes("v1");
        Assert.Single(deletedNodes);
        Assert.Equal('Z', deletedNodes[0].Data);

        // v1_3 (Z)
        features = context.NodeFeatures["v1_3"];
        AssertDeletedZFeatures(features, operations[0].Id);

        // v1_2 (R)
        features = context.NodeFeatures["v1_2"];
        Assert.Equal(2, features.Count);

        // as we have branching, R is the input of two different ops
        // $seg-in for v2
        Assert.Contains(features, f =>
            f.Name == OperationFeature.F_TRACE_SEG_IN &&
            f.Value == $"{operations[1].Id} v1:v2 1");
        // $seg-in for v4
        Assert.Contains(features, f =>
            f.Name == OperationFeature.F_TRACE_SEG_IN &&
            f.Value == $"{operations[3].Id} v1:v4 1");

        // v2_3 (Z): inherited
        features = context.NodeFeatures["v1_3"];
        AssertDeletedZFeatures(features, operations[0].Id);

        // v2_6 (V)
        features = context.NodeFeatures["v2_6"];
        Assert.Equal(3, features.Count);
        // opid
        Assert.Contains(features, f =>
            f.Name == OperationMetadata.OPID_KEY &&
            f.Value == operations[1].Id);
        // $seg-out from v2
        Assert.Contains(features, f =>
            f.Name == OperationFeature.F_TRACE_SEG_OUT &&
            f.Value == $"{operations[1].Id} v1:v2 1");
        // $seg-in for v3
        Assert.Contains(features, f =>
            f.Name == OperationFeature.F_TRACE_SEG_IN &&
            f.Value == $"{operations[2].Id} v2:v3 1");

        // v3_3 (Z): inherited
        features = context.NodeFeatures["v3_3"];
        AssertDeletedZFeatures(features, operations[0].Id);

        // v3_6 (V): inherited
        // opid
        features = context.NodeFeatures["v3_6"];
        Assert.Single(features);
        Assert.Contains(features, f =>
            f.Name == OperationMetadata.OPID_KEY &&
            f.Value == operations[1].Id);

        // v3_4 (D)
        // $seg-in for v5
        features = context.NodeFeatures["v3_4"];
        Assert.Single(features);
        Assert.Contains(features, f =>
            f.Name == OperationFeature.F_TRACE_SEG_IN &&
            f.Value == $"{operations[4].Id} v3:v5 1");

        // v3_5 (C)
        // $seg2-in for v5
        features = context.NodeFeatures["v3_5"];
        Assert.Single(features);
        Assert.Contains(features, f =>
            f.Name == OperationFeature.F_TRACE_SEG2_IN &&
            f.Value == $"{operations[4].Id} v3:v5 1");

        // v3_7 (B)
        features = context.NodeFeatures["v3_7"];
        Assert.Equal(3, features.Count);
        // opid
        Assert.Contains(features, f =>
            f.Name == OperationMetadata.OPID_KEY &&
            f.Value == operations[2].Id);
        // reason
        Assert.Contains(features, f =>
            f.Name == "reason" &&
            f.Value == "like B");
        // $seg-out from v3
        Assert.Contains(features, f =>
            f.Name == OperationFeature.F_TRACE_SEG_OUT &&
            f.Value == $"{operations[2].Id} v2:v3 1");

        // v4_3 (Z): inherited
        features = context.NodeFeatures["v4_3"];
        AssertDeletedZFeatures(features, operations[0].Id);

        // v4_8 (P)
        features = context.NodeFeatures["v4_8"];
        Assert.Equal(2, features.Count);
        // opid
        Assert.Contains(features, f =>
            f.Name == OperationMetadata.OPID_KEY &&
            f.Value == operations[3].Id);
        // $seg-out from v4
        Assert.Contains(features, f =>
            f.Name == OperationFeature.F_TRACE_SEG_OUT &&
            f.Value == $"{operations[3].Id} v1:v4 1");

        // v5_3 (Z): inherited
        features = context.NodeFeatures["v5_3"];
        AssertDeletedZFeatures(features, operations[0].Id);

        // v5_4 (D)
        features = context.NodeFeatures["v5_4"];
        Assert.Single(features);
        // $seg2-out from v5
        Assert.Contains(features, f =>
            f.Name == OperationFeature.F_TRACE_SEG2_OUT &&
            f.Value == $"{operations[4].Id} v3:v5 1");

        // v5_5 (C)
        features = context.NodeFeatures["v5_5"];
        Assert.Single(features);
        // $seg-out from v5
        Assert.Contains(features, f =>
            f.Name == OperationFeature.F_TRACE_SEG_OUT &&
            f.Value == $"{operations[4].Id} v3:v5 1");

        // v5_6 (V): inherited
        features = context.NodeFeatures["v5_6"];
        Assert.Single(features);
        // opid
        Assert.Contains(features, f =>
            f.Name == OperationMetadata.OPID_KEY &&
            f.Value == operations[1].Id);

        // v5_7 (B): inherited
        features = context.NodeFeatures["v5_7"];
        Assert.Equal(2, features.Count);
        // opid
        Assert.Contains(features, f =>
            f.Name == OperationMetadata.OPID_KEY &&
            f.Value == operations[2].Id);
        // reason
        Assert.Contains(features, f =>
            f.Name == "reason" &&
            f.Value == "like B");
    }

    [Fact]
    public void Execute_LimerickSample()
    {
        // https://vedph.github.io/gve-doc/model/sample-limerick.html
        const string text = "there was an old man with a beard,\n" +
            "who cried: \"It is just as I feared!\n" +
            "four larks and a wren,\n" +
            "two swans and a hen,\n" +
            "all built their nests in my beard!\"";

        // setup text
        ChainOperationContext<char> context = new();
        context.Chain.AppendRange("", "v0", TestHelper.GetNodesFrom(text));
        context.LastTag = "v0";
        Assert.Equal(text, TestHelper.GetVersionString(
            context.Chain.GetTagNodes("v0")));

        // setup operations
        const string operationsText =
            // v0>v1
            "40x5=\"said\" [*log=\"said for cried\"]\n" +
            // v1>v2
            "99x5=\"crows\" [*log=\"crows for swans\"]\n" +
            // v2>v3
            "116+[\"have \" [*log=\"insert have\" " +
                "*version^=\"alpha\" reason=\"metrical\"]\n" +
            // v1>v4
            "72x23<>95x21 [*log=\"swap verses\"]\n" +
            // v4>v5
            "155x5=\"owls\" [*log=\"owls for crows\" *version^=\"beta\"]\n";

        List<CharChainOperation> operations = [];
        foreach (string line in operationsText.Split('\n',
            StringSplitOptions.RemoveEmptyEntries))
        {
            CharChainOperation op = CharChainOperation
                .Parse<CharChainOperation>(line);
            operations.Add(op);
        }

        // execute operations
        foreach (CharChainOperation op in operations) op.Execute(context);

        Assert.Equal(5, context.Steps.Count);

        IList<string> tags = context.Chain.GetDistinctTags();
        Assert.Equal(6, tags.Count);

        // v0 (original)
        Assert.Equal("v0", tags[0]);
        Assert.Equal(text, TestHelper.GetVersionString(
            context.Chain.GetTagNodes("v0")));

        // v1 ("said" replaced "cried")
        Assert.Equal("v1", tags[1]);
        Assert.Equal("there was an old man with a beard,\n" +
            "who said: \"It is just as I feared!\n" +
            "four larks and a wren,\n" +
            "two swans and a hen,\n" +
            "all built their nests in my beard!\"",
            TestHelper.GetVersionString(context.Chain.GetTagNodes("v1")));

        // v2 ("crows" replaced "swans")
        Assert.Equal("v2", tags[2]);
        Assert.Equal("there was an old man with a beard,\n" +
            "who said: \"It is just as I feared!\n" +
            "four larks and a wren,\n" +
            "two crows and a hen,\n" +
            "all built their nests in my beard!\"",
            TestHelper.GetVersionString(context.Chain.GetTagNodes("v2")));

        // v3 ("have" added before "all"))
        Assert.Equal("v3", tags[3]);
        Assert.Equal("there was an old man with a beard,\n" +
            "who said: \"It is just as I feared!\n" +
            "four larks and a wren,\n" +
            "two crows and a hen,\n" +
            "have all built their nests in my beard!\"",
            TestHelper.GetVersionString(context.Chain.GetTagNodes("v3")));

        // v4 (swapped verses 3-4)
        Assert.Equal("v4", tags[4]);
        Assert.Equal("there was an old man with a beard,\n" +
            "who said: \"It is just as I feared!\n" +
            "two crows and a hen,\n" +
            "four larks and a wren,\n" +
            "have all built their nests in my beard!\"",
            TestHelper.GetVersionString(context.Chain.GetTagNodes("v4")));

        // v5 ("owls" replaced "crows")
        Assert.Equal("v5", tags[5]);
        Assert.Equal("there was an old man with a beard,\n" +
            "who said: \"It is just as I feared!\n" +
            "two owls and a hen,\n" +
            "four larks and a wren,\n" +
            "have all built their nests in my beard!\"",
            TestHelper.GetVersionString(context.Chain.GetTagNodes("v5")));
    }

    [Fact]
    public void Execute_MockSample2()
    {
        // https://vedph.github.io/gve-doc/model/sample-limerick.html
        const string text =
          // 123456789-123456789-1234567
            "he had a red hat for sure,\n" +
          // 89-123456789-123456789-123456789-1
            "and long moustache and grey beard.";

        // setup text
        ChainOperationContext<char> context = new();
        context.Chain.AppendRange("", "v0", TestHelper.GetNodesFrom(text));
        context.LastTag = "v0";
        Assert.Equal(text, TestHelper.GetVersionString(
            context.Chain.GetTagNodes("v0")));

        // setup operations
        const string operationsText =
            // v0>v1
            "32+[\"very \" [*log=\"insert 'very ' before 'long'\"]\n" +
            // v1>v2
            "10x3=\"green\" [*log=\"replace 'red' with 'green'\"]\n" +
            // v2>v3
            "51x5- [*log=\"delete 'grey '\" *version^=\"A\"]\n" +
            // v3>v4
            "18x9>[1@fs [*log=\"move 'for sure,' before 'he'\"]\n" +
            // v4>v5
            "17-@fs [*log=\"delete tail ' '\"]\n" +
            // v5>v6
            "1+[\" \"@fs [*log=\"insert space before 'he'\"]\n" +
            // v6>v7
            "37x9<>56x5 [*log=\"swap 'moustache' / 'beard'\" *version^=\"B\"]";

        List<CharChainOperation> operations = [];
        foreach (string line in operationsText.Split('\n',
            StringSplitOptions.RemoveEmptyEntries))
        {
            CharChainOperation op = CharChainOperation
                .Parse<CharChainOperation>(line);
            operations.Add(op);
        }

        // execute operations
        foreach (CharChainOperation op in operations) op.Execute(context);

        Assert.Equal(7, context.Steps.Count);

        IList<string> tags = context.Chain.GetDistinctTags();
        Assert.Equal(8, tags.Count);

        // v0 (original)
        Assert.Equal("v0", tags[0]);
        Assert.Equal(text, TestHelper.GetVersionString(
            context.Chain.GetTagNodes("v0")));

        // v1 ("very " added before "long")
        Assert.Equal("v1", tags[1]);
        Assert.Equal("he had a red hat for sure,\n" +
            "and very long moustache and grey beard.",
            TestHelper.GetVersionString(context.Chain.GetTagNodes("v1")));

        // v2 ("green" replaced "red")
        Assert.Equal("v2", tags[2]);
        Assert.Equal("he had a green hat for sure,\n" +
            "and very long moustache and grey beard.",
            TestHelper.GetVersionString(context.Chain.GetTagNodes("v2")));

        // v3 ("grey" deleted)
        Assert.Equal("v3", tags[3]);
        Assert.Equal("he had a green hat for sure,\n" +
            "and very long moustache and beard.",
            TestHelper.GetVersionString(context.Chain.GetTagNodes("v3")));

        // v4 ("for sure," moved before "he")
        Assert.Equal("v4", tags[4]);
        Assert.Equal("for sure,he had a green hat \n" +
            "and very long moustache and beard.",
            TestHelper.GetVersionString(context.Chain.GetTagNodes("v4")));

        // v5 (tail " " deleted)
        Assert.Equal("v5", tags[5]);
        Assert.Equal("for sure,he had a green hat\n" +
            "and very long moustache and beard.",
            TestHelper.GetVersionString(context.Chain.GetTagNodes("v5")));

        // v6 (space added before "he")
        Assert.Equal("v6", tags[6]);
        Assert.Equal("for sure, he had a green hat\n" +
            "and very long moustache and beard.",
            TestHelper.GetVersionString(context.Chain.GetTagNodes("v6")));

        // v7 ("moustache" and "beard" swapped)
        Assert.Equal("v7", tags[7]);
        Assert.Equal("for sure, he had a green hat\n" +
            "and very long beard and moustache.",
            TestHelper.GetVersionString(context.Chain.GetTagNodes("v7")));
    }
}
