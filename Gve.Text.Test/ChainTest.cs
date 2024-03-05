using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Gve.Text.Test;

public sealed class ChainTest
{
    [Fact]
    public void Append_Empty_Ok()
    {
        Chain<char> chain = new();

        chain.Append("", "v0", new ChainNode<char>("1", 1, "A", 'A'));

        ChainNode<char>? node = chain.GetTagNodes("v0").FirstOrDefault();
        Assert.NotNull(node);
        Assert.Equal("1", node.Id);
    }

    [Fact]
    public void Append_NotEmpty_Ok()
    {
        Chain<char> chain = new();

        chain.Append("", "v0", new ChainNode<char>("1", 1, "A", 'A'));
        chain.Append("v0", "v0", new ChainNode<char>("2", 2, "B", 'B'));

        IList<ChainNode<char>> nodes = chain.GetTagNodes("v0");
        Assert.Equal(2, nodes.Count);

        ChainNode<char>? node = nodes[0];
        Assert.NotNull(node);
        Assert.Equal("1", node.Id);

        node = nodes[1];
        Assert.NotNull(node);
        Assert.Equal("2", node.Id);
    }
}
