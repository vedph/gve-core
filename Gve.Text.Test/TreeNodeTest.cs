using Xunit;

namespace Gve.Text.Test;

public sealed class TreeNodeTest
{
    [Fact]
    public void ToggleExpanded_WhenCollapsed_ShouldExpand()
    {
        TreeNode<string> node = new("A");
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
        TreeNode<string> node = new("A");
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
        TreeNode<string> node = new("A");
        node.AddChildFromPayload("B");
        node.AddChildFromPayload("C");
        node.SetExpanded(true);

        node.ToggleExpanded();

        Assert.False(node.IsExpanded);
    }

    [Fact]
    public void ToggleExpandedRecursive_WhenExpanded_ShouldCollapse()
    {
        TreeNode<string> node = new("A");
        node.AddChildFromPayload("B");
        node.AddChildFromPayload("C");
        node.SetExpanded(true);

        node.ToggleExpanded(true);

        Assert.False(node.IsExpanded);
        Assert.False(node.Children[0].IsExpanded);
        Assert.False(node.Children[1].IsExpanded);
    }
}
