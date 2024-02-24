using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Gve.Text.Test;

public sealed class TreeNodeTest
{
    [Fact]
    public void IsFirstSibling_WhenFirst_ShouldReturnTrue()
    {
        TreeNode root = new("A");
        root.AddChildFromData("B");
        root.AddChildFromData("C");

        Assert.True(root.Children[0].IsFirstSibling());
        Assert.False(root.Children[1].IsFirstSibling());
    }

    [Fact]
    public void IsLastSibling_WhenLast_ShouldReturnTrue()
    {
        TreeNode root = new("A");
        root.AddChildFromData("B");
        root.AddChildFromData("C");

        Assert.False(root.Children[0].IsLastSibling());
        Assert.True(root.Children[1].IsLastSibling());
    }

    #region Expansion
    [Fact]
    public void ToggleExpanded_WhenCollapsed_ShouldExpand()
    {
        TreeNode node = new("A");
        node.AddChildFromData("B");
        node.AddChildFromData("C");

        node.ToggleExpanded();

        Assert.True(node.IsExpanded);
        Assert.False(node.Children[0].IsExpanded);
        Assert.False(node.Children[1].IsExpanded);
    }

    [Fact]
    public void ToggleExpandedRecursive_WhenCollapsed_ShouldExpand()
    {
        TreeNode node = new("A");
        node.AddChildFromData("B");
        node.AddChildFromData("C");

        node.ToggleExpanded(true);

        Assert.True(node.IsExpanded);
        Assert.True(node.Children[0].IsExpanded);
        Assert.True(node.Children[1].IsExpanded);
    }

    [Fact]
    public void ToggleExpanded_WhenExpanded_ShouldCollapse()
    {
        TreeNode node = new("A");
        node.AddChildFromData("B");
        node.AddChildFromData("C");
        node.SetExpanded(true);

        node.ToggleExpanded();

        Assert.False(node.IsExpanded);
    }

    [Fact]
    public void ToggleExpandedRecursive_WhenExpanded_ShouldCollapse()
    {
        TreeNode node = new("A");
        node.AddChildFromData("B");
        node.AddChildFromData("C");
        node.SetExpanded(true);

        node.ToggleExpanded(true);

        Assert.False(node.IsExpanded);
        Assert.False(node.Children[0].IsExpanded);
        Assert.False(node.Children[1].IsExpanded);
    }
    #endregion

    #region Path
    [Fact]
    public void GetY__ShouldReturnCorrectDepth()
    {
        TreeNode root = new("Root");
        root.AddChildFromData("Child");

        Assert.Equal(1, root.GetY());
        Assert.Equal(2, root.Children[0].GetY());
    }

    [Fact]
    public void GetMaxY__ShouldReturnMaxDepth()
    {
        TreeNode root = new("A");
        TreeNode b = root.AddChildFromData("B");
        b.AddChildFromData("C");

        Assert.Equal(3, root.GetMaxY());
    }

    [Fact]
    public void GetYX_ShouldReturnCorrectCoordinates()
    {
        TreeNode root = new("A");
        root.AddChildFromData("B");
        root.AddChildFromData("C");

        Assert.Equal(Tuple.Create(1, 1), root.GetYX());
        Assert.Equal(Tuple.Create(2, 1), root.Children[0].GetYX());
        Assert.Equal(Tuple.Create(2, 2), root.Children[1].GetYX());
    }

    [Fact]
    public void HasChildren_WhenHasChildren_ShouldReturnTrue()
    {
        TreeNode root = new("A");
        root.AddChildFromData("B");

        Assert.True(root.HasChildren);
    }

    [Fact]
    public void GetPath_OnRoot_ShouldReturnRootOnly()
    {
        TreeNode root = new("A");

        string path = root.GetPath();

        Assert.Equal("", path);
    }

    [Fact]
    public void GetPath_OnFirstChild_Ok()
    {
        TreeNode root = new("A");
        TreeNode child = root.AddChildFromData("B");

        string path = child.GetPath();

        Assert.Equal("/1", path);
    }

