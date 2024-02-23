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
    public void GetY_WhenCalled_ShouldReturnCorrectDepth()
    {
        TreeNode root = new("Root");
        root.AddChildFromPayload("Child");

        Assert.Equal(1, root.GetY());
        Assert.Equal(2, root.Children[0].GetY());
    }

    [Fact]
    public void GetMaxY_WhenCalled_ShouldReturnMaxDepth()
    {
        TreeNode root = new("A");
        TreeNode b = root.AddChildFromPayload("B");
        b.AddChildFromPayload("C");

        Assert.Equal(3, root.GetMaxY());
    }

    [Fact]
    public void GetYX_WhenCalled_ShouldReturnCorrectCoordinates()
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
        TreeNode root = new("root");
        root.AddChildFromPayload("Child");

        Assert.True(root.HasChildren);
    }

    [Fact]
    public void GetPath_WhenCalledOnRoot_ShouldReturnRootOnly()
    {
        TreeNode root = new("Root");

        string path = root.GetPath();

        Assert.Equal("1.1", path);
    }

    [Fact]
    public void GetPath_WhenCalledOnChild_Ok()
    {
        TreeNode root = new("Root");
        TreeNode child = root.AddChildFromPayload("Child");

        string path = child.GetPath();

        Assert.Equal("1.1/2.1", path);
    }

    [Fact]
    public void GetPath_WhenCalledOnGrandchild_Ok()
    {
        TreeNode root = new("Root");
        TreeNode child = root.AddChildFromPayload("Child");
        TreeNode grandchild = child.AddChildFromPayload("Grandchild");

        string path = grandchild.GetPath();

        Assert.Equal("1.1/2.1/3.1", path);
    }

    [Fact]
    public void GetPath_WhenCalledOnSecondChild_Ok()
    {
        TreeNode root = new("Root");
        root.AddChildFromPayload("Child1");
        TreeNode child2 = root.AddChildFromPayload("Child2");

        string path = child2.GetPath();

        Assert.Equal("1.1/2.2", path);
    }

    [Fact]
    public void GetPath_WhenCalledOnThirdChild_Ok()
    {
        TreeNode root = new("Root");
        root.AddChildFromPayload("Child1");
        root.AddChildFromPayload("Child2");
        TreeNode child3 = root.AddChildFromPayload("Child3");

        string path = child3.GetPath();

        Assert.Equal("1.1/2.3", path);
    }

    [Fact]
    public void GetPath_WhenCalledOnSecondGrandchildOfSecondChild_Ok()
    {
        TreeNode root = new("Root");
        // children
        root.AddChildFromPayload("Child1");
        TreeNode child2 = root.AddChildFromPayload("Child2");
        // grandchildren
        child2.AddChildFromPayload("Grandchild1");
        TreeNode grandchild2 = child2.AddChildFromPayload("Grandchild2");

        string path = grandchild2.GetPath();

        Assert.Equal("1.1/2.2/3.2", path);
    }
    #endregion
}
