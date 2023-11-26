using Xunit;

namespace Gve.Text.Test;

public sealed class SwapVarOperationTest
{
    [Fact]
    public void Execute_Start()
    {
        SwapVarOperation op =
            VarOperation.Parse<SwapVarOperation>("1x3<>5");

        AnnotatedText result = op.Execute("abcde");

        Assert.Equal("edabc", result.Value);
        Assert.Empty(result.Annotations);
    }

    [Fact]
    public void Execute_Mid()
    {
        SwapVarOperation op =
            VarOperation.Parse<SwapVarOperation>("2x2<>5");

        AnnotatedText result = op.Execute("abcde");

        Assert.Equal("aedbc", result.Value);
        Assert.Empty(result.Annotations);
    }

    [Fact]
    public void Execute_End()
    {
        SwapVarOperation op =
            VarOperation.Parse<SwapVarOperation>("5<>2");

        AnnotatedText result = op.Execute("abcde");

        Assert.Equal("aecdb", result.Value);
        Assert.Empty(result.Annotations);
    }
}