    [Fact]
    public void GetPath_OnFirstGrandchild_Ok()
    {
        TreeNode root = new("A");
        TreeNode child = root.AddChildFromData("B");
        TreeNode grandchild = child.AddChildFromData("C");

        string path = grandchild.GetPath();

        Assert.Equal("/1/1", path);
    }

    [Fact]
    public void GetPath_OnSecondChild_Ok()
    {
        TreeNode root = new("A");
        root.AddChildFromData("B");
        TreeNode c = root.AddChildFromData("C");

        string path = c.GetPath();

        Assert.Equal("/2", path);
    }

    [Fact]
    public void GetPath_OnThirdChild_Ok()
    {
        TreeNode root = new("A");
        root.AddChildFromData("B");
        root.AddChildFromData("C");
        TreeNode d = root.AddChildFromData("D");

        string path = d.GetPath();

        Assert.Equal("/3", path);
    }

    [Fact]
    public void GetPath_OnSecondGrandchildOfSecondChild_Ok()
    {
        TreeNode root = new("A");
        // children
        root.AddChildFromData("B");
        TreeNode c = root.AddChildFromData("C");
        // grandchildren
        c.AddChildFromData("D");
        TreeNode e = c.AddChildFromData("E");

        string path = e.GetPath();

        Assert.Equal("/2/2", path);
    }

    [Fact]
    public void WalkPathTo_Empty_ShouldReturnCurrent()
    {
        TreeNode root = new("A");

        TreeNode? node = root.WalkPathTo("");

        Assert.Equal(root, node);
    }

    [Fact]
    public void WalkPathTo_SecondChild_ShouldReturnIt()
    {
        TreeNode root = new("A");
        root.AddChildFromData("B");
        TreeNode c = root.AddChildFromData("C");

        TreeNode? node = root.WalkPathTo("/2");

        Assert.Equal(c, node);
    }

    [Fact]
    public void WalkPathTo_MalformedSecondChild_ShouldReturnIt()
    {
        TreeNode root = new("A");
        root.AddChildFromData("B");
        TreeNode c = root.AddChildFromData("C");

        TreeNode? node = root.WalkPathTo("2");

        Assert.Equal(c, node);
    }
    #endregion

    #region Clone
    [Fact]
    public void Clone_ShouldReturnClone()
    {
        TreeNode root = new("Root");
        root.AddChildFromData("Child");

        TreeNode clone = root.Clone();

        Assert.Equal(root.Id, clone.Id);
        Assert.Equal(root.Label, clone.Label);
        Assert.Equal(root.Data, clone.Data);

        Assert.Equal(root.Children[0].Id, clone.Children[0].Id);
        Assert.Equal(root.Children[0].Label, clone.Children[0].Label);
        Assert.Equal(root.Children[0].Data, clone.Children[0].Data);
    }
    #endregion

    #region Add or Remove
    [Fact]
    public void AddChild_ShouldAddChild()
    {
        TreeNode root = new("A");

        TreeNode child = new("B");
        root.AddChild(child);

        Assert.Single(root.Children);
        Assert.Equal("B", root.Children[0].Data);
        Assert.Equal(child, root.Children[0]);
        Assert.Equal(root, child.Parent);
    }

    [Fact]
    public void AddChildFromData_ShouldAddChild()
    {
        TreeNode root = new("A");

        TreeNode child = root.AddChildFromData("B");

        Assert.Single(root.Children);
        Assert.Equal("B", root.Children[0].Data);
        Assert.Equal(child, root.Children[0]);
        Assert.Equal(root, child.Parent);
    }

    [Fact]
    public void AddChildren_ShouldAddChildren()
    {
        TreeNode root = new("A");
        TreeNode b = new("B");
        TreeNode c = new("C");

        root.AddChildren([b, c]);

        Assert.Equal(2, root.Children.Count);
        Assert.Equal("B", root.Children[0].Data);
        Assert.Equal("C", root.Children[1].Data);
        Assert.Equal(b, root.Children[0]);
        Assert.Equal(c, root.Children[1]);
        Assert.Equal(root, b.Parent);
        Assert.Equal(root, c.Parent);
    }

