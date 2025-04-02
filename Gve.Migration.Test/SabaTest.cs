using Fusi.Tools.Data;
using Gve.Text;
using System;
using System.Collections.Generic;

namespace Gve.Migration.Test;

public sealed class SabaTest
{
    private static (
        ChainOperationContext<char> Context,
        List<CharChainOperation> Operations) GetContext()
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
    public void Export_Scenario_Saba()
    {
        // get chain and its context
        var (context, operations) = GetContext();

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
            FlattenTraceFeatures = true
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
