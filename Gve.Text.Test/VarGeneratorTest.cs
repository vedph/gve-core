using System.Collections.Generic;
using Xunit;

namespace Gve.Text.Test;

public sealed class VarGeneratorTest
{
    private const string TEXT = "ARZDC";

    private const string ENTRIES =
        "- 2=\"\" [a=2 b=2]\n" +
        "  - 1=\"V\" [a]\n" +
        "    - 1=\"B\" [a]\n" +
        "  - 1=\"P\" [b]\n" +
        "- 3=\"\" [a=1]\n" +
        "- 5>4 [a=3]\n";

    [Fact]
    public void GetVersionTags_Ok()
    {
        VarGenerator generator = new(ENTRIES);

        IList<string> tags = generator.GetVersionTags();

        Assert.Equal(2, tags.Count);
        Assert.Equal("a", tags[0]);
        Assert.Equal("b", tags[1]);
    }

    [Fact]
    public void Generate_AllVersions_Ok()
    {
        VarGenerator generator = new(ENTRIES);

        IDictionary<string, IList<CharNodeSet>> results =
            generator.Generate(TEXT);

        Assert.Equal(2, results.Keys.Count);
        // TODO other assertions
    }
}
