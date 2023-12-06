using Gve.Text.Annotations;
using System.Collections.Generic;
using Xunit;

namespace Gve.Text.Test.Annotations;

public sealed class SwapAnnVarOperationTest
{
    [Fact]
    public void Execute_Start()
    {
        VarOperationContext context = new(new("abcde"));
        List<Feature> features = [];
        Assert.True(AnnVarOperation.TryParse("1x3<>5",
            out SwapAnnVarOperation? op));
        Assert.NotNull(op);

        op.Execute(context);

        Assert.Equal("edabc", context.GetAnnText());
        Assert.Empty(features);
    }

    [Fact]
    public void Execute_Mid()
    {
        VarOperationContext context = new(new("abcde"));
        Assert.True(AnnVarOperation.TryParse("2x2<>5",
            out SwapAnnVarOperation? op));
        Assert.NotNull(op);

        op.Execute(context);

        Assert.Equal("aedbc", context.GetAnnText());
    }

    [Fact]
    public void Execute_End()
    {
        VarOperationContext context = new(new("abcde"));
        Assert.True(AnnVarOperation.TryParse("5<>2",
            out SwapAnnVarOperation? op));
        Assert.NotNull(op);

        op.Execute(context);

        Assert.Equal("aecdb", context.GetAnnText());
    }
}
