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
            out ReplaceVarOperation? _));
    }

    [Fact]
    public void Parse_AValue_Ok()
    {
        Assert.True(VarOperation.TryParse("12=\"value\"",
            out ReplaceVarOperation? _));
    }

    [Fact]
    public void Parse_ARValueRank_Ok()
    {
        Assert.True(VarOperation.TryParse("12x3=\"value\"^2",
            out ReplaceVarOperation? op));
        Assert.NotNull(op);
        Assert.Equal(2, op.Rank);
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