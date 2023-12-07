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
        context.SetAnnNodes(1, 5);

        op.Execute(context);

        Assert.Equal("abcde", context.GetAnnText());

        for (int n = 1; n <= 2; n++)
        {
            CharNode? node = context.Set.GetNode(n);
            Assert.NotNull(node);
            Assert.Single(node.Features);
            Assert.Equal("sample", node.Features[0].Value);
        }
    }
}
