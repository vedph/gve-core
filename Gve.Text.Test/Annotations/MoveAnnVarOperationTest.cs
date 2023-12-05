using Gve.Text.Annotations;
using System.Collections.Generic;
using Xunit;

namespace Gve.Text.Test.Annotations;

public sealed class MoveAnnVarOperationTest
{
    [Fact]
    public void Execute_Start()
    {
        VarOperationContext context = new(new("abcde"));
        List<Feature> features = [];
        Assert.True(AnnVarOperation.TryParse("1>3",
            out MoveAnnVarOperation? op));
        Assert.NotNull(op);

        op.Execute(context);

        Assert.Equal("bacde", context.GetTreeText());
        Assert.Empty(features);
    }

    [Fact]
    public void Execute_Mid()
    {
        VarOperationContext context = new(new("abcde"));
        Assert.True(AnnVarOperation.TryParse("3>2",
            out MoveAnnVarOperation? op));
        Assert.NotNull(op);

        op.Execute(context);

        Assert.Equal("acbde", context.GetTreeText());
    }

    [Fact]
    public void Execute_End()
    {
        VarOperationContext context = new(new("abcde"));
        Assert.True(AnnVarOperation.TryParse("5>2",
            out MoveAnnVarOperation? op));
        Assert.NotNull(op);

        op.Execute(context);

        Assert.Equal("aebcd", context.GetTreeText());
    }
}
