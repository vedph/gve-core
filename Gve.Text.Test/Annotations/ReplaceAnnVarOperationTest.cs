using Gve.Text.Annotations;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Gve.Text.Test.Annotations;

public sealed class ReplaceAnnVarOperationTest
{
    [Fact]
    public void Execute_Start()
    {
        CharNodeSet set = new("abcde");
        List<Feature> features = [];
        Assert.True(AnnVarOperation.TryParse("1x2=\"X\"",
            out ReplaceAnnVarOperation? op));
        Assert.NotNull(op);

        op.Execute(set.ToList(), features);

        Assert.Equal("Xcde", set.GetText());
        Assert.Empty(features);
    }
}
