namespace Gve.Text.Test;

public class ReplaceVarOperationTest
{
    [Fact]
    public void Parse_Empty_Invalid()
    {
        Assert.False(VarOperation.TryParse("", out ReplaceVarOperation op));
    }

    [Fact]
    public void Execute_Start()
    {
        ReplaceVarOperation op =
            (ReplaceVarOperation)VarOperation.Parse("1x2=\"X\"");

        AnnotatedText result = op.Execute("abcde");

        Assert.Equal("Xcde", result.Value);
        Assert.Empty(result.Annotations);
    }

    [Fact]
    public void Execute_Mid()
    {
        ReplaceVarOperation op =
            (ReplaceVarOperation)VarOperation.Parse("3x2=\"X\"");

        AnnotatedText result = op.Execute("abcde");

        Assert.Equal("abXe", result.Value);
        Assert.Empty(result.Annotations);
    }

    [Fact]
    public void Execute_End()
    {
        ReplaceVarOperation op =
            (ReplaceVarOperation)VarOperation.Parse("4x2=\"X\"");

        AnnotatedText result = op.Execute("abcde");

        Assert.Equal("abcX", result.Value);
        Assert.Empty(result.Annotations);
    }
}