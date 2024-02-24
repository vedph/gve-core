using Fusi.Tools.Data;
using Gve.Text.Annotations;
using Xunit;

namespace Gve.Text.Test;

public sealed class VarOperationEntryTest
{
    [Fact]
    public void Parse_Ok()
    {
        const string text =
            "- 2=\"\" [a=2 b=2]\n" +
            "  - 1=\"V\" [a]\n" +
            "    - 1=\"B\" [a]\n" +
            "  - 1=\"P\" [b]\n";

        VarOperationEntry? entry = VarOperationEntry.Parse(text);

        Assert.NotNull(entry);
        Assert.Equal(2, entry.Operation.TaggedOrdinals.Count);

        Assert.NotNull(entry.Root);

        // root has no payload and 2 children
        Assert.Null(entry.Root.Data);
        Assert.Equal(2, entry.Root.Children.Count);

        // 1st child has payload and 1 child
        TreeNode<AnnVarOperation> node = entry.Root.Children[0];
        Assert.NotNull(node.Data);
        Assert.Single(node.Children);
        Assert.IsType<ReplaceAnnVarOperation>(node.Data);

        // child of 1st child has payload and no children
        node = node.Children[0];
        Assert.NotNull(node.Data);
        Assert.Empty(node.Children);
        Assert.IsType<ReplaceAnnVarOperation>(node.Data);

        // 2nd child has payload and no children
        node = entry.Root.Children[1];
        Assert.NotNull(node.Data);
        Assert.Empty(node.Children);
        Assert.IsType<ReplaceAnnVarOperation>(node.Data);
    }
}
