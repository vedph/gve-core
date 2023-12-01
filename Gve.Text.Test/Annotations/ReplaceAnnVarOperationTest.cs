using Gve.Text.Annotations;
using System.Collections.Generic;
using Xunit;

namespace Gve.Text.Test.Annotations;

public sealed class ReplaceAnnVarOperationTest
{
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
}
