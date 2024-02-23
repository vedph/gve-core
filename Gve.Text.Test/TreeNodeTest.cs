using System;
using Xunit;

namespace Gve.Text.Test;

public sealed class TreeNodeTest
{
    [Fact]
    public void IsFirstSibling_WhenFirst_ShouldReturnTrue()
    {
        TreeNode root = new("A");
        root.AddChildFromPayload("B");
        root.AddChildFromPayload("C");

        Assert.True(root.Children[0].IsFirstSibling());
        Assert.False(root.Children[1].IsFirstSibling());
    }

    [Fact]
    public void IsLastSibling_WhenLast_ShouldReturnTrue()
    {
        TreeNode root = new("A");
        root.AddChildFromPayload("B");
        root.AddChildFromPayload("C");

        Assert.False(root.Children[0].IsLastSibling());
        Assert.True(root.Children[1].IsLastSibling());
    }

    #region Expansion
    [Fact]
    public void ToggleExpanded_WhenCollapsed_ShouldExpand()
    {
        TreeNode node = new("A");
        node.AddChildFromPayload("B");
        node.AddChildFromPayload("C");

        node.ToggleExpanded();

        Assert.True(node.IsExpanded);
        Assert.False(node.Children[0].IsExpanded);
        Assert.False(node.Children[1].IsExpanded);
    }

    [Fact]
    public void ToggleExpandedRecursive_WhenCollapsed_ShouldExpand()
    {
        TreeNode node = new("A");
        node.AddChildFromPayload("B");
        node.AddChildFromPayload("C");

        node.ToggleExpanded(true);

        Assert.True(node.IsExpanded);
        Assert.True(node.Children[0].IsExpanded);
        Assert.True(node.Children[1].IsExpanded);
    }

    [Fact]
    public void ToggleExpanded_WhenExpanded_ShouldCollapse()
    {
        TreeNode node = new("A");
        node.AddChildFromPayload("B");
        node.AddChildFromPayload("C");
        node.SetExpanded(true);

        node.ToggleExpanded();

        Assert.False(node.IsExpanded);
    }

    [Fact]
    public void ToggleExpandedRecursive_WhenExpanded_ShouldCollapse()
    {
        TreeNode node = new("A");
        node.AddChildFromPayload("B");
        node.AddChildFromPayload("C");
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
        root.AddChildFromPayload("Child");

        Assert.Equal(1, root.GetY());
        Assert.Equal(2, root.Children[0].GetY());
    }

    [Fact]
    public void GetMaxY__ShouldReturnMaxDepth()
    {
        TreeNode root = new("A");
        TreeNode b = root.AddChildFromPayload("B");
        b.AddChildFromPayload("C");

        Assert.Equal(3, root.GetMaxY());
    }

    [Fact]
    public void GetYX_ShouldReturnCorrectCoordinates()
    {
        TreeNode root = new("A");
        root.AddChildFromPayload("B");
        root.AddChildFromPayload("C");

        Assert.Equal(Tuple.Create(1, 1), root.GetYX());
        Assert.Equal(Tuple.Create(2, 1), root.Children[0].GetYX());
        Assert.Equal(Tuple.Create(2, 2), root.Children[1].GetYX());
    }

    [Fact]
    public void HasChildren_WhenHasChildren_ShouldReturnTrue()
    {
        TreeNode root = new("A");
        root.AddChildFromPayload("B");

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
        TreeNode child = root.AddChildFromPayload("B");

        string path = child.GetPath();

        Assert.Equal("/1", path);
    }

    [Fact]
    public void GetPath_OnFirstGrandchild_Ok()
    {
        TreeNode root = new("A");
        TreeNode child = root.AddChildFromPayload("B");
        TreeNode grandchild = child.AddChildFromPayload("C");

        string path = grandchild.GetPath();

        Assert.Equal("/1/1", path);
    }

    [Fact]
    public void GetPath_OnSecondChild_Ok()
    {
        TreeNode root = new("A");
        root.AddChildFromPayload("B");
        TreeNode c = root.AddChildFromPayload("C");

        string path = c.GetPath();

        Assert.Equal("/2", path);
    }

    [Fact]
    public void GetPath_OnThirdChild_Ok()
    {
        TreeNode root = new("A");
        root.AddChildFromPayload("B");
        root.AddChildFromPayload("C");
        TreeNode d = root.AddChildFromPayload("D");

        string path = d.GetPath();

        Assert.Equal("/3", path);
    }

    [Fact]
    public void GetPath_OnSecondGrandchildOfSecondChild_Ok()
    {
        TreeNode root = new("A");
        // children
        root.AddChildFromPayload("B");
        TreeNode c = root.AddChildFromPayload("C");
        // grandchildren
        c.AddChildFromPayload("D");
        TreeNode e = c.AddChildFromPayload("E");

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
        root.AddChildFromPayload("B");
        TreeNode c = root.AddChildFromPayload("C");

        TreeNode? node = root.WalkPathTo("/2");

        Assert.Equal(c, node);
    }
    #endregion

    #region Clone
    [Fact]
    public void Clone_ShouldReturnClone()
    {
        TreeNode root = new("Root");
        root.AddChildFromPayload("Child");

        TreeNode clone = root.Clone();

        Assert.Equal(root.Id, clone.Id);
        Assert.Equal(root.Label, clone.Label);
        Assert.Equal(root.Data, clone.Data);

        Assert.Equal(root.Children[0].Id, clone.Children[0].Id);
        Assert.Equal(root.Children[0].Label, clone.Children[0].Label);
        Assert.Equal(root.Children[0].Data, clone.Children[0].Data);
    }
    #endregion
}
