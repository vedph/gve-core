using Gve.Text.Annotations;
using System.Collections.Generic;
using Xunit;

namespace Gve.Text.Test.Annotations;

public sealed class ReplaceAnnVarOperationTest
{
    [Fact]
    public void Execute_WithFeatures()
    {
        VarOperationContext context = new(new("abcde"));
        Assert.True(AnnVarOperation.TryParse("1x2=\"X\" {note=sample}",
            out ReplaceAnnVarOperation? op));
        Assert.NotNull(op);

        op.Execute(context);

        Assert.Equal("Xcde", context.GetTreeText());
        Assert.Single(context.Features);
        Assert.Equal("sample", context.Features[0].Value);
    }

    [Fact]
    public void Execute_Start()
    {
        VarOperationContext context = new(new("abcde"));
        List<Feature> features = [];
        Assert.True(AnnVarOperation.TryParse("1x2=\"X\"",
            out ReplaceAnnVarOperation? op));
        Assert.NotNull(op);

        op.Execute(context);

        Assert.Equal("Xcde", context.GetTreeText());
        Assert.Empty(features);
    }

    [Fact]
    public void Execute_Mid()
    {
        VarOperationContext context = new(new("abcde"));
        Assert.True(AnnVarOperation.TryParse("3=\"X\"",
            out ReplaceAnnVarOperation? op));
        Assert.NotNull(op);

        op.Execute(context);

        Assert.Equal("abXde", context.GetTreeText());
        Assert.Empty(context.Features);
    }

    [Fact]
    public void Execute_End()
    {
        VarOperationContext context = new(new("abcde"));
        Assert.True(AnnVarOperation.TryParse("5=\"X\"",
            out ReplaceAnnVarOperation? op));
        Assert.NotNull(op);

        op.Execute(context);

        Assert.Equal("abcdX", context.GetTreeText());
        Assert.Empty(context.Features);
    }

    [Fact]
    public void Execute_Shorter()
    {
        VarOperationContext context = new(new("abcde"));
        Assert.True(AnnVarOperation.TryParse("3x2=\"X\"",
            out ReplaceAnnVarOperation? op));
        Assert.NotNull(op);

        op.Execute(context);

        Assert.Equal("abXe", context.GetTreeText());
        Assert.Empty(context.Features);
    }

    [Fact]
    public void Execute_Longer()
    {
        VarOperationContext context = new(new("abcde"));
        Assert.True(AnnVarOperation.TryParse("3=\"XY\"",
            out ReplaceAnnVarOperation? op));
        Assert.NotNull(op);

        op.Execute(context);

        Assert.Equal("abXYde", context.GetTreeText());
        Assert.Empty(context.Features);
    }
}
