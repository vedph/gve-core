using Fusi.Tools.Data;
using System;
using System.Xml.Linq;

namespace Gve.Migration;

public class TreeToXmlConverter<T>
{
    public XName BranchingElementName { get; set; } = "app";
    public XName Branch1ElementName { get; set; } = "lem";
    public XName Branch2ElementName { get; set; } = "rdg";

    private readonly Action<XElement, TreeNode<T>> _attributeInjector;

    public TreeToXmlConverter(
        Action<XElement, TreeNode<T>> attributeInjector)
    {
        _attributeInjector = attributeInjector;
    }

    private XElement BuildBranchXml(TreeNode<T> node)
    {
        // For a node with multiple children, create an app element
        if (node.Children.Count > 1)
        {
            XElement app = new(BranchingElementName);
            _attributeInjector(app, node);

            // First child goes in lem
            XElement lem = new(Branch1ElementName);
            _attributeInjector(lem, node.Children[0]);
            app.Add(lem);
            AddChildrenToElement(lem, node.Children[0]);

            // Subsequent children each go in rdg
            for (int i = 1; i < node.Children.Count; i++)
            {
                XElement rdg = new(Branch2ElementName);
                _attributeInjector(rdg, node.Children[i]);
                app.Add(rdg);
                AddChildrenToElement(rdg, node.Children[i]);
            }

            return app;
        }
        // For a node with single child, create an element with the node's content
        else if (node.Children.Count == 1)
        {
            XElement element = new(Branch1ElementName);
            _attributeInjector(element, node);
            AddChildrenToElement(element, node.Children[0]);
            return element;
        }
        // For a leaf node, create an element with just the node's content
        else
        {
            XElement element = new(Branch1ElementName);
            _attributeInjector(element, node);
            if (node.Data != null)
            {
                element.Add(node.Data.ToString());
            }
            return element;
        }
    }

    private void AddChildrenToElement(XElement element, TreeNode<T> node)
    {
        if (!node.HasChildren)
        {
            if (node.Data != null)
            {
                element.Add(node.Data.ToString());
            }
            return;
        }

        element.Add(BuildBranchXml(node));
    }

    public XElement Convert(TreeNode<T> root)
    {
        if (!root.HasChildren)
        {
            XElement element = new(Branch1ElementName);
            _attributeInjector(element, root);
            return element;
        }

        return BuildBranchXml(root);
    }
}
