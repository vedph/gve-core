using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Gve.Text.Test;

public sealed class ChainTest
{
    private static string GetVersionString(IList<ChainNode<char>> nodes) =>
        new(nodes.Select(n => n.Data).ToArray());

    #region GetDistinctTags
    [Fact]
    public void GetDistinctTags_Empty()
    {
        Chain<char> chain = new();
        IList<string> tags = chain.GetDistinctTags();
        Assert.Empty(tags);
    }

    [Fact]
    public void GetDistinctTags_Not_Empty()
    {
        Chain<char> chain = new();
        chain.Append("", "v0", new ChainNode<char>("1", 1, "A", 'A'));
        chain.Append("v0", "v0", new ChainNode<char>("2", 2, "B", 'B'));

        chain.AddVersion("v0", "v1");
        chain.Append("v0", "v1", new ChainNode<char>("2", 3, "C", 'C'));

        IList<string> tags = chain.GetDistinctTags();

        Assert.Equal(2, tags.Count);
        Assert.Contains("v0", tags);
        Assert.Contains("v1", tags);
    }
    #endregion

    #region GetMaxOrdinal
    [Fact]
    public void GetMaxOrdinal_Empty()
    {
        Chain<char> chain = new();
        int max = chain.GetMaxOrdinal();
        Assert.Equal(0, max);
    }

    [Fact]
    public void GetMaxOrdinal_NotEmpty()
    {
        Chain<char> chain = new();
        chain.Append("", "v0", new ChainNode<char>("1", 1, "A", 'A'));
        chain.Append("v0", "v0", new ChainNode<char>("2", 2, "B", 'B'));

        chain.AddVersion("v0", "v1");
        chain.Append("v0", "v1", new ChainNode<char>("2", 3, "C", 'C'));

        int max = chain.GetMaxOrdinal();
        Assert.Equal(3, max);
    }
    #endregion

    #region GetFirstNode
    [Fact]
    public void GetFirstNode_Empty()
    {
        Chain<char> chain = new();

        ChainNode<char>? firstNode = chain.GetFirstNode("v0");

        Assert.Null(firstNode);
    }

    [Fact]
    public void GetFirstNode_SingleNode()
    {
        Chain<char> chain = new();
        chain.Append("", "v0", new ChainNode<char>("1", 1, "A", 'A'));

        ChainNode<char>? firstNode = chain.GetFirstNode("v0");

        Assert.NotNull(firstNode);
        Assert.Equal("1", firstNode.Id);
    }

    [Fact]
    public void GetFirstNode_MultipleNodes()
    {
        Chain<char> chain = new();
        chain.Append("", "v0", new ChainNode<char>("1", 1, "A", 'A'));
        chain.Append("v0", "v0", new ChainNode<char>("2", 2, "B", 'B'));

        ChainNode<char>? firstNode = chain.GetFirstNode("v0");

        Assert.NotNull(firstNode);
        Assert.Equal("1", firstNode.Id);
    }

    [Fact]
    public void GetFirstNode_NonExistingTag()
    {
        Chain<char> chain = new();
        chain.Append("", "v0", new ChainNode<char>("1", 1, "A", 'A'));
        chain.Append("v0", "v0", new ChainNode<char>("2", 2, "B", 'B'));

        ChainNode<char>? firstNode = chain.GetFirstNode("v1");

        Assert.Null(firstNode);
    }
    #endregion

    #region GetLastNode
    [Fact]
    public void GetLastNode_Empty()
    {
        Chain<char> chain = new();

        ChainNode<char>? lastNode = chain.GetLastNode("v0");

        Assert.Null(lastNode);
    }

    [Fact]
    public void GetLastNode_SingleNode()
    {
        Chain<char> chain = new();
        chain.Append("", "v0", new ChainNode<char>("1", 1, "A", 'A'));

        ChainNode<char>? lastNode = chain.GetLastNode("v0");

        Assert.NotNull(lastNode);
        Assert.Equal("1", lastNode.Id);
    }

    [Fact]
    public void GetLastNode_MultipleNodes()
    {
        Chain<char> chain = new();
        chain.Append("", "v0", new ChainNode<char>("1", 1, "A", 'A'));
        chain.Append("v0", "v0", new ChainNode<char>("2", 2, "B", 'B'));

        ChainNode<char>? lastNode = chain.GetLastNode("v0");

        Assert.NotNull(lastNode);
        Assert.Equal("2", lastNode.Id);
    }

    [Fact]
    public void GetLastNode_NonExistingTag()
    {
        Chain<char> chain = new();
        chain.Append("", "v0", new ChainNode<char>("1", 1, "A", 'A'));
        chain.Append("v0", "v0", new ChainNode<char>("2", 2, "B", 'B'));

        ChainNode<char>? lastNode = chain.GetLastNode("v1");

        Assert.Null(lastNode);
    }
    #endregion

    #region GetNodeLeftLink
    [Fact]
    public void GetNodeLeftLink_NullNode()
    {
        Chain<char> chain = new();
        chain.Append("", "v0", new ChainNode<char>("1", 1, "A", 'A'));

        ChainLink<char>? link = chain.GetNodeLeftLink("v0", null);

        Assert.Null(link);
    }

    [Fact]
    public void GetNodeLeftLink_NotNullNode()
    {
        Chain<char> chain = new();
        ChainNode<char> nodeA = new("1", 1, "A", 'A');
        ChainNode<char> nodeB = new("2", 2, "B", 'B');
        chain.Append("", "v0", nodeA);
        chain.Append("v0", "v0", nodeB);

        ChainLink<char>? link = chain.GetNodeLeftLink("v0", nodeB);

        Assert.NotNull(link);
        Assert.Equal(nodeA, link.Source);
        Assert.Equal(nodeB, link.Target);
    }

    [Fact]
    public void GetNodeLeftLink_NonExistingTag()
    {
        Chain<char> chain = new();
        ChainNode<char> nodeA = new("1", 1, "A", 'A');
        ChainNode<char> nodeB = new("2", 2, "B", 'B');
        chain.Append("", "v0", nodeA);
        chain.Append("v0", "v0", nodeB);

        ChainLink<char>? link = chain.GetNodeLeftLink("v1", nodeB);

        Assert.Null(link);
    }
    #endregion

    #region Append
    [Fact]
    public void Append_Empty()
    {
        Chain<char> chain = new();

        chain.Append("", "v0", new ChainNode<char>("1", 1, "A", 'A'));

        ChainNode<char>? node = chain.GetTagNodes("v0").FirstOrDefault();
        Assert.NotNull(node);
        Assert.Equal("1", node.Id);
    }

    [Fact]
    public void Append_NotEmptySameVersion()
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

    [Fact]
    public void Append_NotEmptyDifferentVersion()
    {
        Chain<char> chain = new();

        chain.Append("", "v0", new ChainNode<char>("1", 1, "A", 'A'));
        chain.Append("v0", "v0", new ChainNode<char>("2", 2, "B", 'B'));

        chain.AddVersion("v0", "v1");
        chain.Append("v0", "v1", new ChainNode<char>("2", 3, "C", 'C'));

        string v0 = GetVersionString(chain.GetTagNodes("v0"));
        Assert.Equal("AB", v0);

        string v1 = GetVersionString(chain.GetTagNodes("v1"));
        Assert.Equal("ABC", v1);
    }
    #endregion
}
