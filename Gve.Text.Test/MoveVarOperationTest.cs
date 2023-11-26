using Xunit;

namespace Gve.Text.Test;

public sealed class MoveVarOperationTest
{
    [Fact]
    public void Execute_Start()
    {
        MoveVarOperation op =
            VarOperation.Parse<MoveVarOperation>("1>3");

        AnnotatedText result = op.Execute("abcde");

        Assert.Equal("bcade", result.Value);
        Assert.Empty(result.Annotations);
    }

    [Fact]
    public void Execute_Mid()
    {
        MoveVarOperation op =
            VarOperation.Parse<MoveVarOperation>("3>2");

        AnnotatedText result = op.Execute("abcde");

        Assert.Equal("acbde", result.Value);
        Assert.Empty(result.Annotations);
    }

    [Fact]
    public void Execute_End()
    {
        MoveVarOperation op =
            VarOperation.Parse<MoveVarOperation>("5>2");

        AnnotatedText result = op.Execute("abcde");

        Assert.Equal("aebcd", result.Value);
        Assert.Empty(result.Annotations);
    }
}