    [Fact]
    public void AddChildrenFromData_ShouldAddChildren()
    {
        TreeNode root = new("A");

        root.AddChildrenFromData(["B", "C"]);

        Assert.Equal(2, root.Children.Count);
        Assert.Equal("B", root.Children[0].Data);
        Assert.Equal("C", root.Children[1].Data);
        Assert.Equal(root, root.Children[0].Parent);
        Assert.Equal(root, root.Children[1].Parent);
    }

    [Fact]
    public void AddBeforeSelf_ShouldAddBeforeSelf()
    {
        TreeNode root = new("A");
        TreeNode child = root.AddChildFromData("B");

        TreeNode before = new("C");
        child.AddBeforeSelf(before);

        Assert.Equal(2, root.Children.Count);
        Assert.Equal("C", root.Children[0].Data);
        Assert.Equal("B", root.Children[1].Data);
        Assert.Equal(before, root.Children[0]);
        Assert.Equal(child, root.Children[1]);
        Assert.Equal(root, before.Parent);
    }

    [Fact]
    public void AddBeforeSelfFromData_ShouldAddBeforeSelf()
    {
        TreeNode root = new("A");
        TreeNode child = root.AddChildFromData("B");

        TreeNode before = child.AddBeforeSelfFromData("C");

        Assert.Equal(2, root.Children.Count);
        Assert.Equal("C", root.Children[0].Data);
        Assert.Equal("B", root.Children[1].Data);
        Assert.Equal(before, root.Children[0]);
        Assert.Equal(child, root.Children[1]);
        Assert.Equal(root, before.Parent);
    }

    [Fact]
    public void AddAfterSelf_ShouldAddAfterSelf()
    {
        TreeNode root = new("A");
        TreeNode child = root.AddChildFromData("B");

        TreeNode after = new("C");
        child.AddAfterSelf(after);

        Assert.Equal(2, root.Children.Count);
        Assert.Equal("B", root.Children[0].Data);
        Assert.Equal("C", root.Children[1].Data);
        Assert.Equal(child, root.Children[0]);
        Assert.Equal(after, root.Children[1]);
        Assert.Equal(root, after.Parent);
    }

    [Fact]
    public void AddAfterSelfFromData_ShouldAddAfterSelf()
    {
        TreeNode root = new("A");
        TreeNode child = root.AddChildFromData("B");

        TreeNode after = child.AddAfterSelfFromData("C");

        Assert.Equal(2, root.Children.Count);
        Assert.Equal("B", root.Children[0].Data);
        Assert.Equal("C", root.Children[1].Data);
        Assert.Equal(child, root.Children[0]);
        Assert.Equal(after, root.Children[1]);
        Assert.Equal(root, after.Parent);
    }

    [Fact]
    public void Remove_ShouldRemoveFromParent()
    {
        TreeNode root = new("A");
        TreeNode child = root.AddChildFromData("B");

        child.Remove();

        Assert.Empty(root.Children);
        Assert.Null(child.Parent);
    }

    [Fact]
    public void RemoveChild_ShouldRemoveChild()
    {
        TreeNode root = new("A");
        TreeNode child = root.AddChildFromData("B");

        root.RemoveChild(child);

        Assert.Empty(root.Children);
        Assert.Null(child.Parent);
    }
    #endregion

    #region Traversing
    [Fact]
    public void GetRoot_ShouldReturnRoot()
    {
        TreeNode root = new("A");
        TreeNode child = root.AddChildFromData("B");

        TreeNode? node = child.GetRoot();

        Assert.Equal(root, node);
    }

