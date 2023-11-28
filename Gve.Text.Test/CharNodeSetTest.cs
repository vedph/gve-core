using System.Collections.Generic;
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

        Assert.Equal(16, set.GetCount());

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

    [Fact]
    public void GetRightNodeOf_DeletedVsNull_Ok()
    {
        CharNodeSet set = new(TEXT);
        // delete al[p]ha
        set.Delete(3);

        CharNode? node = set.GetNode(2);
        Assert.NotNull(node);
        node = set.GetRightNodeOf(node);

        Assert.NotNull(node);
        Assert.Equal('p', node!.Value);
        Assert.Equal(3, node!.Ordinal);
        Assert.True(node.Id > 0);
        Assert.Empty(node.PreviousValues);
        Assert.NotNull(node.LeftNode);
        Assert.True(node.IsDeleted);
    }

    [Fact]
    public void GetRightNodeOf_DeletedVsTrue_Ok()
    {
        CharNodeSet set = new(TEXT);
        // delete al[p]ha
        set.Delete(3);

        CharNode? node = set.GetNode(2);
        Assert.NotNull(node);
        node = set.GetRightNodeOf(node, true);

        Assert.NotNull(node);
        Assert.Equal('p', node!.Value);
        Assert.Equal(3, node!.Ordinal);
        Assert.True(node.Id > 0);
        Assert.Empty(node.PreviousValues);
        Assert.NotNull(node.LeftNode);
        Assert.True(node.IsDeleted);
    }

    [Fact]
    public void GetRightNodeOf_DeletedVsFalse_Ok()
    {
        CharNodeSet set = new(TEXT);
        // delete al[p]ha
        set.Delete(3);

        CharNode? node = set.GetNode(2);
        Assert.NotNull(node);
        node = set.GetRightNodeOf(node, false);

        Assert.NotNull(node);
        Assert.Equal('h', node!.Value);
        Assert.Equal(4, node!.Ordinal);
        Assert.True(node.Id > 0);
        Assert.Empty(node.PreviousValues);
        Assert.NotNull(node.LeftNode);
        Assert.False(node.IsDeleted);
    }

    [Fact]
    public void Delete_First_Ok()
    {
        CharNodeSet set = new(TEXT);
        set.Delete(1);

        CharNode? node = set.GetFirstNode();
        Assert.NotNull(node);
        Assert.Equal('a', node!.Value);
        Assert.Equal(1, node!.Ordinal);
        Assert.True(node.Id > 0);
        Assert.Empty(node.PreviousValues);
        Assert.Null(node.LeftNode);
        Assert.True(node.IsDeleted);

        // right node is still linked to the deleted one
        CharNode? right = set.GetNode(2);
        Assert.NotNull(right);
        Assert.Same(node, right.LeftNode);
    }

    [Fact]
    public void Delete_Last_Ok()
    {
        CharNodeSet set = new(TEXT);
        set.Delete(16);

        CharNode? node = set.GetLastNode();
        Assert.NotNull(node);
        Assert.Equal('a', node!.Value);
        Assert.Equal(16, node!.Ordinal);
        Assert.True(node.Id > 0);
        Assert.Empty(node.PreviousValues);
        Assert.NotNull(node.LeftNode);
        Assert.True(node.IsDeleted);

        // deleted node is still linked to its left node
        CharNode? left = set.GetNode(15);
        Assert.NotNull(left);
        Assert.Same(left, node.LeftNode);
    }

    [Fact]
    public void Delete_Mid_Ok()
    {
        CharNodeSet set = new(TEXT);
        set.Delete(4);

        CharNode? node = set.GetNode(4);
        Assert.NotNull(node);
        Assert.Equal('h', node!.Value);
        Assert.Equal(4, node!.Ordinal);
        Assert.True(node.Id > 0);
        Assert.Empty(node.PreviousValues);
        Assert.NotNull(node.LeftNode);
        Assert.True(node.IsDeleted);

        // deleted node is still linked to its left node
        CharNode? left = set.GetNode(3);
        Assert.NotNull(left);
        Assert.Same(left, node.LeftNode);

        // right node is still linked to the deleted one
        CharNode? right = set.GetNode(5);
        Assert.NotNull(right);
        Assert.Same(node, right.LeftNode);
    }

    [Fact]
    public void InsertBefore_First_Ok()
    {
        CharNodeSet set = new(TEXT);
        IList<CharNode> nodes = set.InsertBefore(1, "XY");

        // X
        CharNode? node = nodes[0];
        Assert.NotNull(node);
        Assert.Equal('X', node!.Value);
        Assert.Equal(0, node!.Ordinal);
        Assert.True(node.Id > 0);
        Assert.Empty(node.PreviousValues);
        Assert.Null(node.LeftNode);
        Assert.False(node.IsDeleted);
        // Y
        node = nodes[1];
        Assert.NotNull(node);
        Assert.Equal('Y', node!.Value);
        Assert.Equal(0, node!.Ordinal);
        Assert.True(node.Id > 0);
        Assert.Empty(node.PreviousValues);
        Assert.NotNull(node.LeftNode);
        Assert.False(node.IsDeleted);
        // node 1 has left node equal to Y
        CharNode? right = set.GetNode(1);
        Assert.NotNull(right);
        Assert.Same(node, right!.LeftNode);
    }

    [Fact]
    public void InsertBefore_Mid_Ok()
    {
        CharNodeSet set = new(TEXT);
        IList<CharNode> nodes = set.InsertBefore(6, "XY");

        // X
        CharNode? x = nodes[0];
        Assert.NotNull(x);
        Assert.Equal('X', x!.Value);
        Assert.Equal(0, x!.Ordinal);
        Assert.True(x.Id > 0);
        Assert.Empty(x.PreviousValues);
        Assert.NotNull(x.LeftNode);
        Assert.False(x.IsDeleted);
        // left of X is node 5
        Assert.Same(set.GetNode(5), x.LeftNode);
        // Y
        CharNode? y = nodes[1];
        Assert.NotNull(y);
        Assert.Equal('Y', y!.Value);
        Assert.Equal(0, y!.Ordinal);
        Assert.True(y.Id > 0);
        Assert.Empty(y.PreviousValues);
        Assert.NotNull(y.LeftNode);
        Assert.False(y.IsDeleted);
        // left of Y is node X
        Assert.Same(x, y.LeftNode);
        // node 6 has left node equal to Y
        CharNode? right = set.GetNode(6);
        Assert.NotNull(right);
        Assert.Same(y, right!.LeftNode);
    }

    [Fact]
    public void InsertAfter_Last_Ok()
    {
        CharNodeSet set = new(TEXT);
        IList<CharNode> nodes = set.InsertAfter(16, "XY");

        // X
        CharNode? x = nodes[0];
        Assert.NotNull(x);
        Assert.Equal('X', x!.Value);
        Assert.Equal(0, x!.Ordinal);
        Assert.True(x.Id > 0);
        Assert.Empty(x.PreviousValues);
        Assert.NotNull(x.LeftNode);
        Assert.Same(set.GetNode(16), x.LeftNode);
        Assert.False(x.IsDeleted);
        // Y
        CharNode? y = nodes[1];
        Assert.NotNull(y);
        Assert.Equal('Y', y!.Value);
        Assert.Equal(0, y!.Ordinal);
        Assert.True(y.Id > 0);
        Assert.Empty(y.PreviousValues);
        Assert.NotNull(y.LeftNode);
        Assert.Same(x, y.LeftNode);
        Assert.False(y.IsDeleted);
    }

    [Fact]
    public void InsertAfter_Mid_Ok()
    {
        CharNodeSet set = new(TEXT);
        IList<CharNode> nodes = set.InsertAfter(5, "XY");

        // X
        CharNode? x = nodes[0];
        Assert.NotNull(x);
        Assert.Equal('X', x!.Value);
        Assert.Equal(0, x!.Ordinal);
        Assert.True(x.Id > 0);
        Assert.Empty(x.PreviousValues);
        Assert.NotNull(x.LeftNode);
        Assert.Same(set.GetNode(5), x.LeftNode);
        Assert.False(x.IsDeleted);
        // Y
        CharNode? y = nodes[1];
        Assert.NotNull(y);
        Assert.Equal('Y', y!.Value);
        Assert.Equal(0, y!.Ordinal);
        Assert.True(y.Id > 0);
        Assert.Empty(y.PreviousValues);
        Assert.NotNull(y.LeftNode);
        Assert.Same(x, y.LeftNode);
        Assert.False(y.IsDeleted);
    }
}
