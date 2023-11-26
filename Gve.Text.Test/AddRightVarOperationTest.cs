using Xunit;

namespace Gve.Text.Test;

public sealed class AddRightVarOperationTest
{
    [Fact]
    public void Execute_Start()
    {
        AddRightVarOperation op =
            VarOperation.Parse<AddRightVarOperation>("1+>\"X\"");

        AnnotatedText result = op.Execute("abcde");

        Assert.Equal("aXbcde", result.Value);
        Assert.Empty(result.Annotations);
    }

    [Fact]
    public void Execute_Mid()
    {
        AddRightVarOperation op =
            VarOperation.Parse<AddRightVarOperation>("3+>\"X\"");

        AnnotatedText result = op.Execute("abcde");

        Assert.Equal("abcXde", result.Value);
        Assert.Empty(result.Annotations);
    }

    [Fact]
    public void Execute_End()
    {
        AddRightVarOperation op =
            VarOperation.Parse<AddRightVarOperation>("5+>\"X\"");

        AnnotatedText result = op.Execute("abcde");

        Assert.Equal("abcdeX", result.Value);
        Assert.Empty(result.Annotations);
    }
}
