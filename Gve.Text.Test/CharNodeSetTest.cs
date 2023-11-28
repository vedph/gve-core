using System;
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
        CharNode node = nodes[0];
        Assert.Equal('X', node!.Value);
        Assert.Equal(0, node!.Ordinal);
        Assert.True(node.Id > 0);
        Assert.Empty(node.PreviousValues);
        Assert.Null(node.LeftNode);
        Assert.False(node.IsDeleted);
        // Y
        node = nodes[1];
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
        CharNode x = nodes[0];
        Assert.Equal('X', x!.Value);
        Assert.Equal(0, x!.Ordinal);
        Assert.True(x.Id > 0);
        Assert.Empty(x.PreviousValues);
        Assert.NotNull(x.LeftNode);
        Assert.False(x.IsDeleted);
        // left of X is node 5
        Assert.Same(set.GetNode(5), x.LeftNode);
        // Y
        CharNode y = nodes[1];
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
        CharNode x = nodes[0];
        Assert.Equal('X', x!.Value);
        Assert.Equal(0, x!.Ordinal);
        Assert.True(x.Id > 0);
        Assert.Empty(x.PreviousValues);
        Assert.NotNull(x.LeftNode);
        Assert.Same(set.GetNode(16), x.LeftNode);
        Assert.False(x.IsDeleted);
        // Y
        CharNode y = nodes[1];
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
        CharNode x = nodes[0];
        Assert.Equal('X', x!.Value);
        Assert.Equal(0, x!.Ordinal);
        Assert.True(x.Id > 0);
        Assert.Empty(x.PreviousValues);
        Assert.NotNull(x.LeftNode);
        Assert.Same(set.GetNode(5), x.LeftNode);
        Assert.False(x.IsDeleted);
        // Y
        CharNode y = nodes[1];
        Assert.Equal('Y', y!.Value);
        Assert.Equal(0, y!.Ordinal);
        Assert.True(y.Id > 0);
        Assert.Empty(y.PreviousValues);
        Assert.NotNull(y.LeftNode);
        Assert.Same(x, y.LeftNode);
        Assert.False(y.IsDeleted);
    }

    [Fact]
    public void Replace_SameChars_Ok()
    {
        CharNodeSet set = new(TEXT);
        IList<CharNode> nodes = set.Replace(3, 2, "PH");

        Assert.Equal(16, set.GetCount());
        // P
        CharNode p = nodes[0];
        Assert.Equal('P', p!.Value);
        Assert.Equal(3, p!.Ordinal);
        Assert.True(p.Id > 0);
        Assert.Single(p.PreviousValues);
        Assert.Equal('p', p.PreviousValues[0]);
        Assert.NotNull(p.LeftNode);
        Assert.Same(set.GetNode(2), p.LeftNode);
        Assert.False(p.IsDeleted);
        // H
        CharNode h = nodes[1];
        Assert.Equal('H', h!.Value);
        Assert.Equal(4, h!.Ordinal);
        Assert.True(h.Id > 0);
        Assert.Single(h.PreviousValues);
        Assert.Equal('h', h.PreviousValues[0]);
        Assert.NotNull(h.LeftNode);
        Assert.Same(p, h.LeftNode);
        Assert.False(h.IsDeleted);
    }

    [Fact]
    public void Replace_LessChars_Ok()
    {
        CharNodeSet set = new(TEXT);
        IList<CharNode> nodes = set.Replace(3, 2, "F");

        Assert.Equal(2, nodes.Count);
        Assert.Equal(15, set.GetCount(false));
        // F
        CharNode f = nodes[0];
        Assert.Equal('F', f!.Value);
        Assert.Equal(3, f!.Ordinal);
        Assert.True(f.Id > 0);
        Assert.Single(f.PreviousValues);
        Assert.Equal('p', f.PreviousValues[0]);
        Assert.NotNull(f.LeftNode);
        Assert.Same(set.GetNode(2), f.LeftNode);
        Assert.False(f.IsDeleted);
        // right node is deleted h
        CharNode? right = set.GetRightNodeOf(nodes[0]);
        Assert.NotNull(right);
        Assert.Same(f, right.LeftNode);
        Assert.True(right.IsDeleted);
    }

    [Fact]
    public void Replace_MoreChars_Ok()
    {
        CharNodeSet set = new(TEXT);
        IList<CharNode> nodes = set.Replace(3, 2, "XYZ");

        Assert.Equal(3, nodes.Count);
        Assert.Equal(17, set.GetCount());
        // X
        CharNode x = nodes[0];
        Assert.Equal('X', x!.Value);
        Assert.Equal(3, x!.Ordinal);
        Assert.True(x.Id > 0);
        Assert.Single(x.PreviousValues);
        Assert.Equal('p', x.PreviousValues[0]);
        Assert.NotNull(x.LeftNode);
        Assert.Same(set.GetNode(2), x.LeftNode);
        Assert.False(x.IsDeleted);
        // Y
        CharNode y = nodes[1];
        Assert.Equal('Y', y!.Value);
        Assert.Equal(4, y!.Ordinal);
        Assert.True(y.Id > 0);
        Assert.Single(y.PreviousValues);
        Assert.Equal('h', y.PreviousValues[0]);
        Assert.NotNull(y.LeftNode);
        Assert.Same(x, y.LeftNode);
        Assert.False(y.IsDeleted);
        // Z
        CharNode z = nodes[2];
        Assert.Equal('Z', z!.Value);
        Assert.Equal(0, z!.Ordinal);
        Assert.True(z.Id > 0);
        Assert.Empty(z.PreviousValues);
        Assert.NotNull(z.LeftNode);
        Assert.Same(y, z.LeftNode);
        Assert.False(z.IsDeleted);
        // node 5 has Z as left node
        CharNode? right = set.GetNode(5);
        Assert.NotNull(right);
        Assert.Same(z, right.LeftNode);
    }

    [Fact]
    public void Replace_Start_Ok()
    {
        CharNodeSet set = new(TEXT);
        IList<CharNode> nodes = set.Replace(1, 1, "X");

        Assert.Equal(16, set.GetCount());
        Assert.Single(nodes);
        // X
        CharNode x = nodes[0];
        Assert.Equal('X', x!.Value);
        Assert.Equal(1, x!.Ordinal);
        Assert.True(x.Id > 0);
        Assert.Single(x.PreviousValues);
        Assert.Equal('a', x.PreviousValues[0]);
        Assert.Null(x.LeftNode);
        Assert.False(x.IsDeleted);
    }

    [Fact]
    public void Replace_End_Ok()
    {
        CharNodeSet set = new(TEXT);
        IList<CharNode> nodes = set.Replace(16, 1, "X");
        Assert.Equal(16, set.GetCount());
        Assert.Single(nodes);
        // X
        CharNode x = nodes[0];
        Assert.Equal('X', x!.Value);
        Assert.Equal(16, x!.Ordinal);
        Assert.True(x.Id > 0);
        Assert.Single(x.PreviousValues);
        Assert.Equal('a', x.PreviousValues[0]);
        Assert.NotNull(x.LeftNode);
        Assert.False(x.IsDeleted);
    }

    [Fact]
    public void Replace_WithZero_Deleted()
    {
        CharNodeSet set = new(TEXT);
        IList<CharNode> nodes = set.Replace(5, 1, "");

        Assert.Equal(16, set.GetCount());
        Assert.Equal(15, set.GetCount(false));
        // 5 was deleted
        CharNode? node = set.GetNode(5);
        Assert.NotNull(node);
        Assert.True(node!.IsDeleted);
        Assert.Same(nodes[0], node);
    }

    [Fact]
    public void Replace_RunZero_Inserted()
    {
        CharNodeSet set = new(TEXT);
        IList<CharNode> nodes = set.Replace(6, 0, "X");

        Assert.Equal(17, set.GetCount());
        Assert.Single(nodes);

        // X was inserted before 6
        CharNode? node = nodes[0];
        Assert.NotNull(node);
        Assert.Equal('X', node!.Value);
        Assert.Equal(0, node!.Ordinal);
        Assert.True(node.Id > 0);
        Assert.Empty(node.PreviousValues);
        Assert.NotNull(node.LeftNode);
        Assert.False(node.IsDeleted);
        Assert.Same(node, set.GetNode(6)!.LeftNode);
    }
}
