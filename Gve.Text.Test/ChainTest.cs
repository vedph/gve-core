using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Gve.Text.Test;

public sealed class ChainTest
{
    private static string GetVersionString(IList<ChainNode<char>> nodes) =>
        new(nodes.Select(n => n.Data).ToArray());

    private static List<ChainNode<char>> GetNodesFrom(string text)
    {
        return text.Select((c, _) =>
            new ChainNode<char>(c, $"{(char.IsWhiteSpace(c) ? '_' : c)}"))
            .ToList();
    }

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
        chain.Append("", "v0", new ChainNode<char>('A', "A", 1));
        chain.Append("v0", "v0", new ChainNode<char>('B', "B", 2));

        chain.AddVersion("v0", "v1");
        chain.Append("v0", "v1", new ChainNode<char>('C', "C", 3));

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
        chain.Append("", "v0", new ChainNode<char>('A', "A", 1));
        chain.Append("v0", "v0", new ChainNode<char>('B', "B", 2));

        chain.AddVersion("v0", "v1");
        chain.Append("v0", "v1", new ChainNode<char>('C', "C", 3));

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
        chain.Append("", "v0", new ChainNode<char>('A', "A", 1));

        ChainNode<char>? firstNode = chain.GetFirstNode("v0");

        Assert.NotNull(firstNode);
        Assert.Equal(1, firstNode.Id);
    }

    [Fact]
    public void GetFirstNode_MultipleNodes()
    {
        Chain<char> chain = new();
        chain.Append("", "v0", new ChainNode<char>('A', "A", 1));
        chain.Append("v0", "v0", new ChainNode<char>('B', "B", 2));

        ChainNode<char>? firstNode = chain.GetFirstNode("v0");

        Assert.NotNull(firstNode);
        Assert.Equal(1, firstNode.Id);
    }

    [Fact]
    public void GetFirstNode_NonExistingTag()
    {
        Chain<char> chain = new();
        chain.Append("", "v0", new ChainNode<char>('A', "A", 1));
        chain.Append("v0", "v0", new ChainNode<char>('B', "B", 2));

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
        chain.Append("", "v0", new ChainNode<char>('A', "A", 1));

        ChainNode<char>? lastNode = chain.GetLastNode("v0");

        Assert.NotNull(lastNode);
        Assert.Equal(1, lastNode.Id);
    }

    [Fact]
    public void GetLastNode_MultipleNodes()
    {
        Chain<char> chain = new();
        chain.Append("", "v0", new ChainNode<char>('A', "A", 1));
        chain.Append("v0", "v0", new ChainNode<char>('B', "B", 2));

        ChainNode<char>? lastNode = chain.GetLastNode("v0");

        Assert.NotNull(lastNode);
        Assert.Equal(2, lastNode.Id);
    }

    [Fact]
    public void GetLastNode_NonExistingTag()
    {
        Chain<char> chain = new();
        chain.Append("", "v0", new ChainNode<char>('A', "A", 1));
        chain.Append("v0", "v0", new ChainNode<char>('B', "B", 2));

        ChainNode<char>? lastNode = chain.GetLastNode("v1");

        Assert.Null(lastNode);
    }
    #endregion

    #region GetNodeLeftLink
    [Fact]
    public void GetNodeLeftLink_NullNode_Null()
    {
        Chain<char> chain = new();
        chain.Append("", "v0", new ChainNode<char>('A', "A", 1));

        ChainLink<char>? link = chain.GetNodeLeftLink("v0", null);

        Assert.Null(link);
    }

    [Fact]
    public void GetNodeLeftLink_FirstNode_Null()
    {
        Chain<char> chain = new();
        List<ChainNode<char>> nodes = GetNodesFrom("ABC");
        chain.AppendRange("", "v0", nodes);

        Assert.Null(chain.GetNodeLeftLink("v0", nodes[0]));
    }

    [Fact]
    public void GetNodeLeftLink_NotNullNode()
    {
        Chain<char> chain = new();
        List<ChainNode<char>> nodes = GetNodesFrom("ABC");
        chain.AppendRange("", "v0", nodes);

        ChainLink<char>? link = chain.GetNodeLeftLink("v0", nodes[1]);

        Assert.NotNull(link);
        Assert.Equal(nodes[0], link.Source);
        Assert.Equal(nodes[1], link.Target);
    }

    [Fact]
    public void GetNodeLeftLink_NonExistingTag()
    {
        Chain<char> chain = new();
        List<ChainNode<char>> nodes = GetNodesFrom("ABC");
        chain.AppendRange("", "v0", nodes);

        ChainLink<char>? link = chain.GetNodeLeftLink("v1", nodes[1]);

        Assert.Null(link);
    }
    #endregion

    #region GetLeftLink
    [Fact]
    public void GetLeftLink_NullNode_Null()
    {
        Chain<char> chain = new();
        List<ChainNode<char>> nodes = GetNodesFrom("ABC");
        chain.AppendRange("", "v0", nodes);

        ChainLink<char>? link = chain.GetLeftLink("v0", null);

        Assert.Null(link);
    }

    [Fact]
    public void GetLeftLink_FirstNode_Null()
    {
        Chain<char> chain = new();
        List<ChainNode<char>> nodes = GetNodesFrom("ABC");
        chain.AppendRange("", "v0", nodes);

        ChainLink<char>? link = chain.GetLeftLink("v0",
            chain.GetNodeLeftLink("v0", nodes[0]));

        Assert.Null(link);
    }

    [Fact]
    public void GetLeftLink_NonNullNode()
    {
        Chain<char> chain = new();
        List<ChainNode<char>> nodes = GetNodesFrom("ABC");
        chain.AppendRange("", "v0", nodes);
        ChainLink<char>? refLink = chain.GetNodeRightLink("v0", nodes[1]);

        ChainLink<char>? link = chain.GetLeftLink("v0", refLink);

        Assert.NotNull(link);
        Assert.Equal(nodes[0], link.Source);
        Assert.Equal(nodes[1], link.Target);
    }

    [Fact]
    public void GetLeftLink_NonExistingTag()
    {
        Chain<char> chain = new();
        List<ChainNode<char>> nodes = GetNodesFrom("ABC");
        ChainLink<char>? refLink = chain.GetNodeRightLink("v0", nodes[1]);

        ChainLink<char>? link = chain.GetLeftLink("v1", refLink);

        Assert.Null(link);
    }
    #endregion

    #region Append
    [Fact]
    public void Append_Empty()
    {
        Chain<char> chain = new();

        chain.Append("", "v0", new ChainNode<char>('A', "A", 1));

        ChainNode<char>? node = chain.GetTagNodes("v0").FirstOrDefault();
        Assert.NotNull(node);
        Assert.Equal(1, node.Id);
    }

    [Fact]
    public void Append_NotEmptySameVersion()
    {
        Chain<char> chain = new();

        chain.Append("", "v0", new ChainNode<char>('A', "A", 1));

        chain.Append("v0", "v0", new ChainNode<char>('B', "B", 2));

        IList<ChainNode<char>> nodes = chain.GetTagNodes("v0");
        Assert.Equal(2, nodes.Count);

        ChainNode<char>? node = nodes[0];
        Assert.NotNull(node);
        Assert.Equal(1, node.Id);

        node = nodes[1];
        Assert.NotNull(node);
        Assert.Equal(2, node.Id);
    }

    [Fact]
    public void Append_NotEmptyDifferentVersion()
    {
        Chain<char> chain = new();

        chain.Append("", "v0", new ChainNode<char>('A', "A", 1));
        chain.Append("v0", "v0", new ChainNode<char>('B', "B", 2));

        chain.AddVersion("v0", "v1");
        chain.Append("v0", "v1", new ChainNode<char>('C', "C", 3));

        string v0 = GetVersionString(chain.GetTagNodes("v0"));
        Assert.Equal("AB", v0);

        string v1 = GetVersionString(chain.GetTagNodes("v1"));
        Assert.Equal("ABC", v1);
    }
    #endregion
}
