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
        "- 3=\"\" [x=1 y=1] {reason=\"don't like Z\"}\n" +
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
        TreeNode root = entry.Root;
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
        VarGenerator generator = new(ENTRIES, _logger)
        {
            IsTrackingEnabled = true
        };

        IDictionary<string, IList<CharNodeSet>> results =
            generator.Generate(TEXT);

        Assert.Equal(2, results.Keys.Count);
        // x
        IList<CharNodeSet> r = results["x"];
        Assert.Equivalent(2, r.Count);
        // AVZCD
        Assert.Equal("AVCD", r[0].GetText());
        Assert.Equal("AVZ†CD", r[0].GetText(true));
        // A
        IList<CharNode> nodes = [.. r[0]];
        CharNode? cn = nodes[0];
        Assert.NotNull(cn);
        Assert.Equal(1, cn.Ordinal);
        Assert.Equal('A', cn.Value);
        Assert.Empty(cn.PreviousValues);
        Assert.False(cn.IsDeleted);
        Assert.Empty(cn.Features);
        // V
        cn = nodes[1];
        Assert.NotNull(cn);
        Assert.Equal(2, cn.Ordinal);
        Assert.Equal('V', cn.Value);
        Assert.Single(cn.PreviousValues);
        Assert.Equal('R', cn.PreviousValues[0]);
        Assert.False(cn.IsDeleted);
        Assert.Empty(cn.Features);
        // Z
        cn = nodes[2];
        Assert.NotNull(cn);
        Assert.Equal(3, cn.Ordinal);
        Assert.Equal('Z', cn.Value);
        Assert.Empty(cn.PreviousValues);
        Assert.True(cn.IsDeleted);
        Assert.Single(cn.Features);
        Assert.Equal("reason", cn.Features[0].Name);
        Assert.Equal("don't like Z", cn.Features[0].Value);
        // C
        cn = nodes[3];
        Assert.NotNull(cn);
        Assert.Equal(5, cn.Ordinal);
        Assert.Equal('C', cn.Value);
        Assert.Empty(cn.PreviousValues);
        Assert.False(cn.IsDeleted);
        Assert.Empty(cn.Features);
        // D
        cn = nodes[4];
        Assert.NotNull(cn);
        Assert.Equal(4, cn.Ordinal);
        Assert.Equal('D', cn.Value);
        Assert.Empty(cn.PreviousValues);
        Assert.False(cn.IsDeleted);
        Assert.Empty(cn.Features);

        // ABZCD
        Assert.Equal("ABCD", r[1].GetText());
        Assert.Equal("ABZ†CD", r[1].GetText(true));
        // A
        nodes = [.. r[1]];
        cn = nodes[0];
        Assert.NotNull(cn);
        Assert.Equal(1, cn.Ordinal);
        Assert.Equal('A', cn.Value);
        Assert.Empty(cn.PreviousValues);
        Assert.False(cn.IsDeleted);
        Assert.Empty(cn.Features);
        // B
        cn = nodes[1];
        Assert.NotNull(cn);
        Assert.Equal(2, cn.Ordinal);
        Assert.Equal('B', cn.Value);
        Assert.Equal(2, cn.PreviousValues.Count);
        Assert.Equal('V', cn.PreviousValues[0]);
        Assert.Equal('R', cn.PreviousValues[1]);
        Assert.False(cn.IsDeleted);
        Assert.Empty(cn.Features);
        // Z
        cn = nodes[2];
        Assert.NotNull(cn);
        Assert.Equal(3, cn.Ordinal);
        Assert.Equal('Z', cn.Value);
        Assert.Empty(cn.PreviousValues);
        Assert.True(cn.IsDeleted);
        Assert.Single(cn.Features);
        Assert.Equal("reason", cn.Features[0].Name);
        Assert.Equal("don't like Z", cn.Features[0].Value);
        // C
        cn = nodes[3];
        Assert.NotNull(cn);
        Assert.Equal(5, cn.Ordinal);
        Assert.Equal('C', cn.Value);
        Assert.Empty(cn.PreviousValues);
        Assert.False(cn.IsDeleted);
        Assert.Empty(cn.Features);
        // D
        cn = nodes[4];
        Assert.NotNull(cn);
        Assert.Equal(4, cn.Ordinal);
        Assert.Equal('D', cn.Value);
        Assert.Empty(cn.PreviousValues);
        Assert.False(cn.IsDeleted);
        Assert.Empty(cn.Features);

        // y
        r = results["y"];
        Assert.Single(r);
        Assert.Equal("APCD", r[0].GetText());
        Assert.Equal("APZ†CD", r[0].GetText(true));

        // TODO assert steps
    }
}
