using System;
using System.Collections.Generic;
using Xunit;

namespace Gve.Text.Test;

public sealed class CharChainOperationTest
{
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

        // v0 ARZDC
        Assert.Equal("v0", tags[0]);
        Assert.Equal("ARZDC", TestHelper.GetVersionString(
            context.Chain.GetTagNodes("v0")));

        // v1 ARDC
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
}
