using Gve.Text.Base;
using Xunit;

namespace Gve.Text.Test.Base;

public sealed class MoveBaseVarOperationTest
{
    [Fact]
    public void Execute_Start()
    {
        VarOperationContext context = new(new("abcde"));
        Assert.True(BaseVarOperation.TryParse("1>3",
                                  out MoveBaseVarOperation? op));
        Assert.NotNull(op);

        op.Execute(context);

        Assert.Equal("bacde", context.Set.GetText());
        Assert.Empty(context.Features);
    }

    [Fact]
    public void Execute_Mid()
    {
        VarOperationContext context = new(new("abcde"));
        Assert.True(BaseVarOperation.TryParse("3>2",
                                  out MoveBaseVarOperation? op));
        Assert.NotNull(op);

        op.Execute(context);

        Assert.Equal("acbde", context.Set.GetText());
        Assert.Empty(context.Features);
    }

    [Fact]
    public void Execute_End()
    {
        VarOperationContext context = new(new("abcde"));
        Assert.True(BaseVarOperation.TryParse("5>2",
                                  out MoveBaseVarOperation? op));
        Assert.NotNull(op);

        op.Execute(context);

        Assert.Equal("aebcd", context.Set.GetText());
        Assert.Empty(context.Features);
    }
}