    [Fact]
    public void Traverse_ShouldTraverse()
    {
        // A
        // +- B
        // |  +- D
        // |  +- E
        // +- C
        TreeNode root = new("A");
        TreeNode b = root.AddChildFromData("B");
        TreeNode c = root.AddChildFromData("C");
        TreeNode d = b.AddChildFromData("D");
        TreeNode e = b.AddChildFromData("E");

        List<TreeNode> nodes = [];
        root.Traverse(n =>
        {
            nodes.Add(n);
            return true;
        });

        Assert.Equal(5, nodes.Count);
        Assert.Equal(root, nodes[0]);
        Assert.Equal(b, nodes[1]);
        Assert.Equal(d, nodes[2]);
        Assert.Equal(e, nodes[3]);
        Assert.Equal(c, nodes[4]);
    }

    [Fact]
    public void Traverse_WhenFalse_ShouldStop()
    {
        // A
        // +- B
        // |  +- D
        // |  +- E
        // +- C
        TreeNode root = new("A");
        TreeNode b = root.AddChildFromData("B");
        root.AddChildFromData("C");
        TreeNode d = b.AddChildFromData("D");
        TreeNode e = b.AddChildFromData("E");

        List<TreeNode> nodes = [];
        root.Traverse(n =>
        {
            nodes.Add(n);
            return n != e;
        });

        Assert.Equal(4, nodes.Count);
        Assert.Equal(root, nodes[0]);
        Assert.Equal(b, nodes[1]);
        Assert.Equal(d, nodes[2]);
        Assert.Equal(e, nodes[3]);
    }

    [Fact]
    public void GetAscendants_ShouldEnumerateAscendants()
    {
        // A
        // +- B
        // |  +- D
        // |  +- E
        // +- C
        TreeNode root = new("A");
        TreeNode b = root.AddChildFromData("B");
        root.AddChildFromData("C");
        b.AddChildFromData("D");
        TreeNode e = b.AddChildFromData("E");

        List<TreeNode> nodes = e.GetAscendants().ToList();

        Assert.Equal(2, nodes.Count);
        Assert.Equal(b, nodes[0]);
        Assert.Equal(root, nodes[1]);
    }

    [Fact]
    public void GetFirstAscendant_ShouldReturnFirstMatchingAscendant()
    {
        // A
        // +- B
        // |  +- D
        // |  +- E
        // +- C
        TreeNode root = new("A");
        TreeNode b = root.AddChildFromData("B");
        root.AddChildFromData("C");
        b.AddChildFromData("D");
        TreeNode e = b.AddChildFromData("E");

        TreeNode? node = e.GetFirstAscendant(n => n == b);

        Assert.Equal(b, node);
    }

    [Fact]
    public void GetFirstAscendant_NoMatch_ShouldReturnNull()
    {
        // A
        // +- B
        // |  +- D
        // |  +- E
        // +- C
        TreeNode root = new("A");
        TreeNode b = root.AddChildFromData("B");
        root.AddChildFromData("C");
        b.AddChildFromData("D");
        TreeNode e = b.AddChildFromData("E");

        TreeNode? node = e.GetFirstAscendant(n => n.Label == "not-existing");

        Assert.Null(node);
    }

    [Fact]
    public void GetFirstAscendantOrSelf_ShouldReturnFirstMatchingAscendantOrSelf()
    {
        // A
        // +- B
        // |  +- D
        // |  +- E
        // +- C
        TreeNode root = new("A");
        TreeNode b = root.AddChildFromData("B");
        root.AddChildFromData("C");
        b.AddChildFromData("D");
        TreeNode e = b.AddChildFromData("E");

        TreeNode? node = e.GetFirstAscendantOrSelf(n => n == e);

        Assert.Equal(e, node);
    }

    [Fact]
    public void GetDescendants_ShouldEnumerateDescendants()
    {
        // A
        // +- B
        // |  +- D
        // |  +- E
        // +- C
        TreeNode root = new("A");
        TreeNode b = root.AddChildFromData("B");
        TreeNode c = root.AddChildFromData("C");
        TreeNode d = b.AddChildFromData("D");
        TreeNode e = b.AddChildFromData("E");

        List<TreeNode> nodes = root.GetDescendants().ToList();

        Assert.Equal(4, nodes.Count);
        Assert.Equal(b, nodes[0]);
        Assert.Equal(d, nodes[1]);
        Assert.Equal(e, nodes[2]);
        Assert.Equal(c, nodes[3]);
    }

