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

    [Fact]
    public void Execute_LimerickSample()
    {
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
}
