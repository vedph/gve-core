using Gve.Text.Annotations;
using Xunit;

namespace Gve.Text.Test.Annotations;

public sealed class AddLeftAnnVarOperationTest
{
    [Fact]
    public void Execute_Start()
    {
        VarOperationContext context = new(new("abcde"));
        Assert.True(AnnVarOperation.TryParse("1+<\"X\"",
            out AddLeftAnnVarOperation? op));
        Assert.NotNull(op);

        op.Execute(context);

        Assert.Equal("Xabcde", context.GetAnnText());
    }

    [Fact]
    public void Execute_Mid()
    {
        VarOperationContext context = new(new("abcde"));
        Assert.True(AnnVarOperation.TryParse("3+<\"X\"",
            out AddLeftAnnVarOperation? op));
        Assert.NotNull(op);

        op.Execute(context);

        Assert.Equal("abXcde", context.GetAnnText());
    }

    [Fact]
    public void Execute_End()
    {
        VarOperationContext context = new(new("abcde"));
        Assert.True(AnnVarOperation.TryParse("5+<\"X\"",
            out AddLeftAnnVarOperation? op));
        Assert.NotNull(op);

        op.Execute(context);

        Assert.Equal("abcdXe", context.GetAnnText());
    }
}
