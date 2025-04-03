using Fusi.Tools.Data;
using Gve.Text;
using System;
using System.Collections.Generic;

namespace Gve.Migration.Test;

public sealed class VersionTreeBuilderTest
{
    private static (
        ChainOperationContext<char> Context,
        List<CharChainOperation> Operations) GetPenContext()
    {
        // https://vedph.github.io/gve-doc/model/sample-limerick.html
                          // 123456789
        const string text = "a red pen";

        // setup text
        ChainOperationContext<char> context = new();
        context.Chain.AppendRange("", "v0", TestHelper.GetNodesFrom(text));
        context.LastTag = "v0";
        Assert.Equal(text, TestHelper.GetVersionString(
            context.Chain.GetTagNodes("v0")));

        // setup operations
        const string operationsText =
            // v0>v1: delete 'red '
            "3x4- [*log=\"delete 'red '\"]\n" +
            // 1|2|7|8|9
            // a| |p|e|n

            // v1>v2: replace 'pen' with 'hat'
            "7x3=\"hat\" [*log=\"replace 'pen' with 'hat'\"]\n" +
            // 1|2|10|11|12
            // a| |h |a |t

            // v2>v3: replace 'hat' with 'cat'
            "10x3=\"cat\" [*log=\"replace 'hat' with 'cat'\"]\n" +
            // 1|2|13|14|15
            // a| |c |a |t

            // v3>v4: insert 'nice ' before 'cat'
            "13+[\"nice \" [*log=\"insert 'nice ' before 'cat'\"]\n" +
            // 1|2|16|17|18|19|20|13|14|15
            // a| |n |i |c |e |  |c |a |t

            // v4>v5: swap 'nice' with 'cat'
            "16x4<>13x3 [*log=\"swap 'nice' with 'cat'\"]\n" +
            // 1|2|13|14|15|20|16|17|18|19
            // a| |c |a |t |  |n |i |c |e

            // v5>v6: insert 'is ' before 'nice'
            "16+[\"is \" [*log=\"insert 'is ' before 'nice'\"]";
            // 1|2|13|14|15|20|21|22|23|16|17|18|19
            // a| |c |a |t |  |i |s |  |n |i |c |e

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

        return (context, operations);
    }

    [Fact]
    public void Build_Pen_NoDeletions()
    {
        // get chain and its context
        var (context, operations) = GetPenContext();

        // get tree for final version (v7)
        VersionTreeBuilder builder = new()
        {
            FeatureNames =
            [
                OperationFeature.F_TRACE_SEG_IN,
                OperationFeature.F_TRACE_SEG2_IN,
                OperationFeature.F_TRACE_SEG_OUT,
                OperationFeature.F_TRACE_SEG2_OUT,
                OperationFeature.F_TRACE_DELETED
            ],
            TraceFeatureFlattening = true,
            IncludeDeleted = false
        };

        IDictionary<string, TreeNode<ExportedSegment>> versionTrees =
            builder.Build(context, operations);

        Assert.Equal(7, versionTrees.Count);

        // v0 tree: a red pen
        TreeNode<ExportedSegment> tree = versionTrees["v0"];
        string text = VersionTreeBuilder.GetTreeText(tree);
        Assert.Equal("a red pen", text);

        // v1 tree: a pen
        tree = versionTrees["v1"];
        text = VersionTreeBuilder.GetTreeText(tree);
        Assert.Equal("a pen", text);

        // v2 tree: a hat
        tree = versionTrees["v2"];
        text = VersionTreeBuilder.GetTreeText(tree);
        Assert.Equal("a hat", text);

        // v3 tree: a cat
        tree = versionTrees["v3"];
        text = VersionTreeBuilder.GetTreeText(tree);
        Assert.Equal("a cat", text);

        // v4 tree: a nice cat
        tree = versionTrees["v4"];
        text = VersionTreeBuilder.GetTreeText(tree);
        Assert.Equal("a nice cat", text);

        // v5 tree: a cat nice
        tree = versionTrees["v5"];
        text = VersionTreeBuilder.GetTreeText(tree);
        Assert.Equal("a cat nice", text);

        // v6 tree: a cat is nice
        tree = versionTrees["v6"];
        text = VersionTreeBuilder.GetTreeText(tree);
        Assert.Equal("a cat is nice", text);
    }