    [Fact]
    public void GetFirstDescendant_ShouldReturnFirstMatchingDescendant()
    {
        // A
        // +- B
        // |  +- D
        // |  +- E
        // +- C
        TreeNode root = new("A");
        TreeNode b = root.AddChildFromData("B");
        root.AddChildFromData("C");
        b.AddChildFromData("D");
        TreeNode e = b.AddChildFromData("E");

        TreeNode? node = root.GetFirstDescendant(n => n == e);

        Assert.Equal(e, node);
    }

    [Fact]
    public void GetFirstDescendantOrSelf_ShouldReturnFirstMatchingDescendantOrSelf()
    {
        // A
        // +- B
        // |  +- D
        // |  +- E
        // +- C
        TreeNode root = new("A");
        TreeNode b = root.AddChildFromData("B");
        root.AddChildFromData("C");
        b.AddChildFromData("D");
        b.AddChildFromData("E");

        TreeNode? node = root.GetFirstDescendantOrSelf(n => n == root);

        Assert.Equal(root, node);
    }

    [Fact]
    public void HasAscendant_OnMatch_ShouldReturnTrue()
    {
        // A
        // +- B
        // |  +- D
        // |  +- E
        // +- C
        TreeNode root = new("A");
        TreeNode b = root.AddChildFromData("B");
        root.AddChildFromData("C");
        b.AddChildFromData("D");
        TreeNode e = b.AddChildFromData("E");

        bool has = e.HasAscendant(n => n == b);

        Assert.True(has);
    }

    [Fact]
    public void HasDescendant_OnFound_ShouldReturnTrue()
    {
        // A
        // +- B
        // |  +- D
        // |  +- E
        // +- C
        TreeNode root = new("A");
        TreeNode b = root.AddChildFromData("B");
        root.AddChildFromData("C");
        b.AddChildFromData("D");
        TreeNode e = b.AddChildFromData("E");

        bool has = root.HasDescendant(n => n == e);

        Assert.True(has);
    }

    [Fact]
    public void GetLeftSiblings_ShouldReturnLeftSiblings()
    {
        // A
        // +- B
        // |  +- D
        // |  +- E
        // +- C
        TreeNode root = new("A");
        TreeNode b = root.AddChildFromData("B");
        root.AddChildFromData("C");
        TreeNode d = b.AddChildFromData("D");
        TreeNode e = b.AddChildFromData("E");

        List<TreeNode> nodes = e.GetLeftSiblings().ToList();

        Assert.Single(nodes);
        Assert.Equal(d, nodes[0]);
    }

    [Fact]
    public void GetRightSiblings_ShouldReturnRightSiblings()
    {
        // A
        // +- B
        // |  +- D
        // |  +- E
        // +- C
        TreeNode root = new("A");
        TreeNode b = root.AddChildFromData("B");
        root.AddChildFromData("C");
        TreeNode d = b.AddChildFromData("D");
        TreeNode e = b.AddChildFromData("E");

        List<TreeNode> nodes = d.GetRightSiblings().ToList();

        Assert.Single(nodes);
        Assert.Equal(e, nodes[0]);
    }

    [Fact]
    public void HasLeftSibling_OnMatch_ShouldReturnTrue()
    {
        TreeNode root = new("A");
        TreeNode b = root.AddChildFromData("B");
        TreeNode c = root.AddChildFromData("C");

        Assert.True(c.HasLeftSibling(n => n == b));
    }

    [Fact]
    public void HasRightSibling_OnMatch_ShouldReturnTrue()
    {
        TreeNode root = new("A");
        TreeNode b = root.AddChildFromData("B");
        TreeNode c = root.AddChildFromData("C");

        Assert.True(b.HasRightSibling(n => n == c));
    }
    #endregion
}
