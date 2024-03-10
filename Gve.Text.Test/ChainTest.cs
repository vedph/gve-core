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
        chain.AppendRange("v1", "v2", GetNodesFrom("DE"));

        Assert.Equal(5, chain.GetNodeCount());
    }

    [Fact]
    public void GetNodeCount_WithTag_Filtered()
    {
        Chain<char> chain = new();
        chain.AppendRange("", "v1", GetNodesFrom("ABC"));
        chain.AppendRange("v1", "v2", GetNodesFrom("DE"));

        Assert.Equal(3, chain.GetNodeCount("v1"));
        Assert.Equal(5, chain.GetNodeCount("v2"));
    }
    #endregion

    #region GetLinkCount
    [Fact]
    public void GetLinkCount_NoTag_Total()
    {
        Chain<char> chain = new();
        chain.AppendRange("", "v1", GetNodesFrom("ABC"));
        chain.AppendRange("v1", "v2", GetNodesFrom("DE"));

        Assert.Equal(10, chain.GetLinkCount());
    }

    [Fact]
    public void GetLinkCount_WithTag_Filtered()
    {
        Chain<char> chain = new();
        chain.AppendRange("", "v1", GetNodesFrom("ABC"));
        chain.AppendRange("v1", "v2", GetNodesFrom("DE"));

        Assert.Equal(4, chain.GetLinkCount("v1"));
        Assert.Equal(6, chain.GetLinkCount("v2"));
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

    #region IsNodeInVersion
    [Fact]
    public void IsNodeInVersion_NodeNotInVersion_False()
    {
        Chain<char> chain = new();
        ChainNode<char> node = new('A');
        chain.Append("", "v1", node);

        Assert.False(chain.IsNodeInVersion(node, "nonExistingTag"));
    }

    [Fact]
    public void IsNodeInVersion_NodeInVersion_True()
    {
        Chain<char> chain = new();
        ChainNode<char> node = new('A');
        chain.Append("", "v1", node);

        Assert.True(chain.IsNodeInVersion(node, "v1"));
    }
    #endregion

    #region FindNode
    [Fact]
    public void FindNode_EmptyChain_Null()
    {
        Chain<char> chain = new();

        ChainNode<char>? result = chain.FindNode(1);

        Assert.Null(result);
    }

    [Fact]
    public void FindNode_MatchingId_Node()
    {
        Chain<char> chain = new();
        List<ChainNode<char>> nodes = GetNodesFrom("ABC");
        chain.AppendRange("", "v1", nodes);

        ChainNode<char>? result = chain.FindNode(nodes[1].Id);

        Assert.Equal(nodes[1], result);
    }

    [Fact]
    public void FindNode_NonMatchingId_Null()
    {
        Chain<char> chain = new();
        List<ChainNode<char>> nodes = GetNodesFrom("ABC");
        chain.AppendRange("", "v1", nodes);

        ChainNode<char>? result = chain.FindNode(0);

        Assert.Null(result);
    }
    #endregion

    #region FindNodeByIndex
    [Fact]
    public void FindNodeByIndex_NegativeIndex_Throws()
    {
        Chain<char> chain = new();

        Assert.Throws<ArgumentOutOfRangeException>(
            () => chain.FindNodeByIndex("tag", -1));
    }

    [Fact]
    public void FindNodeByIndex_EmptyChain_Null()
    {
        Chain<char> chain = new();

        ChainNode<char>? result = chain.FindNodeByIndex("tag", 0);

        Assert.Null(result);
    }

    [Fact]
    public void FindNodeByIndex_ValidIndexNotExistingVersion_Null()
    {
        Chain<char> chain = new();
        List<ChainNode<char>> nodes = GetNodesFrom("ABC");
        chain.AppendRange("", "v1", nodes);

        ChainNode<char>? result = chain.FindNodeByIndex("v2", 0);

        Assert.Null(result);
    }

    [Fact]
    public void FindNodeByIndex_ValidIndexExistingVersion_Node()
    {
        Chain<char> chain = new();
        List<ChainNode<char>> nodes = GetNodesFrom("ABC");
        chain.AppendRange("", "v1", nodes);

        ChainNode<char>? result = chain.FindNodeByIndex("v1", 2);

        Assert.Equal(nodes[2], result);
    }

    [Fact]
    public void FindNodeByIndex_IndexOutOfRange_Null()
    {
        Chain<char> chain = new();
        List<ChainNode<char>> nodes = GetNodesFrom("test");
        chain.AppendRange("tag", "newTag", nodes);

        ChainNode<char>? result = chain.FindNodeByIndex("tag", 5);

        Assert.Null(result);
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

    #region GetNextNode
    [Fact]
    public void GetNextNode_NonExistingTag_Null()
    {
        Chain<char> chain = new();
        ChainNode<char> node = new('A');
        chain.Append("", "v1", node);

        Assert.Null(chain.GetNextNode("nonExistingTag", node));
    }

    [Fact]
    public void GetNextNode_ExistingTagButNoNextNode_Null()
    {
        Chain<char> chain = new();
        ChainNode<char> node = new('A');
        chain.Append("", "v1", node);

        Assert.Null(chain.GetNextNode("v1", node));
    }

    [Fact]
    public void GetNextNode_ExistingTagAndHasNextNode_NextNode()
    {
        Chain<char> chain = new();
        List<ChainNode<char>> nodes = GetNodesFrom("ABC");
        chain.AppendRange("", "v1", nodes);

        Assert.Equal(nodes[1], chain.GetNextNode("v1", nodes[0]));
    }
    #endregion

    #region GetPreviousNode
    [Fact]
    public void GetPreviousNode_NonExistingTag_Null()
    {
        Chain<char> chain = new();
        ChainNode<char> node = new('A');
        chain.Append("", "v1", node);

        Assert.Null(chain.GetPreviousNode("nonExistingTag", node));
    }

    [Fact]
    public void GetPreviousNode_ExistingTagButNoPreviousNode_Null()
    {
        Chain<char> chain = new();
        ChainNode<char> node = new('A');
        chain.Append("", "v1", node);

        Assert.Null(chain.GetPreviousNode("v1", node));
    }

    [Fact]
    public void GetPreviousNode_ExistingTagAndHasPreviousNode_PreviousNode()
    {
        Chain<char> chain = new();
        List<ChainNode<char>> nodes = GetNodesFrom("ABC");
        chain.AppendRange("", "v1", nodes);

        Assert.Equal(nodes[0], chain.GetPreviousNode("v1", nodes[1]));
    }
    #endregion

    #region IsNodeBefore
    [Fact]
    public void IsNodeBefore_NodeIsNotBefore_False()
    {
        Chain<char> chain = new();
        List<ChainNode<char>> nodes = GetNodesFrom("ABC");
        chain.AppendRange("", "v1", nodes);

        Assert.False(chain.IsNodeBefore("v1", nodes[1], nodes[0]));
    }

    [Fact]
    public void IsNodeBefore_NodeIsBefore_True()
    {
        Chain<char> chain = new();
        List<ChainNode<char>> nodes = GetNodesFrom("ABC");
        chain.AppendRange("", "v1", nodes);

        Assert.True(chain.IsNodeBefore("v1", nodes[0], nodes[2]));
    }
    #endregion

    #region IsNodeAfter
    [Fact]
    public void IsNodeAfter_NodeIsNotAfter_False()
    {
        Chain<char> chain = new();
        List<ChainNode<char>> nodes = GetNodesFrom("ABC");
        chain.AppendRange("", "v1", nodes);

        Assert.False(chain.IsNodeAfter("v1", nodes[0], nodes[1]));
    }

    [Fact]
    public void IsNodeAfter_NodeIsAfter_True()
    {
        Chain<char> chain = new();
        List<ChainNode<char>> nodes = GetNodesFrom("ABC");
        chain.AppendRange("", "v1", nodes);

        Assert.True(chain.IsNodeAfter("v1", nodes[2], nodes[0]));
    }
    #endregion

    #region GetNodePreviousLink
    [Fact]
    public void GetNodePreviousLink_NullNode_Null()
    {
        Chain<char> chain = new();
        chain.Append("", "v0", new ChainNode<char>('A', "A", 1));

        ChainLink<char>? link = chain.GetNodePreviousLink("v0", null);

        Assert.Null(link);
    }

    [Fact]
    public void GetNodePreviousLink_FirstNode_Edge()
    {
        Chain<char> chain = new();
        List<ChainNode<char>> nodes = GetNodesFrom("ABC");
        chain.AppendRange("", "v0", nodes);

        ChainLink<char>? link = chain.GetNodePreviousLink("v0", nodes[0]);

        Assert.NotNull(link);
        Assert.True(link.IsHead);
    }

    [Fact]
    public void GetNodePreviousLink_NotNullNode()
    {
        Chain<char> chain = new();
        List<ChainNode<char>> nodes = GetNodesFrom("ABC");
        chain.AppendRange("", "v0", nodes);

        ChainLink<char>? link = chain.GetNodePreviousLink("v0", nodes[1]);

        Assert.NotNull(link);
        Assert.Equal(nodes[0], link.Source);
        Assert.Equal(nodes[1], link.Target);
    }

    [Fact]
    public void GetNodePreviousLink_NonExistingTag()
    {
        Chain<char> chain = new();
        List<ChainNode<char>> nodes = GetNodesFrom("ABC");
        chain.AppendRange("", "v0", nodes);

        ChainLink<char>? link = chain.GetNodePreviousLink("v1", nodes[1]);

        Assert.Null(link);
    }
    #endregion

    #region GetNodeNextLink
    [Fact]
    public void GetNodeNextLink_NullNode_Null()
    {
        Chain<char> chain = new();
        List<ChainNode<char>> nodes = GetNodesFrom("ABC");
        chain.AppendRange("", "v0", nodes);

        ChainLink<char>? link = chain.GetNodeNextLink("v0", null);

        Assert.Null(link);
    }

    [Fact]
    public void GetNodeNextLink_LastNode_Edge()
    {
        Chain<char> chain = new();
        List<ChainNode<char>> nodes = GetNodesFrom("ABC");
        chain.AppendRange("", "v0", nodes);

        ChainLink<char>? link = chain.GetNodeNextLink("v0", nodes[^1]);

        Assert.NotNull(link);
        Assert.True(link.IsTail);
    }

    [Fact]
    public void GetNodeNextLink_NotNullNode()
    {
        Chain<char> chain = new();
        List<ChainNode<char>> nodes = GetNodesFrom("ABC");
        chain.AppendRange("", "v0", nodes);

        ChainLink<char>? link = chain.GetNodeNextLink("v0", nodes[1]);

        Assert.NotNull(link);
        Assert.Equal(nodes[1], link.Source);
        Assert.Equal(nodes[2], link.Target);
    }

    [Fact]
    public void GetNodeNextLink_NonExistingTag()
    {
        Chain<char> chain = new();
        List<ChainNode<char>> nodes = GetNodesFrom("ABC");
        chain.AppendRange("", "v0", nodes);

        ChainLink<char>? link = chain.GetNodeNextLink("v1", nodes[1]);

        Assert.Null(link);
    }
    #endregion

    #region GetPreviousLink
    [Fact]
    public void GetPreviousLink_NullNode_Null()
    {
        Chain<char> chain = new();
        List<ChainNode<char>> nodes = GetNodesFrom("ABC");
        chain.AppendRange("", "v0", nodes);

        ChainLink<char>? link = chain.GetPreviousLink("v0", null);

        Assert.Null(link);
    }

    [Fact]
    public void GetPreviousLink_FirstNode_Null()
    {
        Chain<char> chain = new();
        List<ChainNode<char>> nodes = GetNodesFrom("ABC");
        chain.AppendRange("", "v0", nodes);

        ChainLink<char>? link = chain.GetPreviousLink("v0",
            chain.GetNodePreviousLink("v0", nodes[0]));

        Assert.Null(link);
    }

    [Fact]
    public void GetPreviousLink_NonNullNode()
    {
        Chain<char> chain = new();
        List<ChainNode<char>> nodes = GetNodesFrom("ABC");
        chain.AppendRange("", "v0", nodes);
        ChainLink<char>? refLink = chain.GetNodeNextLink("v0", nodes[1]);

        ChainLink<char>? link = chain.GetPreviousLink("v0", refLink);

        Assert.NotNull(link);
        Assert.Equal(nodes[0], link.Source);
        Assert.Equal(nodes[1], link.Target);
    }

    [Fact]
    public void GetPreviousLink_NonExistingTag()
    {
        Chain<char> chain = new();
        List<ChainNode<char>> nodes = GetNodesFrom("ABC");
        ChainLink<char>? refLink = chain.GetNodeNextLink("v0", nodes[1]);

        ChainLink<char>? link = chain.GetPreviousLink("v1", refLink);

        Assert.Null(link);
    }
    #endregion

    #region GetNextLink
    [Fact]
    public void GetNextLink_NullNode_Null()
    {
        Chain<char> chain = new();
        List<ChainNode<char>> nodes = GetNodesFrom("ABC");
        chain.AppendRange("", "v0", nodes);

        ChainLink<char>? link = chain.GetNextLink("v0", null);

        Assert.Null(link);
    }

    [Fact]
    public void GetNextLink_LastNode_Null()
    {
        Chain<char> chain = new();
        List<ChainNode<char>> nodes = GetNodesFrom("ABC");
        chain.AppendRange("", "v0", nodes);

        ChainLink<char>? link = chain.GetNextLink("v0",
                       chain.GetNodeNextLink("v0", nodes[^1]));

        Assert.Null(link);
    }

    [Fact]
    public void GetNextLink_NonNullNode()
    {
        Chain<char> chain = new();
        List<ChainNode<char>> nodes = GetNodesFrom("ABC");
        chain.AppendRange("", "v0", nodes);
        ChainLink<char>? refLink = chain.GetNodePreviousLink("v0", nodes[1]);

        ChainLink<char>? link = chain.GetNextLink("v0", refLink);

        Assert.NotNull(link);
        Assert.Equal(nodes[1], link.Source);
        Assert.Equal(nodes[2], link.Target);
    }

    [Fact]
    public void GetNextLink_NonExistingTag()
    {
        Chain<char> chain = new();
        List<ChainNode<char>> nodes = GetNodesFrom("ABC");
        ChainLink<char>? refLink = chain.GetNodePreviousLink("v0", nodes[1]);

        ChainLink<char>? link = chain.GetNextLink("v1", refLink);

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

        Assert.Equal("AC", GetVersionString(chain.GetTagNodes("v2")));
    }

    [Fact]
    public void Delete_InnerNode_SameVersion_True()
    {
        Chain<char> chain = new();
        List<ChainNode<char>> nodes = GetNodesFrom("ABC");
        chain.AppendRange("", "v1", nodes);

        Assert.True(chain.Delete("v1", "v1", nodes[1]));

        Assert.Equal("AC", GetVersionString(chain.GetTagNodes("v1")));
        Assert.Single(chain.GetDistinctTags());
    }

    [Fact]
    public void Delete_FirstNode_True()
    {
        Chain<char> chain = new();
        List<ChainNode<char>> nodes = GetNodesFrom("ABC");
        chain.AppendRange("", "v1", nodes);

        Assert.True(chain.Delete("v1", "v2", nodes[0]));

        Assert.Equal("BC", GetVersionString(chain.GetTagNodes("v2")));
    }

    [Fact]
    public void Delete_FirstNode_SameVersion_True()
    {
        Chain<char> chain = new();
        List<ChainNode<char>> nodes = GetNodesFrom("ABC");
        chain.AppendRange("", "v1", nodes);

        Assert.True(chain.Delete("v1", "v1", nodes[0]));

        Assert.Equal("BC", GetVersionString(chain.GetTagNodes("v1")));
        Assert.Single(chain.GetDistinctTags());
    }

    [Fact]
    public void Delete_LastNode_True()
    {
        Chain<char> chain = new();
        List<ChainNode<char>> nodes = GetNodesFrom("ABC");
        chain.AppendRange("", "v1", nodes);

        Assert.True(chain.Delete("v1", "v2", nodes[2]));

        Assert.Equal("AB", GetVersionString(chain.GetTagNodes("v2")));
    }

    [Fact]
    public void Delete_LastNode_SameVersion_True()
    {
        Chain<char> chain = new();
        List<ChainNode<char>> nodes = GetNodesFrom("ABC");
        chain.AppendRange("", "v1", nodes);

        Assert.True(chain.Delete("v1", "v1", nodes[2]));

        Assert.Equal("AB", GetVersionString(chain.GetTagNodes("v1")));
        Assert.Single(chain.GetDistinctTags());
    }
    #endregion

    #region DeleteRange
    [Fact]
    public void DeleteRange_NodesNotConnected_False()
    {
        Chain<char> chain = new();
        List<ChainNode<char>> nodes = GetNodesFrom("ABC");
        chain.AppendRange("", "v1", nodes);

        Assert.False(chain.DeleteRange("v1", "v2", new('X'), new('Y')));
    }

    [Fact]
    public void DeleteRange_FirstNodes_True()
    {
        Chain<char> chain = new();
        List<ChainNode<char>> nodes = GetNodesFrom("ABC");
        chain.AppendRange("", "v1", nodes);

        Assert.True(chain.DeleteRange("v1", "v2", nodes[0], nodes[1]));

        Assert.Equal("C", GetVersionString(chain.GetTagNodes("v2")));
    }

    [Fact]
    public void DeleteRange_FirstNodes_SameVersion_True()
    {
        Chain<char> chain = new();
        List<ChainNode<char>> nodes = GetNodesFrom("ABC");
        chain.AppendRange("", "v1", nodes);

        Assert.True(chain.DeleteRange("v1", "v1", nodes[0], nodes[1]));

        Assert.Equal("C", GetVersionString(chain.GetTagNodes("v1")));
        Assert.Single(chain.GetDistinctTags());
    }

    [Fact]
    public void DeleteRange_LastNodes_True()
    {
        Chain<char> chain = new();
        List<ChainNode<char>> nodes = GetNodesFrom("ABC");
        chain.AppendRange("", "v1", nodes);

        Assert.True(chain.DeleteRange("v1", "v2", nodes[1], nodes[2]));

        Assert.Equal("A", GetVersionString(chain.GetTagNodes("v2")));
    }

    [Fact]
    public void DeleteRange_LastNodes_SameVersion_True()
    {
        Chain<char> chain = new();
        List<ChainNode<char>> nodes = GetNodesFrom("ABC");
        chain.AppendRange("", "v1", nodes);

        Assert.True(chain.DeleteRange("v1", "v1", nodes[1], nodes[2]));

        Assert.Equal("A", GetVersionString(chain.GetTagNodes("v1")));
        Assert.Single(chain.GetDistinctTags());
    }

    [Fact]
    public void DeleteRange_InnerNodes_True()
    {
        Chain<char> chain = new();
        List<ChainNode<char>> nodes = GetNodesFrom("ABCD");
        chain.AppendRange("", "v1", nodes);

        Assert.True(chain.DeleteRange("v1", "v2", nodes[1], nodes[2]));

        Assert.Equal("AD", GetVersionString(chain.GetTagNodes("v2")));
    }

    [Fact]
    public void DeleteRange_InnerNodes_SameVersion_True()
    {
        Chain<char> chain = new();
        List<ChainNode<char>> nodes = GetNodesFrom("ABCD");
        chain.AppendRange("", "v1", nodes);

        Assert.True(chain.DeleteRange("v1", "v1", nodes[1], nodes[2]));

        Assert.Equal("AD", GetVersionString(chain.GetTagNodes("v1")));
        Assert.Single(chain.GetDistinctTags());
    }

    [Fact]
    public void DeleteRange_AllNodes_True()
    {
        Chain<char> chain = new();
        List<ChainNode<char>> nodes = GetNodesFrom("ABC");
        chain.AppendRange("", "v1", nodes);

        Assert.True(chain.DeleteRange("v1", "v2", nodes[0], nodes[2]));

        Assert.Equal("", GetVersionString(chain.GetTagNodes("v2")));
    }

    [Fact]
    public void DeleteRange_AllNodes_SameVersionTrue()
    {
        Chain<char> chain = new();
        List<ChainNode<char>> nodes = GetNodesFrom("ABC");
        chain.AppendRange("", "v1", nodes);

        Assert.True(chain.DeleteRange("v1", "v1", nodes[0], nodes[2]));

        Assert.Equal("", GetVersionString(chain.GetTagNodes("v1")));
        Assert.Single(chain.GetDistinctTags());
    }
    #endregion

    #region InsertBefore
    [Fact]
    public void InsertBefore_NodeNotConnected_False()
    {
        Chain<char> chain = new();
        ChainNode<char> nodeA = new('A');
        ChainNode<char> nodeB = new('B');
        chain.Append("", "v0", nodeA);

        bool result = chain.InsertBefore("", "v1", nodeB, nodeA);

        Assert.False(result);
    }

    [Fact]
    public void InsertBefore_FirstNode()
    {
        Chain<char> chain = new();
        List<ChainNode<char>> nodes = GetNodesFrom("ABC");
        chain.AppendRange("", "v0", nodes);

        Assert.True(chain.InsertBefore("v0", "v1", nodes[0],
            new ChainNode<char>('X')));
        Assert.Equal("XABC", GetVersionString(chain.GetTagNodes("v1")));
    }

    [Fact]
    public void InsertBefore_LastNode()
    {
        Chain<char> chain = new();
        List<ChainNode<char>> nodes = GetNodesFrom("ABC");
        chain.AppendRange("", "v0", nodes);

        Assert.True(chain.InsertBefore("v0", "v1", nodes[2],
            new ChainNode<char>('X')));
        Assert.Equal("ABXC", GetVersionString(chain.GetTagNodes("v1")));
    }

    [Fact]
    public void InsertBefore_InnerNode()
    {
        Chain<char> chain = new();
        List<ChainNode<char>> nodes = GetNodesFrom("ABC");
        chain.AppendRange("", "v0", nodes);

        Assert.True(chain.InsertBefore("v0", "v1", nodes[1],
            new ChainNode<char>('X')));
        Assert.Equal("AXBC", GetVersionString(chain.GetTagNodes("v1")));
    }
    #endregion

    #region InsertAfter
    [Fact]
    public void InsertAfter_NodeNotConnected_False()
    {
        Chain<char> chain = new();
        List<ChainNode<char>> nodes = GetNodesFrom("ABC");
        chain.AppendRange("", "v0", nodes);

        bool result = chain.InsertAfter("", "v1",
            new ChainNode<char>('X'),
            new ChainNode<char>('Y'));

        Assert.False(result);
    }

    [Fact]
    public void InsertAfter_FirstNode()
    {
        Chain<char> chain = new();
        List<ChainNode<char>> nodes = GetNodesFrom("ABC");
        chain.AppendRange("", "v0", nodes);

        Assert.True(chain.InsertAfter("v0", "v1", nodes[0],
            new ChainNode<char>('X')));
        Assert.Equal("AXBC", GetVersionString(chain.GetTagNodes("v1")));
    }

    [Fact]
    public void InsertAfter_LastNode()
    {
        Chain<char> chain = new();
        List<ChainNode<char>> nodes = GetNodesFrom("ABC");
        chain.AppendRange("", "v0", nodes);

        Assert.True(chain.InsertAfter("v0", "v1", nodes[2],
                       new ChainNode<char>('X')));
        Assert.Equal("ABCX", GetVersionString(chain.GetTagNodes("v1")));
    }

    [Fact]
    public void InsertAfter_InnerNode()
    {
        Chain<char> chain = new();
        List<ChainNode<char>> nodes = GetNodesFrom("ABC");
        chain.AppendRange("", "v0", nodes);

        Assert.True(chain.InsertAfter("v0", "v1", nodes[1],
            new ChainNode<char>('X')));
        Assert.Equal("ABXC", GetVersionString(chain.GetTagNodes("v1")));
    }
    #endregion

    #region InsertRangeBefore
    [Fact]
    public void InsertRangeBefore_NodesNotConnected_False()
    {
        Chain<char> chain = new();
        List<ChainNode<char>> nodes = GetNodesFrom("ABC");
        chain.AppendRange("", "v0", nodes);

        Assert.False(chain.InsertRangeBefore("", "v1", new ChainNode<char>('X'),
            GetNodesFrom("XY")));
    }

    [Fact]
    public void InsertRangeBefore_FirstNode()
    {
        Chain<char> chain = new();
        List<ChainNode<char>> nodes = GetNodesFrom("ABC");
        chain.AppendRange("", "v0", nodes);

        Assert.True(chain.InsertRangeBefore("v0", "v1", nodes[0],
            GetNodesFrom("XY")));

        Assert.Equal("XYABC", GetVersionString(chain.GetTagNodes("v1")));
    }

    [Fact]
    public void InsertRangeBefore_LastNode()
    {
        Chain<char> chain = new();
        List<ChainNode<char>> nodes = GetNodesFrom("ABC");
        chain.AppendRange("", "v0", nodes);

        Assert.True(chain.InsertRangeBefore("v0", "v1", nodes[2],
            GetNodesFrom("XY")));

        Assert.Equal("ABXYC", GetVersionString(chain.GetTagNodes("v1")));
    }

    [Fact]
    public void InsertRangeBefore_InnerNode()
    {
        Chain<char> chain = new();
        List<ChainNode<char>> nodes = GetNodesFrom("ABC");
        chain.AppendRange("", "v0", nodes);

        Assert.True(chain.InsertRangeBefore("v0", "v1", nodes[1],
            GetNodesFrom("XY")));

        Assert.Equal("AXYBC", GetVersionString(chain.GetTagNodes("v1")));
    }
    #endregion

    #region InsertRangeAfter
    [Fact]
    public void InsertRangeAfter_NodesNotConnected_False()
    {
        Chain<char> chain = new();
        List<ChainNode<char>> nodes = GetNodesFrom("ABC");
        chain.AppendRange("", "v0", nodes);

        Assert.False(chain.InsertRangeAfter("", "v1", new ChainNode<char>('X'),
            GetNodesFrom("XY")));
    }

    [Fact]
    public void InsertRangeAfter_FirstNode()
    {
        Chain<char> chain = new();
        List<ChainNode<char>> nodes = GetNodesFrom("ABC");
        chain.AppendRange("", "v0", nodes);

        Assert.True(chain.InsertRangeAfter("v0", "v1", nodes[0],
            GetNodesFrom("XY")));

        Assert.Equal("AXYBC", GetVersionString(chain.GetTagNodes("v1")));
    }

    [Fact]
    public void InsertRangeAfter_LastNode()
    {
        Chain<char> chain = new();
        List<ChainNode<char>> nodes = GetNodesFrom("ABC");
        chain.AppendRange("", "v0", nodes);

        Assert.True(chain.InsertRangeAfter("v0", "v1", nodes[2],
            GetNodesFrom("XY")));

        Assert.Equal("ABCXY", GetVersionString(chain.GetTagNodes("v1")));
    }

    [Fact]
    public void InsertRangeAfter_InnerNode()
    {
        Chain<char> chain = new();
        List<ChainNode<char>> nodes = GetNodesFrom("ABC");
        chain.AppendRange("", "v0", nodes);

        Assert.True(chain.InsertRangeAfter("v0", "v1", nodes[1],
            GetNodesFrom("XY")));

        Assert.Equal("ABXYC", GetVersionString(chain.GetTagNodes("v1")));
    }
    #endregion

    #region Append
    [Fact]
    public void Append_Empty()
    {
        Chain<char> chain = new();

        chain.Append("", "v0", new ChainNode<char>('A'));

        ChainNode<char>? node = chain.GetTagNodes("v0").FirstOrDefault();
        Assert.NotNull(node);
        Assert.Equal(1, node.Id);
    }

    [Fact]
    public void Append_NotEmptySameVersion()
    {
        Chain<char> chain = new();
        chain.Append("", "v0", new ChainNode<char>('A'));

        chain.Append("v0", "v0", new ChainNode<char>('B'));

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
        chain.AppendRange("", "v0", GetNodesFrom("AB"));

        chain.Append("v0", "v1", new ChainNode<char>('C'));

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
        chain.AppendRange("", "v1", GetNodesFrom("ABC"));

        Assert.Equal(3, chain.GetNodeCount("v1"));
        Assert.Equal(4, chain.GetLinkCount("v1"));
        Assert.Equal("ABC", GetVersionString(chain.GetTagNodes("v1")));
    }

    [Fact]
    public void AppendRange_NonEmptyChainSameVersion()
    {
        Chain<char> chain = new();
        chain.Append("", "v1", new ChainNode<char>('A'));
        chain.AppendRange("v1", "v2", GetNodesFrom("BC"));

        Assert.Equal(3, chain.GetNodeCount("v2"));
        Assert.Equal("ABC", GetVersionString(chain.GetTagNodes("v2")));
    }

    [Fact]
    public void AppendRange_NonEmptyChainDifferentVersion()
    {
        Chain<char> chain = new();
        chain.Append("", "v1", new ChainNode<char>('A'));
        chain.AppendRange("v1", "v2", GetNodesFrom("BC"));

        Assert.Equal(1, chain.GetNodeCount("v1"));
        Assert.Equal(3, chain.GetNodeCount("v2"));
        Assert.Equal("A", GetVersionString(chain.GetTagNodes("v1")));
        Assert.Equal("ABC", GetVersionString(chain.GetTagNodes("v2")));
    }

    [Fact]
    public void AppendRange_NoNodesToAdd()
    {
        Chain<char> chain = new();
        chain.Append("v1", "v1", new ChainNode<char>('A'));

        chain.AppendRange("v1", "v2", []);

        Assert.Equal("A", GetVersionString(chain.GetTagNodes("v1")));
        Assert.Equal("", GetVersionString(chain.GetTagNodes("v2")));
    }
    #endregion

    #region Replace
    [Fact]
    public void Replace_NodeNotConnected_False()
    {
        Chain<char> chain = new();
        List<ChainNode<char>> nodes = GetNodesFrom("ABC");
        chain.AppendRange("", "v0", nodes);

        bool result = chain.Replace("", "v1",
            new ChainNode<char>('X'),
            new ChainNode<char>('Y'));

        Assert.False(result);
    }

    [Fact]
    public void Replace_FirstNode()
    {
        Chain<char> chain = new();
        List<ChainNode<char>> nodes = GetNodesFrom("ABC");
        chain.AppendRange("", "v0", nodes);

        Assert.True(chain.Replace("v0", "v1", nodes[0],
            new ChainNode<char>('X')));

        Assert.Equal("ABC", GetVersionString(chain.GetTagNodes("v0")));
        Assert.Equal("XBC", GetVersionString(chain.GetTagNodes("v1")));
    }

    [Fact]
    public void Replace_LastNode()
    {
        Chain<char> chain = new();
        List<ChainNode<char>> nodes = GetNodesFrom("ABC");
        chain.AppendRange("", "v0", nodes);

        Assert.True(chain.Replace("v0", "v1", nodes[2],
            new ChainNode<char>('X')));

        Assert.Equal("ABC", GetVersionString(chain.GetTagNodes("v0")));
        Assert.Equal("ABX", GetVersionString(chain.GetTagNodes("v1")));
    }

    [Fact]
    public void Replace_InnerNode()
    {
        Chain<char> chain = new();
        List<ChainNode<char>> nodes = GetNodesFrom("ABC");
        chain.AppendRange("", "v0", nodes);

        Assert.True(chain.Replace("v0", "v1", nodes[1],
            new ChainNode<char>('X')));

        Assert.Equal("ABC", GetVersionString(chain.GetTagNodes("v0")));
        Assert.Equal("AXC", GetVersionString(chain.GetTagNodes("v1")));
    }
    #endregion

    #region ReplaceRange
    [Fact]
    public void ReplaceRange_NodesNotConnected_False()
    {
        Chain<char> chain = new();
        List<ChainNode<char>> nodes = GetNodesFrom("ABC");
        chain.AppendRange("", "v0", nodes);

        Assert.False(chain.ReplaceRange("", "v1",
            new ChainNode<char>('X'),
            new ChainNode<char>('Y'),
            GetNodesFrom("DE")));
    }

    [Fact]
    public void ReplaceRange_FirstNode()
    {
        Chain<char> chain = new();
        List<ChainNode<char>> nodes = GetNodesFrom("ABC");
        chain.AppendRange("", "v0", nodes);

        Assert.True(chain.ReplaceRange("v0", "v1", nodes[0], nodes[1],
            GetNodesFrom("DE")));

        Assert.Equal("DEC", GetVersionString(chain.GetTagNodes("v1")));
    }

    [Fact]
    public void ReplaceRange_LastNode()
    {
        Chain<char> chain = new();
        List<ChainNode<char>> nodes = GetNodesFrom("ABC");
        chain.AppendRange("", "v0", nodes);

        Assert.True(chain.ReplaceRange("v0", "v1", nodes[1], nodes[2],
            GetNodesFrom("DE")));

        Assert.Equal("ADE", GetVersionString(chain.GetTagNodes("v1")));
    }

    [Fact]
    public void ReplaceRange_InnerNodes()
    {
        Chain<char> chain = new();
        List<ChainNode<char>> nodes = GetNodesFrom("ABCD");
        chain.AppendRange("", "v0", nodes);

        Assert.True(chain.ReplaceRange("v0", "v1", nodes[1], nodes[2],
            GetNodesFrom("XY")));

        Assert.Equal("AXYD", GetVersionString(chain.GetTagNodes("v1")));
    }
    #endregion
}
