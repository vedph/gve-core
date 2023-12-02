using Gve.Text.Base;
using Xunit;

namespace Gve.Text.Test.Base;

public sealed class AddRightBaseVarOperationTest
{
    [Fact]
    public void Execute_Start()
    {
        VarOperationContext context = new(new("abcde"));
        Assert.True(BaseVarOperation.TryParse("1+>\"X\"",
                       out AddRightBaseVarOperation? op));
        Assert.NotNull(op);

        op.Execute(context);

        Assert.Equal("aXbcde", context.Set.GetText());
        Assert.Empty(context.Features);
    }

    [Fact]
    public void Execute_Mid()
    {
        VarOperationContext context = new(new("abcde"));
        Assert.True(BaseVarOperation.TryParse("3+>\"X\"",
                       out AddRightBaseVarOperation? op));
        Assert.NotNull(op);

        op.Execute(context);

        Assert.Equal("abcXde", context.Set.GetText());
        Assert.Empty(context.Features);
    }

    [Fact]
    public void Execute_End()
    {
        VarOperationContext context = new(new("abcde"));
        Assert.True(BaseVarOperation.TryParse("5+>\"X\"",
                       out AddRightBaseVarOperation? op));
        Assert.NotNull(op);

        op.Execute(context);

        Assert.Equal("abcdeX", context.Set.GetText());
        Assert.Empty(context.Features);
    }
}
