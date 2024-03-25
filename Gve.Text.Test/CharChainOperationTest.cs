using System;
using System.Collections.Generic;
using Xunit;

namespace Gve.Text.Test;

public sealed class CharChainOperationTest
{
    [Fact]
    public void Execute_ManyOperations_AutoTags()
    {
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
            // v0>v1
            "3- [*log=\"delete Z\"]\n" +
            // v1>v2
            "2=V [*log=\"replace R with V\"]\n" +
            // v2>v3
            "6=B [*log=\"replace V with B\"]\n" +
            // v1>v4
            "(v1:) 2=P [*log=\"replace R with P\"]\n" +
            // v4>v5
            "4<>5\n" +
            // v3>v6
            "(v3:) 4<>5";

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

        Assert.Equal(6, context.Steps.Count);

        IList<string> tags = context.Chain.GetDistinctTags();
        Assert.Equal(7, tags.Count);

        Assert.Equal("v0", tags[0]);
        Assert.Equal("ARZDC", TestHelper.GetVersionString(
            context.Chain.GetTagNodes("v0")));

        Assert.Equal("v1", tags[1]);
        Assert.Equal("ARDC", TestHelper.GetVersionString(
            context.Chain.GetTagNodes("v1")));

        Assert.Equal("v2", tags[2]);
        Assert.Equal("AVDC", TestHelper.GetVersionString(
            context.Chain.GetTagNodes("v2")));

        Assert.Equal("v3", tags[3]);
        Assert.Equal("ABDC", TestHelper.GetVersionString(
            context.Chain.GetTagNodes("v3")));

        Assert.Equal("v4", tags[4]);
        Assert.Equal("APDC", TestHelper.GetVersionString(
            context.Chain.GetTagNodes("v4")));

        Assert.Equal("v5", tags[5]);
        Assert.Equal("APCD", TestHelper.GetVersionString(
            context.Chain.GetTagNodes("v5")));

        Assert.Equal("v6", tags[6]);
        Assert.Equal("ABCD", TestHelper.GetVersionString(
            context.Chain.GetTagNodes("v6")));
    }
}
