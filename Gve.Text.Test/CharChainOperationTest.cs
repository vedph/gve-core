using System.Collections.Generic;
using Xunit;

namespace Gve.Text.Test;

public sealed class CharChainOperationTest
{
    [Fact]
    public void Execute_ManyOperations_AutoTags()
    {
        ChainOperationContext<char> context = new();
        context.Chain.AppendRange("", "v0", TestHelper.GetNodesFrom("AXVC"));
        context.LastTag = "v0";

        Assert.Equal("AXVC", TestHelper.GetVersionString(
            context.Chain.GetTagNodes("v0")));

        CharChainOperation del = CharChainOperation
            .Parse<CharChainDeleteOperation>("2-");
        CharChainOperation rep = CharChainOperation
            .Parse<CharChainReplaceOperation>("3=B");

        del.Execute(context);
        rep.Execute(context);

        Assert.Equal(2, context.Steps.Count);

        IList<string> tags = context.Chain.GetDistinctTags();
        Assert.Equal(3, tags.Count);
        Assert.Equal("v0", tags[0]);
        Assert.Equal("AXVC", TestHelper.GetVersionString(
            context.Chain.GetTagNodes("v0")));

        Assert.Equal("v1", tags[1]);
        Assert.Equal("AVC", TestHelper.GetVersionString(
            context.Chain.GetTagNodes("v1")));

        Assert.Equal("v2", tags[2]);
        Assert.Equal("ABC", TestHelper.GetVersionString(
            context.Chain.GetTagNodes("v2")));
    }
}
