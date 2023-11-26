using Xunit;

namespace Gve.Text.Test;

public sealed class AddLeftVarOperationTest
{
    [Fact]
    public void Execute_Start()
    {
        AddLeftVarOperation op =
            VarOperation.Parse<AddLeftVarOperation>("1+<\"X\"");

        AnnotatedText result = op.Execute("abcde");

        Assert.Equal("Xabcde", result.Value);
        Assert.Empty(result.Annotations);
    }

    [Fact]
    public void Execute_Mid()
    {
        AddLeftVarOperation op =
            VarOperation.Parse<AddLeftVarOperation>("3+<\"X\"");

        AnnotatedText result = op.Execute("abcde");

        Assert.Equal("abXcde", result.Value);
        Assert.Empty(result.Annotations);
    }

    [Fact]
    public void Execute_End()
    {
        AddLeftVarOperation op =
            VarOperation.Parse<AddLeftVarOperation>("5+<\"X\"");

        AnnotatedText result = op.Execute("abcde");

        Assert.Equal("abcdXe", result.Value);
        Assert.Empty(result.Annotations);
    }
}
