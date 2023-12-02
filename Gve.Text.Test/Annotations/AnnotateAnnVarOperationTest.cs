using Gve.Text.Annotations;
using Xunit;

namespace Gve.Text.Test.Annotations;

public sealed class AnnotateAnnVarOperationTest
{
    [Fact]
    public void Execute_Ok()
    {
        VarOperationContext context = new(new("abcde"));
        Assert.True(AnnVarOperation.TryParse("1x2: {note=sample}",
            out AnnotateAnnVarOperation? op));
        Assert.NotNull(op);

        op.Execute(context);

        Assert.Equal("abcde", context.GetTreeText());
        Assert.Single(context.Features);
        Assert.Equal("sample", context.Features[0].Value);
    }
}
