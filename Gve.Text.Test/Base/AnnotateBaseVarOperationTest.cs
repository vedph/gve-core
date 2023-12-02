using Gve.Text.Base;
using Xunit;

namespace Gve.Text.Test.Base;

public sealed class AnnotateBaseVarOperationTest
{
    [Fact]
    public void Execute_Ok()
    {
        VarOperationContext context = new(new("abcde"));
        Assert.True(BaseVarOperation.TryParse("1x2: {note=sample}",
            out AnnotateBaseVarOperation? op));
        Assert.NotNull(op);

        op.Execute(context);

        Assert.Equal("abcde", context.Set.GetText());
        Assert.Single(context.Features);
        Assert.Equal("sample", context.Features[0].Value);
    }
}