    [Fact]
    public void Build_Pen_Deletions()
    {
        // get chain and its context
        var (context, operations) = GetPenContext();

        // get tree for final version (v7)
        VersionTreeBuilder builder = new()
        {
            FeatureNames =
            [
                OperationFeature.F_TRACE_SEG_IN,
                OperationFeature.F_TRACE_SEG2_IN,
                OperationFeature.F_TRACE_SEG_OUT,
                OperationFeature.F_TRACE_SEG2_OUT,
                OperationFeature.F_TRACE_DELETED
            ],
            TraceFeatureFlattening = true,
            IncludeDeleted = true
        };

        IDictionary<string, TreeNode<ExportedSegment>> versionTrees =
            builder.Build(context, operations);

        Assert.Equal(7, versionTrees.Count);

        // v0 tree: a red pen
        TreeNode<ExportedSegment> tree = versionTrees["v0"];
        string text = VersionTreeBuilder.GetTreeText(tree);
        Assert.Equal("a red pen", text);
        text = VersionTreeBuilder.GetTreeText(tree, true);
        Assert.Equal("a red pen", text);

        // v1 tree (del): a {red }pen
        tree = versionTrees["v1"];
        text = VersionTreeBuilder.GetTreeText(tree);
        Assert.Equal("a red pen", text);
        text = VersionTreeBuilder.GetTreeText(tree, true);
        Assert.Equal("a {red }pen", text);

        // v2 tree (rep): a {red }{pen}hat
        tree = versionTrees["v2"];
        text = VersionTreeBuilder.GetTreeText(tree);
        Assert.Equal("a red penhat", text);
        text = VersionTreeBuilder.GetTreeText(tree, true);
        Assert.Equal("a {red }{pen}hat", text);

        // v3 tree (rep): a {red }{pen}{hat}cat
        tree = versionTrees["v3"];
        text = VersionTreeBuilder.GetTreeText(tree);
        Assert.Equal("a red penhatcat", text);
        text = VersionTreeBuilder.GetTreeText(tree, true);
        Assert.Equal("a {red }{pen}{hat}cat", text);

        // v4 tree (ins): a nice {red }{pen}{hat}cat
        tree = versionTrees["v4"];
        text = VersionTreeBuilder.GetTreeText(tree);
        Assert.Equal("a nice red penhatcat", text);
        text = VersionTreeBuilder.GetTreeText(tree, true);
        Assert.Equal("a nice {red }{pen}{hat}cat", text);

        // v5 tree (swp): a {red }{pen}{hat}{nice}cat {cat}nice
        tree = versionTrees["v5"];
        text = VersionTreeBuilder.GetTreeText(tree);
        Assert.Equal("a red penhatnicecat catnice", text);
        text = VersionTreeBuilder.GetTreeText(tree, true);
        Assert.Equal("a {red }{pen}{hat}{nice}cat {cat}nice", text);

        // v6 tree: (ins) a {red }{pen}{hat}cat is nice
        tree = versionTrees["v6"];
        text = VersionTreeBuilder.GetTreeText(tree);
        Assert.Equal("a red penhatnicecat is catnice", text);
        text = VersionTreeBuilder.GetTreeText(tree, true);
        Assert.Equal("a {red }{pen}{hat}cat is nice", text);
    }

    private static (
        ChainOperationContext<char> Context,
        List<CharChainOperation> Operations) GetHatContext()
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

        return (context, operations);
    }

    [Fact]
    public void Build_Hat_Deletions()
    {
        // get chain and its context
        var (context, operations) = GetHatContext();

        // get tree for final version (v7)
        VersionTreeBuilder builder = new()
        {
            FeatureNames =
            [
                OperationFeature.F_TRACE_SEG_IN,
                OperationFeature.F_TRACE_SEG2_IN,
                OperationFeature.F_TRACE_SEG_OUT,
                OperationFeature.F_TRACE_SEG2_OUT,
                OperationFeature.F_TRACE_DELETED
            ],
            TraceFeatureFlattening = true,
            IncludeDeleted = true
        };

        IDictionary<string, TreeNode<ExportedSegment>> versionTrees =
            builder.Build(context, operations);

        Assert.Equal(8, versionTrees.Count);

        // v7 tree:
        // for sure, he had a green hat|and very long beard and moustache.
        // gggggggggg.........22222.........11111.....77777.....777777777.
        // g=4,5,6
        TreeNode<ExportedSegment> tree = versionTrees["v7"];
        // TODO
    }
}
