using System;
using System.Collections.Generic;

namespace Gve.Text;

/// <summary>
/// A generic tree node.
/// </summary>
/// <typeparam name="T">The type of payload data for each node.</typeparam>
public class TreeNode<T>
{
    /// <summary>
    /// Gets or sets the payload.
    /// </summary>
    public T? Payload { get; set; }

    /// <summary>
    /// Gets or sets the parent.
    /// </summary>
    public TreeNode<T>? Parent { get; set; }

    /// <summary>
    /// Gets or sets the children.
    /// </summary>
    public List<TreeNode<T>> Children { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TreeNode{T}"/> class.
    /// </summary>
    public TreeNode()
    {
        Children = [];
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TreeNode{T}"/> class.
    /// </summary>
    /// <param name="payload">The payload.</param>
    public TreeNode(T? payload)
    {
        Payload = payload;
        Children = [];
    }

    /// <summary>
    /// Gets the depth.
    /// </summary>
    public int GetDepth()
    {
        int depth = 0;
        TreeNode<T>? node = this;
        while (node.Parent != null)
        {
            depth++;
            node = node.Parent;
        }
        return depth;
    }

    /// <summary>
    /// Gets the Y (depth) and X (sibling ordinal number) coordinates of
    /// this node.
    /// </summary>
    /// <returns>Tuple with Y and X.</returns>
    /// <exception cref="InvalidOperationException">Parent does not contain
    /// this node</exception>
    public Tuple<int, int> GetYX()
    {
        int y = GetDepth();
        if (Parent == null) return Tuple.Create(y, 1);

        int x = 1;
        foreach (TreeNode<T> sibling in Parent.Children)
        {
            if (sibling == this) return Tuple.Create(y, x);
            x++;
        }
        throw new InvalidOperationException("Parent does not contain this node.");
    }

    /// <summary>
    /// Gets the root node.
    /// </summary>
    /// <returns>Root node.</returns>
    public TreeNode<T> GetRoot()
    {
        TreeNode<T> node = this;
        while (node.Parent != null) node = node.Parent;
        return node;
    }

    /// <summary>
    /// Deeply clones this instance and all its descendants. This creates a new
    /// tree node from this node, and clones all the descendants recursively.
    /// Payloads are not cloned.
    /// </summary>
    /// <returns>New node.</returns>
    public TreeNode<T> Clone()
    {
        TreeNode<T> newNode = new(Payload);
        foreach (TreeNode<T> child in Children) newNode.AddChild(child.Clone());
        return newNode;
    }

    /// <summary>
    /// Deeply clones this instance and all its descendants, applying to each
    /// node's payload the specified transform function.
    /// </summary>
    /// <typeparam name="P">The type of payload to transform to.</typeparam>
    /// <param name="transformFunc">The transform function.</param>
    /// <returns>Node.</returns>
    public TreeNode<P> Clone<P>(Func<T?, P> transformFunc)
    {
        TreeNode<P> newNode = new(transformFunc(Payload));
        foreach (TreeNode<T> child in Children)
        {
            newNode.AddChild(child.Clone(transformFunc));
        }
        return newNode;
    }

    /// <summary>
    /// Adds the specified node as a child of this node.
    /// </summary>
    /// <param name="child">The child.</param>
    /// <exception cref="ArgumentNullException">child</exception>
    public void AddChild(TreeNode<T> child)
    {
        ArgumentNullException.ThrowIfNull(child);

        child.Parent = this;
        Children.Add(child);
    }

    /// <summary>
    /// Adds the specified nodes as children of this node.
    /// </summary>
    /// <param name="children">The children.</param>
    /// <exception cref="ArgumentNullException">children</exception>
    public void AddChildren(IEnumerable<TreeNode<T>> children)
    {
        ArgumentNullException.ThrowIfNull(children);

        foreach (TreeNode<T> child in children) AddChild(child);
    }

    /// <summary>
    /// Adds the specified node before this node.
    /// </summary>
    /// <param name="node">The node.</param>
    /// <exception cref="InvalidOperationException">Parent is null,  or
    /// Parent does not contain this node.</exception>
    public void AddBeforeSelf(TreeNode<T> node)
    {
        ArgumentNullException.ThrowIfNull(node);

        if (Parent == null)
            throw new InvalidOperationException("Parent is null.");

        int index = Parent.Children.IndexOf(this);
        if (index < 0)
            throw new InvalidOperationException("Parent does not contain this node.");

        Parent.Children.Insert(index, node);
    }

    /// <summary>
    /// Adds the specified node after this node.
    /// </summary>
    /// <param name="node">The node.</param>
    /// <exception cref="InvalidOperationException">Parent is null,  or
    /// Parent does not contain this node.</exception>
    public void AddAfterSelf(TreeNode<T> node)
    {
        ArgumentNullException.ThrowIfNull(node);

        if (Parent == null)
            throw new InvalidOperationException("Parent is null.");

        int index = Parent.Children.IndexOf(this);
        if (index < 0)
            throw new InvalidOperationException("Parent does not contain this node.");

        Parent.Children.Insert(index + 1, node);
    }

    /// <summary>
    /// Traverse the tree in depth-first order.
    /// </summary>
    /// <param name="visitor">The visitor object to use.</param>
    /// <exception cref="ArgumentNullException">visitor</exception>
    public void Traverse(ITreeNodeVisitor<T> visitor)
    {
        ArgumentNullException.ThrowIfNull(visitor);

        visitor.Visit(this);
        foreach (TreeNode<T> child in Children) child.Traverse(visitor);
    }

    /// <summary>
    /// Traverses the tree in depth-first order, calling the specified visitor
    /// function for each node.
    /// </summary>
    /// <param name="visitorFunc">The visitor function. This returns false to
    /// stop the traversal.</param>
    /// <returns>True to continue traversing, false to stop.</returns>
    /// <exception cref="ArgumentNullException">visitorFunc</exception>
    public bool Traverse(Func<TreeNode<T>, bool> visitorFunc)
    {
        ArgumentNullException.ThrowIfNull(visitorFunc);

        if (!visitorFunc(this)) return false;

        foreach (TreeNode<T> child in Children)
        {
            if (!child.Traverse(visitorFunc)) return false;
        }

        return true;
    }

    /// <summary>
    /// Gets all the ascendants of this node.
    /// </summary>
    /// <returns>Nodes.</returns>
    public IEnumerable<TreeNode<T>> GetAscendants()
    {
        TreeNode<T> node = this;
        while (node.Parent != null)
        {
            yield return node.Parent;
            node = node.Parent;
        }
    }

    /// <summary>
    /// Gets all the descendants of this node.
    /// </summary>
    /// <returns>Nodes.</returns>
    public IEnumerable<TreeNode<T>> GetDescendants()
    {
        foreach (TreeNode<T> child in Children)
        {
            yield return child;
            foreach (TreeNode<T> grandchild in child.GetDescendants())
                yield return grandchild;
        }
    }

    /// <summary>
    /// Gets the left siblings of this node.
    /// </summary>
    /// <returns>Nodes.</returns>
    public IEnumerable<TreeNode<T>> GetLeftSiblings()
    {
        if (Parent == null) yield break;

        foreach (TreeNode<T> sibling in Parent.Children)
        {
            if (sibling == this) yield break;
            yield return sibling;
        }
    }

    /// <summary>
    /// Gets the right siblings of this node.
    /// </summary>
    /// <returns>Nodes.</returns>
    public IEnumerable<TreeNode<T>> GetRightSiblings()
    {
        if (Parent == null) yield break;

        bool foundSelf = false;
        foreach (TreeNode<T> sibling in Parent.Children)
        {
            if (foundSelf)
            {
                yield return sibling;
            }
            else if (sibling == this)
            {
                foundSelf = true;
            }
        }
    }
}

/// <summary>
/// Interface for a tree node visitor.
/// </summary>
/// <typeparam name="T">The type of node's payload.</typeparam>
public interface ITreeNodeVisitor<T>
{
    /// <summary>
    /// Visits the specified node.
    /// </summary>
    /// <param name="node">The node.</param>
    void Visit(TreeNode<T> node);
}
