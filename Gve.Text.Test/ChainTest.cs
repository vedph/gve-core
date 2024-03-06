using System;
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

    #region GetNodeCount
    [Fact]
    public void GetNodeCount_NoTag_Total()
    {
        Chain<char> chain = new();
        chain.AppendRange("", "v1", GetNodesFrom("ABC"));
        chain.AppendRange("", "v2", GetNodesFrom("DE"));

        int n = chain.GetNodeCount();

        Assert.Equal(5, n);
    }

    [Fact]
    public void GetNodeCount_WithTag_Filtered()
    {
        Chain<char> chain = new();
        chain.AppendRange("", "v1", GetNodesFrom("ABC"));
        chain.AppendRange("", "v2", GetNodesFrom("DE"));

        Assert.Equal(3, chain.GetNodeCount("v1"));
        Assert.Equal(2, chain.GetNodeCount("v2"));
    }
    #endregion

    #region GetLinkCount
    [Fact]
    public void GetLinkCount_NoTag_Total()
    {
        Chain<char> chain = new();
        chain.AppendRange("", "v1", GetNodesFrom("ABC"));
        chain.AppendRange("", "v2", GetNodesFrom("DE"));

        int n = chain.GetLinkCount();

        // there should be one less link than nodes
        Assert.Equal(4, n);
    }

    [Fact]
    public void GetLinkCount_WithTag_Filtered()
    {
        Chain<char> chain = new();
        chain.AppendRange("", "v1", GetNodesFrom("ABC"));
        chain.AppendRange("", "v2", GetNodesFrom("DE"));

        // there should be one less link than nodes
        Assert.Equal(2, chain.GetLinkCount("v1"));
        Assert.Equal(1, chain.GetLinkCount("v2"));
    }
    #endregion

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

    #region GetNodeRightLink
    [Fact]
    public void GetNodeRightLink_NullNode_Null()
    {
        Chain<char> chain = new();
        List<ChainNode<char>> nodes = GetNodesFrom("ABC");
        chain.AppendRange("", "v0", nodes);

        ChainLink<char>? link = chain.GetNodeRightLink("v0", null);

        Assert.Null(link);
    }

    [Fact]
    public void GetNodeRightLink_LastNode_Null()
    {
        Chain<char> chain = new();
        List<ChainNode<char>> nodes = GetNodesFrom("ABC");
        chain.AppendRange("", "v0", nodes);

        Assert.Null(chain.GetNodeRightLink("v0", nodes[^1]));
    }

    [Fact]
    public void GetNodeRightLink_NotNullNode()
    {
        Chain<char> chain = new();
        List<ChainNode<char>> nodes = GetNodesFrom("ABC");
        chain.AppendRange("", "v0", nodes);

        ChainLink<char>? link = chain.GetNodeRightLink("v0", nodes[1]);

        Assert.NotNull(link);
        Assert.Equal(nodes[1], link.Source);
        Assert.Equal(nodes[2], link.Target);
    }

    [Fact]
    public void GetNodeRightLink_NonExistingTag()
    {
        Chain<char> chain = new();
        List<ChainNode<char>> nodes = GetNodesFrom("ABC");
        chain.AppendRange("", "v0", nodes);

        ChainLink<char>? link = chain.GetNodeRightLink("v1", nodes[1]);

        Assert.Null(link);
    }
    #endregion

    #region GetRightLink
    [Fact]
    public void GetRightLink_NullNode_Null()
    {
        Chain<char> chain = new();
        List<ChainNode<char>> nodes = GetNodesFrom("ABC");
        chain.AppendRange("", "v0", nodes);

        ChainLink<char>? link = chain.GetRightLink("v0", null);

        Assert.Null(link);
    }

    [Fact]
    public void GetRightLink_LastNode_Null()
    {
        Chain<char> chain = new();
        List<ChainNode<char>> nodes = GetNodesFrom("ABC");
        chain.AppendRange("", "v0", nodes);

        ChainLink<char>? link = chain.GetRightLink("v0",
                       chain.GetNodeRightLink("v0", nodes[^1]));

        Assert.Null(link);
    }

    [Fact]
    public void GetRightLink_NonNullNode()
    {
        Chain<char> chain = new();
        List<ChainNode<char>> nodes = GetNodesFrom("ABC");
        chain.AppendRange("", "v0", nodes);
        ChainLink<char>? refLink = chain.GetNodeLeftLink("v0", nodes[1]);

        ChainLink<char>? link = chain.GetRightLink("v0", refLink);

        Assert.NotNull(link);
        Assert.Equal(nodes[1], link.Source);
        Assert.Equal(nodes[2], link.Target);
    }

    [Fact]
    public void GetRightLink_NonExistingTag()
    {
        Chain<char> chain = new();
        List<ChainNode<char>> nodes = GetNodesFrom("ABC");
        ChainLink<char>? refLink = chain.GetNodeLeftLink("v0", nodes[1]);

        ChainLink<char>? link = chain.GetRightLink("v1", refLink);

        Assert.Null(link);
    }
    #endregion

    #region EnumerateTagNodes
    [Fact]
    public void EnumerateTagNodes_Empty()
    {
        Chain<char> chain = new();
        IList<ChainNode<char>> nodes = chain.GetTagNodes("v0");
        Assert.Empty(nodes);
    }

    [Fact]
    public void EnumerateTagNodes_NotEmpty()
    {
        Chain<char> chain = new();
        List<ChainNode<char>> nodes = GetNodesFrom("ABC");
        chain.AppendRange("", "v0", nodes);

        IList<ChainNode<char>> nodes2 = chain.GetTagNodes("v0");

        Assert.Equal(3, nodes2.Count);
        Assert.Equal('A', nodes2[0].Data);
        Assert.Equal('B', nodes2[1].Data);
        Assert.Equal('C', nodes2[2].Data);
    }
    #endregion

    #region BuildLinksFrom
    [Fact]
    public void BuildLinksFrom_EmptyNodes_Empty()
    {
        List<ChainNode<char>> nodes = [];

        IList<ChainLink<char>> result = Chain<char>.BuildLinksFrom("tag", nodes);

        Assert.Empty(result);
    }

    [Fact]
    public void BuildLinksFrom_SingleNode_Empty()
    {
        List<ChainNode<char>> nodes = GetNodesFrom("t");

        IList<ChainLink<char>> result = Chain<char>.BuildLinksFrom("tag", nodes);

        Assert.Empty(result);
    }

    [Fact]
    public void BuildLinksFrom_MultipleNodes_ReturnsLinks()
    {
        List<ChainNode<char>> nodes = GetNodesFrom("test");

        IList<ChainLink<char>> result = Chain<char>.BuildLinksFrom("tag", nodes);

        Assert.Equal(nodes.Count - 1, result.Count);
        for (int i = 0; i < result.Count; i++)
        {
            Assert.Equal("tag", result[i].Tag);
            Assert.Equal(nodes[i], result[i].Source);
            Assert.Equal(nodes[i + 1], result[i].Target);
        }
    }
    #endregion

    #region Delete
    [Fact]
    public void Delete_NodeNotConnected_False()
    {
        Chain<char> chain = new();

        Assert.False(chain.Delete("", "v1", new('A')));
    }

    [Fact]
    public void Delete_InnerNode_True()
    {
        Chain<char> chain = new();
        List<ChainNode<char>> nodes = GetNodesFrom("ABC");
        chain.AppendRange("", "v1", nodes);

        Assert.True(chain.Delete("v1", "v2", nodes[1]));

        IList<ChainNode<char>> nodes2 = chain.GetTagNodes("v2");
        Assert.Equal(2, nodes2.Count);
        Assert.Equal('A', nodes2[0].Data);
        Assert.Equal('C', nodes2[1].Data);
    }

    [Fact]
    public void Delete_FirstNode_True()
    {
        Chain<char> chain = new();
        List<ChainNode<char>> nodes = GetNodesFrom("ABC");
        chain.AppendRange("", "v1", nodes);

        Assert.True(chain.Delete("v1", "v2", nodes[0]));

        IList<ChainNode<char>> nodes2 = chain.GetTagNodes("v2");
        Assert.Equal(2, nodes2.Count);
        Assert.Equal('B', nodes2[0].Data);
        Assert.Equal('C', nodes2[1].Data);
    }

    [Fact]
    public void Delete_LastNode_True()
    {
        Chain<char> chain = new();
        List<ChainNode<char>> nodes = GetNodesFrom("ABC");
        chain.AppendRange("", "v1", nodes);

        Assert.True(chain.Delete("v1", "v2", nodes[2]));

        IList<ChainNode<char>> nodes2 = chain.GetTagNodes("v2");
        Assert.Equal(2, nodes2.Count);
        Assert.Equal('A', nodes2[0].Data);
        Assert.Equal('B', nodes2[1].Data);
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

    #region AppendRange
    [Fact]
    public void AppendRange_Empty()
    {
        Chain<char> chain = new();
        chain.AppendRange("", "v1", GetNodesFrom("abc"));

        Assert.Equal(3, chain.GetNodeCount("v1"));
        Assert.Equal("abc", GetVersionString(chain.GetTagNodes("v1")));
    }

    [Fact]
    public void AppendRange_NonEmptyChainSameVersion()
    {
        Chain<char> chain = new();
        chain.Append("", "v1", new ChainNode<char>('a'));
        chain.AppendRange("v1", "v1", GetNodesFrom("bc"));

        Assert.Equal(3, chain.GetNodeCount("v1"));
        Assert.Equal("abc", GetVersionString(chain.GetTagNodes("v1")));
    }

    [Fact]
    public void AppendRange_NonEmptyChainDifferentVersion()
    {
        Chain<char> chain = new();
        chain.Append("", "v1", new ChainNode<char>('a'));
        chain.AppendRange("v1", "v2", GetNodesFrom("bc"));

        Assert.Equal(1, chain.GetNodeCount("v1"));
        Assert.Equal(3, chain.GetNodeCount("v2"));
        Assert.Equal("a", GetVersionString(chain.GetTagNodes("v1")));
        Assert.Equal("abc", GetVersionString(chain.GetTagNodes("v2")));
    }

    [Fact]
    public void AppendRange_EmptyNodes()
    {
        Chain<char> chain = new();
        chain.Append("v1", "v1", new ChainNode<char>('a'));
        List<ChainNode<char>> nodes = [];
        chain.AppendRange("v1", "v2", nodes);

        Assert.Equal(1, chain.GetNodeCount("v1"));
        Assert.Equal(1, chain.GetNodeCount("v2"));
        Assert.Equal("a", GetVersionString(chain.GetTagNodes("v1")));
        Assert.Equal("a", GetVersionString(chain.GetTagNodes("v2")));
    }
    #endregion
}
