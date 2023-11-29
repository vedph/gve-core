using Xunit;

namespace Gve.Text.Test;

public class ReplaceVarOperationTest
{
    [Fact]
    public void Parse_Empty_Invalid()
    {
        Assert.False(VarOperation.TryParse("", out ReplaceVarOperation? _));
    }

    [Fact]
    public void Parse_Invalid_Invalid()
    {
        Assert.False(VarOperation.TryParse("invalid", out ReplaceVarOperation? _));
    }

    [Fact]
    public void Parse_ARValue_Ok()
    {
        Assert.True(VarOperation.TryParse("12x3=\"value\"",
            out ReplaceVarOperation? op));
        Assert.NotNull(op);
        Assert.Equal(12, op.At);
        Assert.Equal(3, op.Run);
        Assert.Equal("value", op.Value);
    }

    [Fact]
    public void Parse_AValue_Ok()
    {
        Assert.True(VarOperation.TryParse("12=\"value\"",
            out ReplaceVarOperation? op));
        Assert.NotNull(op);
        Assert.Equal(12, op.At);
        Assert.Equal(1, op.Run);
        Assert.Equal("value", op.Value);
    }

    [Fact]
    public void Parse_AValueTags_Ok()
    {
        Assert.True(VarOperation.TryParse("12=\"value\" [alpha beta]",
            out ReplaceVarOperation? op));
        Assert.NotNull(op);
        Assert.Equal(12, op.At);
        Assert.Equal(1, op.Run);
        Assert.Equal("value", op.Value);
        Assert.Contains("alpha", op.VersionTags);
        Assert.Contains("beta", op.VersionTags);
    }

    [Fact]
    public void Parse_ARValueRank_Ok()
    {
        Assert.True(VarOperation.TryParse("12x3=\"value\"^2",
            out ReplaceVarOperation? op));
        Assert.NotNull(op);
        Assert.Equal(12, op.At);
        Assert.Equal(3, op.Run);
        Assert.Equal("value", op.Value);
        Assert.Equal(2, op.Rank);
    }

    [Fact]
    public void Parse_ARValueRankTags_Ok()
    {
        Assert.True(VarOperation.TryParse("12x3=\"value\"^2 [alpha beta]",
            out ReplaceVarOperation? op));
        Assert.NotNull(op);
        Assert.Equal(12, op.At);
        Assert.Equal(3, op.Run);
        Assert.Equal("value", op.Value);
        Assert.Equal(2, op.Rank);
        Assert.Contains("alpha", op.VersionTags);
        Assert.Contains("beta", op.VersionTags);
    }

    [Fact]
    public void Parse_ARValueGroup_Ok()
    {
        Assert.True(VarOperation.TryParse("12x3=\"value\"@group",
            out ReplaceVarOperation? op));
        Assert.NotNull(op);
        Assert.Equal(12, op.At);
        Assert.Equal(3, op.Run);
        Assert.Equal("value", op.Value);
        Assert.Equal("group", op.GroupId);
    }

    [Fact]
    public void Parse_ARValueGroupTags_Ok()
    {
        Assert.True(VarOperation.TryParse("12x3=\"value\"@group [alpha beta]",
            out ReplaceVarOperation? op));
        Assert.NotNull(op);
        Assert.Equal(12, op.At);
        Assert.Equal(3, op.Run);
        Assert.Equal("value", op.Value);
        Assert.Equal("group", op.GroupId);
        Assert.Contains("alpha", op.VersionTags);
        Assert.Contains("beta", op.VersionTags);
    }

    [Fact]
    public void Parse_ARValueRankGroup_Ok()
    {
        Assert.True(VarOperation.TryParse("12x3=\"value\"^2@group",
            out ReplaceVarOperation? op));
        Assert.NotNull(op);
        Assert.Equal(12, op.At);
        Assert.Equal(3, op.Run);
        Assert.Equal(2, op.Rank);
        Assert.Equal("group", op.GroupId);
    }

    [Fact]
    public void Parse_ARValueRankGroupTags_Ok()
    {
        Assert.True(VarOperation.TryParse("12x3=\"value\"^2@group [alpha beta]",
            out ReplaceVarOperation? op));
        Assert.NotNull(op);
        Assert.Equal(12, op.At);
        Assert.Equal(3, op.Run);
        Assert.Equal(2, op.Rank);
        Assert.Equal("group", op.GroupId);
        Assert.Contains("alpha", op.VersionTags);
        Assert.Contains("beta", op.VersionTags);
    }

    [Fact]
    public void Parse_ARValueNote_Ok()
    {
        Assert.True(VarOperation.TryParse("12x3=\"value\" {a note}",
            out ReplaceVarOperation? op));
        Assert.NotNull(op);
        Assert.Equal(12, op.At);
        Assert.Equal(3, op.Run);
        Assert.Equal("value", op.Value);
        Assert.Equal("a note", op.Note);
    }

