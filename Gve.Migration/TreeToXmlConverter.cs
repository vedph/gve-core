using Fusi.Tools.Data;
using System;
using System.Xml.Linq;

namespace Gve.Migration;

/// <summary>
/// Converter from a tree to XML.
/// </summary>
/// <typeparam name="T">The tree node data type.</typeparam>
public sealed class TreeToXmlConverter<T>
{
    /// <summary>
    /// Gets or sets the name of the branching element.
    /// </summary>
    public XName BranchingElementName { get; set; } = "app";

    /// <summary>
    /// Gets or sets the name of the first branch element.
    /// </summary>
    public XName Branch1ElementName { get; set; } = "lem";

    /// <summary>
    /// Gets or sets the name of the second branch element.
    /// </summary>
    public XName Branch2ElementName { get; set; } = "rdg";

    /// <summary>
    /// Gets or sets the optional attribute injector.
    /// </summary>
    public Action<XElement, TreeNode<T>>? AttributeInjector { get; set; }

    private XElement BuildBranchXml(TreeNode<T> node)
    {
        // for a node with multiple children, create an app element
        if (node.Children.Count > 1)
        {
            XElement app = new(BranchingElementName);
            AttributeInjector?.Invoke(app, node);

            // first child
            XElement child1 = new(Branch1ElementName);
            AttributeInjector?.Invoke(child1, node.Children[0]);
            app.Add(child1);
            AddChildrenToElement(child1, node.Children[0]);

            // subsequent children
            for (int i = 1; i < node.Children.Count; i++)
            {
                XElement child2 = new(Branch2ElementName);
                AttributeInjector?.Invoke(child2, node.Children[i]);
                app.Add(child2);
                AddChildrenToElement(child2, node.Children[i]);
            }

            return app;
        }
        // for a node with single child, create an element with the node's content
        else if (node.Children.Count == 1)
        {
            XElement element = new(Branch1ElementName);
            AttributeInjector?.Invoke(element, node);
            AddChildrenToElement(element, node.Children[0]);
            return element;
        }
        // for a leaf node, create an element with just the node's content
        else
        {
            XElement element = new(Branch1ElementName);
            AttributeInjector?.Invoke(element, node);
            if (node.Data is not null) element.Add(node.Data.ToString());
            return element;
        }
    }

    private void AddChildrenToElement(XElement element, TreeNode<T> node)
    {
        if (!node.HasChildren)
        {
            if (node.Data is not null) element.Add(node.Data.ToString());
            return;
        }

        element.Add(BuildBranchXml(node));
    }

    /// <summary>
    /// Converts the specified tree.
    /// </summary>
    /// <param name="root">The root node of the tree.</param>
    /// <returns>The root XML element or null if root has no children.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public XElement? Convert(TreeNode<T> root)
    {
        ArgumentNullException.ThrowIfNull(root);

        if (!root.HasChildren) return null;

        return BuildBranchXml(root);
    }
}
