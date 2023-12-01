using Gve.Text.Base;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Gve.Text.Test.Base;

public sealed class ReplaceBaseVarOperationTest
{
    private readonly CharNodeSet _set;

    public ReplaceBaseVarOperationTest()
    {
        _set = new CharNodeSet("alpha beta gamma");
    }

    [Fact]
    public void Parse_Empty_Invalid()
    {
        Assert.False(BaseVarOperation.TryParse("", _set,
            out ReplaceBaseVarOperation? _));
    }

    [Fact]
    public void Parse_Invalid_Invalid()
    {
        Assert.False(BaseVarOperation.TryParse("invalid", _set,
            out ReplaceBaseVarOperation? _));
    }

    [Fact]
    public void Parse_ARValue_Ok()
    {
        Assert.True(BaseVarOperation.TryParse("12x3=\"value\"",
            _set, out ReplaceBaseVarOperation? op));
        Assert.NotNull(op);
        Assert.Equal(12, op.At);
        Assert.Equal(3, op.Run);
        Assert.Equal("value", op.Value);
    }

    [Fact]
    public void Parse_AValue_Ok()
    {
        Assert.True(BaseVarOperation.TryParse("12=\"value\"",
            _set, out ReplaceBaseVarOperation? op));
        Assert.NotNull(op);
        Assert.Equal(12, op.At);
        Assert.Equal(1, op.Run);
        Assert.Equal("value", op.Value);
    }

    [Fact]
    public void Parse_AValueTags_Ok()
    {
        Assert.True(BaseVarOperation.TryParse("12=\"value\" [alpha=1 beta=2]",
            _set, out ReplaceBaseVarOperation? op));
        Assert.NotNull(op);
        Assert.Equal(12, op.At);
        Assert.Equal(1, op.Run);
        Assert.Equal("value", op.Value);
        Assert.Equal(1, op.TaggedOrdinals["alpha"]);
        Assert.Equal(2, op.TaggedOrdinals["beta"]);
    }

    [Fact]
    public void Parse_ARValueRank_Ok()
    {
        Assert.True(BaseVarOperation.TryParse("12x3=\"value\"^2",
            _set, out ReplaceBaseVarOperation? op));
        Assert.NotNull(op);
        Assert.Equal(12, op.At);
        Assert.Equal(3, op.Run);
        Assert.Equal("value", op.Value);
        Assert.Equal(2, op.Rank);
    }

    [Fact]
    public void Parse_ARValueRankTags_Ok()
    {
        Assert.True(BaseVarOperation.TryParse("12x3=\"value\"^2 [alpha=1 beta=2]",
            _set, out ReplaceBaseVarOperation? op));
        Assert.NotNull(op);
        Assert.Equal(12, op.At);
        Assert.Equal(3, op.Run);
        Assert.Equal("value", op.Value);
        Assert.Equal(2, op.Rank);
        Assert.Equal(1, op.TaggedOrdinals["alpha"]);
        Assert.Equal(2, op.TaggedOrdinals["beta"]);
    }

    [Fact]
    public void Parse_ARValueGroup_Ok()
    {
        Assert.True(BaseVarOperation.TryParse("12x3=\"value\"@group",
            _set, out ReplaceBaseVarOperation? op));
        Assert.NotNull(op);
        Assert.Equal(12, op.At);
        Assert.Equal(3, op.Run);
        Assert.Equal("value", op.Value);
        Assert.Equal("group", op.GroupId);
    }

    [Fact]
    public void Parse_ARValueGroupTags_Ok()
    {
        Assert.True(BaseVarOperation.TryParse(
            "12x3=\"value\"@group [alpha=1 beta=2]",
            _set, out ReplaceBaseVarOperation? op));
        Assert.NotNull(op);
        Assert.Equal(12, op.At);
        Assert.Equal(3, op.Run);
        Assert.Equal("value", op.Value);
        Assert.Equal("group", op.GroupId);
        Assert.Equal(1, op.TaggedOrdinals["alpha"]);
        Assert.Equal(2, op.TaggedOrdinals["beta"]);
    }

    [Fact]
    public void Parse_ARValueRankGroup_Ok()
    {
        Assert.True(BaseVarOperation.TryParse("12x3=\"value\"^2@group",
            _set, out ReplaceBaseVarOperation? op));
        Assert.NotNull(op);
        Assert.Equal(12, op.At);
        Assert.Equal(3, op.Run);
        Assert.Equal(2, op.Rank);
        Assert.Equal("group", op.GroupId);
    }

    [Fact]
    public void Parse_ARValueRankGroupTags_Ok()
    {
        Assert.True(BaseVarOperation.TryParse(
            "12x3=\"value\"^2@group [alpha=1 beta=2]",
            _set, out ReplaceBaseVarOperation? op));
        Assert.NotNull(op);
        Assert.Equal(12, op.At);
        Assert.Equal(3, op.Run);
        Assert.Equal(2, op.Rank);
        Assert.Equal("group", op.GroupId);
        Assert.Equal(1, op.TaggedOrdinals["alpha"]);
        Assert.Equal(2, op.TaggedOrdinals["beta"]);
    }

    [Fact]
    public void Parse_ARValueNote_Ok()
    {
        Assert.True(BaseVarOperation.TryParse("12x3=\"value\" {note=\"a note\"}",
            _set, out ReplaceBaseVarOperation? op));
        Assert.NotNull(op);
        Assert.Equal(12, op.At);
        Assert.Equal(3, op.Run);
        Assert.Equal("value", op.Value);
        Assert.Equal("a note", op.Features.FirstOrDefault(
            f => f.Name == "note")?.Value);
    }