    [Fact]
    public void Parse_ARValueTagsNote_Ok()
    {
        Assert.True(VarOperation.TryParse("12x3=\"value\" [alpha beta] {a note}",
            out ReplaceVarOperation? op));
        Assert.NotNull(op);
        Assert.Equal(12, op.At);
        Assert.Equal(3, op.Run);
        Assert.Equal("value", op.Value);
        Assert.Equal("a note", op.Note);
        Assert.Contains("alpha", op.VersionTags);
        Assert.Contains("beta", op.VersionTags);
    }

    [Fact]
    public void Parse_ARValueRankNote_Ok()
    {
        Assert.True(VarOperation.TryParse("12x3=\"value\"^2 {a note}",
            out ReplaceVarOperation? op));
        Assert.NotNull(op);
        Assert.Equal(12, op.At);
        Assert.Equal(3, op.Run);
        Assert.Equal("value", op.Value);
        Assert.Equal(2, op.Rank);
        Assert.Equal("a note", op.Note);
    }

    [Fact]
    public void Parse_ARValueRanTagsNote_Ok()
    {
        Assert.True(VarOperation.TryParse("12x3=\"value\"^2 [alpha beta] {a note}",
            out ReplaceVarOperation? op));
        Assert.NotNull(op);
        Assert.Equal(12, op.At);
        Assert.Equal(3, op.Run);
        Assert.Equal("value", op.Value);
        Assert.Equal(2, op.Rank);
        Assert.Equal("a note", op.Note);
        Assert.Contains("alpha", op.VersionTags);
        Assert.Contains("beta", op.VersionTags);
    }

    [Fact]
    public void Parse_ARValueRankGroupNote_Ok()
    {
        Assert.True(VarOperation.TryParse("12x3=\"value\"^2@group {a note}",
            out ReplaceVarOperation? op));
        Assert.NotNull(op);
        Assert.Equal(12, op.At);
        Assert.Equal(3, op.Run);
        Assert.Equal("value", op.Value);
        Assert.Equal(2, op.Rank);
        Assert.Equal("group", op.GroupId);
        Assert.Equal("a note", op.Note);
    }

    [Fact]
    public void Parse_ARValueRankGroupTagsNote_Ok()
    {
        Assert.True(VarOperation.TryParse(
            "12x3=\"value\"^2@group [alpha beta] {a note}",
            out ReplaceVarOperation? op));
        Assert.NotNull(op);
        Assert.Equal(12, op.At);
        Assert.Equal(3, op.Run);
        Assert.Equal("value", op.Value);
        Assert.Equal(2, op.Rank);
        Assert.Equal("group", op.GroupId);
        Assert.Equal("a note", op.Note);
        Assert.Contains("alpha", op.VersionTags);
        Assert.Contains("beta", op.VersionTags);
    }

    [Fact]
    public void Parse_ARValueGroupNote_Ok()
    {
        Assert.True(VarOperation.TryParse("12x3=\"value\"@group {a note}",
            out ReplaceVarOperation? op));
        Assert.NotNull(op);
        Assert.Equal(12, op.At);
        Assert.Equal(3, op.Run);
        Assert.Equal("value", op.Value);
        Assert.Equal("group", op.GroupId);
        Assert.Equal("a note", op.Note);
    }

    [Fact]
    public void Parse_ARValueGroupTagsNote_Ok()
    {
        Assert.True(VarOperation.TryParse("12x3=\"value\"@group [alpha beta] {a note}",
            out ReplaceVarOperation? op));
        Assert.NotNull(op);
        Assert.Equal(12, op.At);
        Assert.Equal(3, op.Run);
        Assert.Equal("value", op.Value);
        Assert.Equal("group", op.GroupId);
        Assert.Equal("a note", op.Note);
        Assert.Contains("alpha", op.VersionTags);
        Assert.Contains("beta", op.VersionTags);
    }

    [Fact]
    public void Execute_Start()
    {
        ReplaceVarOperation op =
            VarOperation.Parse<ReplaceVarOperation>("1x2=\"X\"");

        AnnotatedText result = op.Execute("abcde");

        Assert.Equal("Xcde", result.Value);
        Assert.Empty(result.Annotations);
    }

    [Fact]
    public void Execute_Mid()
    {
        ReplaceVarOperation op =
            VarOperation.Parse<ReplaceVarOperation>("3x2=\"X\"");

        AnnotatedText result = op.Execute("abcde");

        Assert.Equal("abXe", result.Value);
        Assert.Empty(result.Annotations);
    }

    [Fact]
    public void Execute_End()
    {
        ReplaceVarOperation op =
            VarOperation.Parse<ReplaceVarOperation>("4x2=\"X\"");

        AnnotatedText result = op.Execute("abcde");

        Assert.Equal("abcX", result.Value);
        Assert.Empty(result.Annotations);
    }
}