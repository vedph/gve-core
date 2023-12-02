using Gve.Text.Annotations;
using Xunit;

namespace Gve.Text.Test.Annotations;

public sealed class AddRightAnnVarOperationTest
{
    [Fact]
    public void Execute_Start()
    {
        VarOperationContext context = new(new("abcde"));
        Assert.True(AnnVarOperation.TryParse("1+>\"X\"",
                       out AddRightAnnVarOperation? op));
        Assert.NotNull(op);

        op.Execute(context);

        Assert.Equal("aXbcde", context.GetTreeText());
        Assert.Empty(context.Features);
    }

    [Fact]
    public void Execute_Mid()
    {
        VarOperationContext context = new(new("abcde"));
        Assert.True(AnnVarOperation.TryParse("3+>\"X\"",
                       out AddRightAnnVarOperation? op));
        Assert.NotNull(op);

        op.Execute(context);

        Assert.Equal("abcXde", context.GetTreeText());
        Assert.Empty(context.Features);
    }

    [Fact]
    public void Execute_End()
    {
        VarOperationContext context = new(new("abcde"));
        Assert.True(AnnVarOperation.TryParse("5+>\"X\"",
                       out AddRightAnnVarOperation? op));
        Assert.NotNull(op);

        op.Execute(context);

        Assert.Equal("abcdeX", context.GetTreeText());
        Assert.Empty(context.Features);
    }
}