    [Fact]
    public void Parse_ARValueTagsNote_Ok()
    {
        Assert.True(BaseVarOperation.TryParse(
            "12x3=\"value\" [alpha=1 beta=2] {note=\"a note\"}",
            _set, out ReplaceBaseVarOperation? op));
        Assert.NotNull(op);
        Assert.Equal(12, op.At);
        Assert.Equal(3, op.Run);
        Assert.Equal("value", op.Value);
        Assert.Equal("a note", op.Features.FirstOrDefault(
            f => f.Name == "note")?.Value);
        Assert.Equal(1, op.TaggedOrdinals["alpha"]);
        Assert.Equal(2, op.TaggedOrdinals["beta"]);
    }

    [Fact]
    public void Parse_ARValueRankNote_Ok()
    {
        Assert.True(BaseVarOperation.TryParse(
            "12x3=\"value\"^2 {note=\"a note\"}",
            _set, out ReplaceBaseVarOperation? op));
        Assert.NotNull(op);
        Assert.Equal(12, op.At);
        Assert.Equal(3, op.Run);
        Assert.Equal("value", op.Value);
        Assert.Equal(2, op.Rank);
        Assert.Equal("a note", op.Features.FirstOrDefault(
            f => f.Name == "note")?.Value);
    }

    [Fact]
    public void Parse_ARValueRanTagsNote_Ok()
    {
        Assert.True(BaseVarOperation.TryParse(
            "12x3=\"value\"^2 [alpha=1 beta=2] {note=\"a note\"}",
            _set, out ReplaceBaseVarOperation? op));
        Assert.NotNull(op);
        Assert.Equal(12, op.At);
        Assert.Equal(3, op.Run);
        Assert.Equal("value", op.Value);
        Assert.Equal(2, op.Rank);
        Assert.Equal("a note", op.Features.FirstOrDefault(
            f => f.Name == "note")?.Value);
        Assert.Equal(1, op.TaggedOrdinals["alpha"]);
        Assert.Equal(2, op.TaggedOrdinals["beta"]);
    }

    [Fact]
    public void Parse_ARValueRankGroupNote_Ok()
    {
        Assert.True(BaseVarOperation.TryParse(
            "12x3=\"value\"^2@group {note=\"a note\"}",
            _set, out ReplaceBaseVarOperation? op));
        Assert.NotNull(op);
        Assert.Equal(12, op.At);
        Assert.Equal(3, op.Run);
        Assert.Equal("value", op.Value);
        Assert.Equal(2, op.Rank);
        Assert.Equal("group", op.GroupId);
        Assert.Equal("a note", op.Features.FirstOrDefault(
            f => f.Name == "note")?.Value);
    }

    [Fact]
    public void Parse_ARValueRankGroupTagsNote_Ok()
    {
        Assert.True(BaseVarOperation.TryParse(
            "12x3=\"value\"^2@group [alpha=1 beta=2] {note=\"a note\"}",
            _set, out ReplaceBaseVarOperation? op));
        Assert.NotNull(op);
        Assert.Equal(12, op.At);
        Assert.Equal(3, op.Run);
        Assert.Equal("value", op.Value);
        Assert.Equal(2, op.Rank);
        Assert.Equal("group", op.GroupId);
        Assert.Equal("a note", op.Features.FirstOrDefault(
            f => f.Name == "note")?.Value);
        Assert.Equal(1, op.TaggedOrdinals["alpha"]);
        Assert.Equal(2, op.TaggedOrdinals["beta"]);
    }

    [Fact]
    public void Parse_ARValueGroupNote_Ok()
    {
        Assert.True(BaseVarOperation.TryParse(
            "12x3=\"value\"@group {note=\"a note\"}",
            _set, out ReplaceBaseVarOperation? op));
        Assert.NotNull(op);
        Assert.Equal(12, op.At);
        Assert.Equal(3, op.Run);
        Assert.Equal("value", op.Value);
        Assert.Equal("group", op.GroupId);
        Assert.Equal("a note", op.Features.FirstOrDefault(
            f => f.Name == "note")?.Value);
    }

    [Fact]
    public void Parse_ARValueGroupTagsNote_Ok()
    {
        Assert.True(BaseVarOperation.TryParse(
            "12x3=\"value\"@group [alpha=1 beta=2] {note=\"a note\"}",
            _set, out ReplaceBaseVarOperation? op));
        Assert.NotNull(op);
        Assert.Equal(12, op.At);
        Assert.Equal(3, op.Run);
        Assert.Equal("value", op.Value);
        Assert.Equal("group", op.GroupId);
        Assert.Equal("a note", op.Features.FirstOrDefault(
            f => f.Name == "note")?.Value);
        Assert.Equal(1, op.TaggedOrdinals["alpha"]);
        Assert.Equal(2, op.TaggedOrdinals["beta"]);
    }

    [Fact]
    public void Execute_Start()
    {
        CharNodeSet set = new("abcde");
        List<Feature> features = [];
        Assert.True(BaseVarOperation.TryParse("1x2=\"X\"",
            set, out ReplaceBaseVarOperation? op));
        Assert.NotNull(op);

        op.Execute(set.ToList(), features);

        Assert.Equal("Xcde", set.GetText());
        Assert.Empty(features);
    }
}
