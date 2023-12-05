using Gve.Text.Base;
using Xunit;

namespace Gve.Text.Test.Base;

public sealed class SwapBaseVarOperationTest
{
    [Fact]
    public void Execute_Start()
    {
        VarOperationContext context = new(new("abcde"));
        Assert.True(BaseVarOperation.TryParse("1x3<>5",
            out SwapBaseVarOperation? op));
        Assert.NotNull(op);

        op.Execute(context);

        Assert.Equal("edabc", context.Set.GetText());
    }

    [Fact]
    public void Execute_Mid()
    {
        VarOperationContext context = new(new("abcde"));
        Assert.True(BaseVarOperation.TryParse("2x2<>5",
            out SwapBaseVarOperation? op));
        Assert.NotNull(op);

        op.Execute(context);

        Assert.Equal("aedbc", context.Set.GetText());
    }

    [Fact]
    public void Execute_End()
    {
        VarOperationContext context = new(new("abcde"));
        Assert.True(BaseVarOperation.TryParse("5<>2",
            out SwapBaseVarOperation? op));
        Assert.NotNull(op);

        op.Execute(context);

        Assert.Equal("aecdb", context.Set.GetText());
    }
}
