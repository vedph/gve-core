using Gve.Text.Annotations;
using Gve.Text.Base;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using Xunit;

namespace Gve.Text.Test;

public sealed class VarGeneratorTest
{
    //                           12345
    private const string TEXT = "ARZDC";
    private const string ENTRIES =
        "- 2=\"\" [x=2 y=2]\n" +
        "  - 1=\"V\" [x]\n" +
        "    - 1=\"B\" [x]\n" +
        "  - 1=\"P\" [y]\n" +
        "- 3=\"\" [x=1 y=1]\n" +
        "- 5>4 [x=3 y=3]\n";

    private readonly ILogger<VarGeneratorTest>? _logger;

    public VarGeneratorTest()
    {
        _logger = new ServiceCollection()
            .AddLogging(builder => builder.AddDebug())
            .BuildServiceProvider()
            .GetService<ILogger<VarGeneratorTest>>();
    }

    [Fact]
    public void GetVersionTags_Ok()
    {
        VarGenerator generator = new(ENTRIES);

        IList<string> tags = generator.GetVersionTags();

        Assert.Equal(2, tags.Count);
        Assert.Equal("x", tags[0]);
        Assert.Equal("y", tags[1]);
    }

    [Fact]
    public void GetVersionEntries_Ok()
    {
        VarGenerator generator = new(ENTRIES, _logger);

        IList<VarOperationEntry> entries = generator.GetVersionEntries("x");

        Assert.Equal(3, entries.Count);
        // [0] = 3=""
        VarOperationEntry entry = entries[0];
        Assert.IsType<ReplaceBaseVarOperation>(entry.Operation);
        Assert.Equal(3, entry.Operation.At);
        Assert.Equal(1, entry.Operation.Run);
        Assert.Equal("", entry.Operation.Value);
        Assert.Null(entry.Root);

        // [1] = 2="" with root having 1="V" having 1="B"
        entry = entries[1];
        Assert.IsType<ReplaceBaseVarOperation>(entry.Operation);
        Assert.Equal(2, entry.Operation.At);
        Assert.Equal(1, entry.Operation.Run);
        Assert.Equal("", entry.Operation.Value);

        Assert.NotNull(entry.Root);
        TreeNode<AnnVarOperation> root = entry.Root;
        Assert.Equal(2, root.Children.Count);
        Assert.Single(root.Children[0].Children);
        Assert.Empty(root.Children[1].Children);

        // [2] = 5>4
        entry = entries[2];
        Assert.IsType<MoveBaseVarOperation>(entry.Operation);
        Assert.Equal(5, entry.Operation.At);
        Assert.Equal(1, entry.Operation.Run);
        Assert.Null(entry.Operation.Value);
        Assert.Null(entry.Root);
    }

    [Fact]
    public void Generate_AllVersions_Ok()
    {
        VarGenerator generator = new(ENTRIES, _logger);

        IDictionary<string, IList<CharNodeSet>> results =
            generator.Generate(TEXT);

        Assert.Equal(2, results.Keys.Count);
        // TODO other assertions
    }
}
