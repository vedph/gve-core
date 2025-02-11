using Fusi.Tools.Data;
using System;
using System.Linq;
using System.Xml.Linq;

namespace Gve.Migration.Test;

public sealed class TreeToXmlConverterTest
{
    [Fact]
    public void Convert_ShouldThrowArgumentNullException_WhenRootIsNull()
    {
        TreeToXmlConverter<string> converter = new();

        Assert.Throws<ArgumentNullException>(() => converter.Convert(null!));
    }

    [Fact]
    public void Convert_ShouldReturnNull_WhenRootHasNoChildren()
    {
        TreeNode<string> root = new();
        TreeToXmlConverter<string> converter = new();

        XElement? result = converter.Convert(root);

        Assert.Null(result);
    }

    [Fact]
    public void Convert_ShouldReturnElementWithChildren_WhenRootHasChildren()
    {
        TreeNode<string> root = new();
        TreeNode<string> child1 = new() { Data = "child1" };
        TreeNode<string> child2 = new() { Data = "child2" };
        root.Children.Add(child1);
        root.Children.Add(child2);

        TreeToXmlConverter<string> converter = new();

        XElement? result = converter.Convert(root);

        Assert.NotNull(result);
        Assert.Equal(converter.BranchingElementName, result.Name);
        Assert.Equal(2, result.Elements().Count());
        Assert.Equal(converter.Branch1ElementName, result.Elements().First().Name);
        Assert.Equal(converter.Branch2ElementName, result.Elements().Last().Name);
    }

    [Fact]
    public void Convert_ShouldInvokeAttributeInjector_WhenProvided()
    {
        // - root
        //  - A
        //  - X
        TreeNode<string> root = new();
        TreeNode<string> a = new() { Data = "A" };
        TreeNode<string> x = new() { Data = "X" };
        root.AddChild(a);
        root.AddChild(x);
        root.Children.Add(a);
        root.Children.Add(x);

        TreeToXmlConverter<string> converter = new()
        {
            AttributeInjector = (element, node) =>
                element.SetAttributeValue("data", node.Data)
        };

        XElement? result = converter.Convert(root);

        // root
        Assert.NotNull(result);
        Assert.Null(result.Attribute("data")?.Value);

        // root/app/lem is A
        XElement? lem = result.Element(converter.Branch1ElementName);
        Assert.NotNull(lem);
        Assert.Equal("A", lem.Attribute("data")?.Value);
        Assert.Equal("A", lem.Value);

        // root/app/rdg is X
        XElement? rdg = result.Element(converter.Branch2ElementName);
        Assert.NotNull(rdg);
        Assert.Equal("X", rdg.Attribute("data")?.Value);
        Assert.Equal("X", rdg.Value);
    }
}
