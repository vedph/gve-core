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

        for (int n = 1; n <= 2; n++)
        {
            CharNode? node = context.Set.GetNode(n);
            Assert.NotNull(node);
            Assert.Single(node.Features);
            Assert.Equal("sample", node.Features[0].Value);
        }
    }
}
