using Gve.Text.Base;
using Xunit;

namespace Gve.Text.Test.Base;

public sealed class AddLeftBaseVarOperationTest
{
    [Fact]
    public void Execute_Start()
    {
        VarOperationContext context = new(new("abcde"));
        Assert.True(BaseVarOperation.TryParse("1+<\"X\"",
            out AddLeftBaseVarOperation? op));
        Assert.NotNull(op);

        op.Execute(context);

        Assert.Equal("Xabcde", context.Set.GetText());
        Assert.Empty(context.Features);
    }

    [Fact]
    public void Execute_Mid()
    {
        VarOperationContext context = new(new("abcde"));
        Assert.True(BaseVarOperation.TryParse("3+<\"X\"",
            out AddLeftBaseVarOperation? op));
        Assert.NotNull(op);

        op.Execute(context);

        Assert.Equal("abXcde", context.Set.GetText());
        Assert.Empty(context.Features);
    }

    [Fact]
    public void Execute_End()
    {
        VarOperationContext context = new(new("abcde"));
        Assert.True(BaseVarOperation.TryParse("5+<\"X\"",
            out AddLeftBaseVarOperation? op));
        Assert.NotNull(op);

        op.Execute(context);

        Assert.Equal("abcdXe", context.Set.GetText());
        Assert.Empty(context.Features);
    }
}
