using Xunit;

namespace Gve.Text.Test;

public sealed class CharNodeSetTest
{
    //                           123456789-123456
    private const string TEXT = "alpha beta gamma";

    [Fact]
    public void SetText_Ok()
    {
        CharNodeSet set = new(TEXT);

        int i = 0;
        foreach (CharNode node in set)
        {
            Assert.Equal(TEXT[i], node.Value);
            if (i == 0)
            {
                Assert.Null(node.LeftNode);
            }
            else
            {
                Assert.NotNull(node.LeftNode);
                Assert.Equal(TEXT[i - 1], node.LeftNode!.Value);
            }
            i++;
        }
    }

    [Fact]
    public void GetFirstNode_Ok()
    {
        CharNodeSet set = new(TEXT);

        CharNode? node = set.GetFirstNode();

        Assert.NotNull(node);
        Assert.Equal('a', node!.Value);
        Assert.Equal(1, node!.Ordinal);
        Assert.True(node.Id > 0);
        Assert.Empty(node.PreviousValues);
        Assert.Null(node.LeftNode);
        Assert.False(node.IsDeleted);
    }

    [Fact]
    public void GetLastNode_Ok()
    {
        CharNodeSet set = new(TEXT);

        CharNode? node = set.GetLastNode();

        Assert.NotNull(node);
        Assert.Equal('a', node!.Value);
        Assert.Equal(16, node!.Ordinal);
        Assert.True(node.Id > 0);
        Assert.Empty(node.PreviousValues);
        Assert.NotNull(node.LeftNode);
        Assert.False(node.IsDeleted);
    }

    [Fact]
    public void GetNode_Ok()
    {
        CharNodeSet set = new(TEXT);

        CharNode? node = set.GetNode(3);

        Assert.NotNull(node);
        Assert.Equal('p', node!.Value);
        Assert.Equal(3, node!.Ordinal);
        Assert.True(node.Id > 0);
        Assert.Empty(node.PreviousValues);
        Assert.NotNull(node.LeftNode);
        Assert.False(node.IsDeleted);
    }

    [Fact]
    public void GetLeftNodeOf_Ok()
    {
        CharNodeSet set = new(TEXT);

        CharNode? node = set.GetNode(4);
        Assert.NotNull(node);
        node = set.GetLeftNodeOf(node);

        Assert.NotNull(node);
        Assert.Equal('p', node!.Value);
        Assert.Equal(3, node!.Ordinal);
        Assert.True(node.Id > 0);
        Assert.Empty(node.PreviousValues);
        Assert.NotNull(node.LeftNode);
        Assert.False(node.IsDeleted);
    }

    [Fact]
    public void GetLeftNodeOf_DeletedVsNull_Ok()
    {
        CharNodeSet set = new(TEXT);

        // delete al[p]ha
        set.Delete(3);

        CharNode? node = set.GetNode(4);
        Assert.NotNull(node);
        node = set.GetLeftNodeOf(node);

        Assert.NotNull(node);
        Assert.Equal('p', node!.Value);
        Assert.Equal(3, node!.Ordinal);
        Assert.True(node.Id > 0);
        Assert.Empty(node.PreviousValues);
        Assert.NotNull(node.LeftNode);
        Assert.True(node.IsDeleted);
    }

    [Fact]
    public void GetLeftNodeOf_DeletedVsTrue_Ok()
    {
        CharNodeSet set = new(TEXT);

        // delete al[p]ha
        set.Delete(3);

        CharNode? node = set.GetNode(4);
        Assert.NotNull(node);
        node = set.GetLeftNodeOf(node, true);

        Assert.NotNull(node);
        Assert.Equal('p', node!.Value);
        Assert.Equal(3, node!.Ordinal);
        Assert.True(node.Id > 0);
        Assert.Empty(node.PreviousValues);
        Assert.NotNull(node.LeftNode);
        Assert.True(node.IsDeleted);
    }

    [Fact]
    public void GetLeftNodeOf_DeletedVsFalse_Ok()
    {
        CharNodeSet set = new(TEXT);

        // delete al[p]ha
        set.Delete(3);

        CharNode? node = set.GetNode(4);
        Assert.NotNull(node);
        node = set.GetLeftNodeOf(node, false);

        Assert.NotNull(node);
        Assert.Equal('l', node!.Value);
        Assert.Equal(2, node!.Ordinal);
        Assert.True(node.Id > 0);
        Assert.Empty(node.PreviousValues);
        Assert.NotNull(node.LeftNode);
        Assert.False(node.IsDeleted);
    }
}
