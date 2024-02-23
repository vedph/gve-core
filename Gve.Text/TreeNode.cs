using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gve.Text;

/// <summary>
/// A generic tree node.
/// </summary>
/// <typeparam name="T">The type of payload data for each node.</typeparam>
public class TreeNode<T>
{
    /// <summary>
    /// Gets or sets the optional label for this node.
    /// </summary>
    public string? Label { get; set; }

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
    /// Gets a value indicating whether this instance has children.
    /// </summary>
    public bool HasChildren => Children.Count > 0;

    /// <summary>
    /// Gets or sets a value indicating whether this node is expanded in some
    /// kind of visual representation. Note that expanded state is not dependent
    /// on the presence of children.
    /// </summary>
    public bool IsExpanded { get; set; }

    /// <summary>
    /// Determines whether this node is the first sibling.
    /// </summary>
    /// <returns><c>true</c> if is first sibling.</returns>
    public bool IsFirstSibling() => Parent?.Children[0] == this;

    /// <summary>
    /// Determines whether this node is the last sibling.
    /// </summary>
    /// <returns><c>true</c> if is last sibling.</returns>
    public bool IsLastSibling() => Parent?.Children[^1] == this;

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

    #region Expansion
    /// <summary>
    /// Sets the expanded state for this node and all its descendants.
    /// </summary>
    /// <param name="expanded">the new state for expanded.</param>
    /// <param name="recursive">if set to <c>true</c>, recursively propagate
    /// the new expanded state to all the descendants of this node.</param>
    public void SetExpanded(bool expanded, bool recursive = false)
    {
        IsExpanded = expanded;
        if (recursive)
        {
            Traverse(node =>
            {
                node.IsExpanded = IsExpanded;
                return true;
            });
        }
    }

    /// <summary>
    /// Toggles the expanded state for this node and optionally for all its
    /// descendants.
    /// </summary>
    /// <param name="recursive">if set to <c>true</c>, recursively propagate
    /// the new expanded state to all the descendants of this node.</param>
    public void ToggleExpanded(bool recursive = false) =>
        SetExpanded(!IsExpanded, recursive);
    #endregion

    #region Path
    /// <summary>
    /// Gets the depth level (Y: 1-N).
    /// </summary>
    public int GetY()
    {
        int level = 1;
        TreeNode<T>? node = this;
        while (node.Parent != null)
        {
            level++;
            node = node.Parent;
        }
        return level;
    }

    /// <summary>
    /// Gets the maximum depth level (Y) reachable from this node.
    /// </summary>
    /// <returns>The maximum depth level (1-N).</returns>
    public int GetMaxY()
    {
        int depth = GetY();
        Traverse((node) =>
        {
            if (depth < node.GetY()) depth = node.GetY();
            return true;
        });
        return depth;
    }

    /// <summary>
    /// Gets the Y (depth, 1-N) and X (1-N, sibling ordinal number) coordinates
    /// of this node.
    /// </summary>
    /// <returns>Tuple with Y and X.</returns>
    /// <exception cref="InvalidOperationException">Parent does not contain
    /// this node</exception>
    public Tuple<int, int> GetYX()
    {
        int y = GetY();
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
    /// Gets the path to this node represented by a string where each node
    /// is separated by a slash, and labeled with its Y.X values.
    /// </summary>
    /// <returns>String.</returns>
    public string GetPath()
    {
        (int y, int x) = GetYX();
        string prefix = string.Join("/", GetAscendants().Reverse()
            .Select(node => $"{node.GetYX().Item1}.{node.GetYX().Item2}"));

        return prefix.Length > 0 ? $"{prefix}/{y}.{x}" : $"{y}.{x}";
    }

    /// <summary>
    /// Walks the specified path to get its target node.
    /// </summary>
    /// <param name="path">The path, as generated by <see cref="GetPath"/>.
    /// </param>
    /// <returns>Target node, or null if not found.</returns>
    public TreeNode<T>? WalkPathTo(string path)
    {
        TreeNode<T>? node = this;
        foreach (string part in path.Split('/'))
        {
            if (node == null) return null;
            int dotIndex = part.IndexOf('.');
            if (dotIndex < 0) return null;
            if (!int.TryParse(part[..dotIndex], out int y)) return null;
            if (!int.TryParse(part[(dotIndex + 1)..], out int x)) return null;
            if (y != node.GetY()) return null;
            if (x < 1 || x > node.Parent?.Children.Count) return null;
            node = node.Parent?.Children[x - 1];
        }
        return node;
    }
    #endregion

    #region Clone
    /// <summary>
    /// Deeply clones this instance and all its descendants. This creates a new
    /// tree node from this node, and clones all the descendants recursively.
    /// Payloads are not cloned.
    /// </summary>
    /// <returns>New node.</returns>
    public TreeNode<T> Clone()
    {
        TreeNode<T> newNode = new(Payload)
        {
            Label = Label
        };
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
        TreeNode<P> newNode = new(transformFunc(Payload))
        {
            Label = Label
        };
        foreach (TreeNode<T> child in Children)
        {
            newNode.AddChild(child.Clone(transformFunc));
        }
        return newNode;
    }
    #endregion

    #region Add or Remove
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
    /// Adds a new node as a child of this node creating it from the specified
    /// payload.
    /// </summary>
    /// <param name="payload">The payload.</param>
    /// <returns>The newly created child node.</returns>
    public TreeNode<T> AddChildFromPayload(T? payload)
    {
        TreeNode<T> child = new(payload);
        AddChild(child);
        return child;
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
    /// Adds new nodes as children of this node creating them from the
    /// specified payloads.
    /// </summary>
    /// <param name="payloads">The payloads.</param>
    public void AddChildrenFromPayload(IEnumerable<T?> payloads)
    {
        foreach (T? payload in payloads) AddChildFromPayload(payload);
    }

    /// <summary>
    /// Removes this node from its parent.
    /// </summary>
    public void Remove() => Parent?.RemoveChild(this);

    /// <summary>
    /// Removes the specified node from the children of this node.
    /// </summary>
    /// <param name="child">The child node to remove.</param>
    /// <returns>True if removed, false if not found.</returns>
    public bool RemoveChild(TreeNode<T> child)
    {
        ArgumentNullException.ThrowIfNull(child);

        if (Children.Remove(child))
        {
            child.Parent = null;
            return true;
        }
        return false;
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

        node.Parent = Parent;
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

        node.Parent = Parent;
        Parent.Children.Insert(index + 1, node);
    }
    #endregion

    #region Traversing
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
    /// Gets the first ascendant node matching the specified predicate.
    /// </summary>
    /// <param name="predicate">The predicate.</param>
    /// <returns>Node or null if not found.</returns>
    /// <exception cref="ArgumentNullException">predicate</exception>
    public TreeNode<T>? GetFirstAscendant(Func<TreeNode<T>, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);

        TreeNode<T> node = this;
        while (node.Parent != null)
        {
            if (predicate(node.Parent)) return node.Parent;
            node = node.Parent;
        }
        return null;
    }

    /// <summary>
    /// Gets either this node or the first ascendant node matching the
    /// specified predicate.
    /// </summary>
    /// <param name="predicate">The predicate.</param>
    /// <returns>Node or null if not found.</returns>
    /// <exception cref="ArgumentNullException">predicate</exception>
    public TreeNode<T>? GetFirstAscendantOrSelf(
        Func<TreeNode<T>, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);

        TreeNode<T>? node = this;
        while (node != null)
        {
            if (predicate(node)) return node;
            node = node.Parent;
        }
        return null;
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
    /// Gets the first descendant node matching the specified predicate.
    /// </summary>
    /// <param name="predicate">The predicate.</param>
    /// <returns>Node or null if not found.</returns>
    /// <exception cref="ArgumentNullException">predicate</exception>
    public TreeNode<T>? GetFirstDescendant(Func<TreeNode<T>, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);

        foreach (TreeNode<T> child in Children)
        {
            if (predicate(child)) return child;
            TreeNode<T>? descendant = child.GetFirstDescendant(predicate);
            if (descendant != null) return descendant;
        }
        return null;
    }

    /// <summary>
    /// Gets either this node or the first descendant node matching the
    /// specified predicate.
    /// </summary>
    /// <param name="predicate">The predicate.</param>
    /// <returns>Node or null if not found.</returns>
    /// <exception cref="ArgumentNullException">predicate</exception>
    public TreeNode<T>? GetFirstDescendantOrSelf(
        Func<TreeNode<T>, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);

        if (predicate(this)) return this;

        foreach (TreeNode<T> child in Children)
        {
            TreeNode<T>? descendant = child.GetFirstDescendantOrSelf(predicate);
            if (descendant != null) return descendant;
        }
        return null;
    }

    /// <summary>
    /// Determines whether the specified node is an ascendant of this node.
    /// </summary>
    /// <param name="node">The node.</param>
    /// <returns><c>true</c> if the specified node is an ascendant of this
    /// node; otherwise, <c>false</c>.</returns>
    public bool HasAscendant(TreeNode<T> node) =>
        GetAscendants().Any(ascendant => ascendant == node);

    /// <summary>
    /// Determines whether the specified node is a descendant of this node.
    /// </summary>
    /// <param name="node">The node.</param>
    /// <returns><c>true</c> if the specified node is a descendant of this
    /// node; otherwise, <c>false</c>.</returns>
    public bool HasDescendant(TreeNode<T> node) =>
        GetDescendants().Any(descendant => descendant == node);

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

    /// <summary>
    /// Determines whether the specified node is among the left siblings
    /// of this node.
    /// </summary>
    /// <param name="node">The node.</param>
    /// <returns><c>true</c> if the specified node is among the left siblings
    /// of this node; otherwise, <c>false</c>.</returns>
    public bool HasLeftSibling(TreeNode<T> node) =>
        GetLeftSiblings().Any(sibling => sibling == node);

    /// <summary>
    /// Determines whether the specified node is among the right siblings
    /// of this node.
    /// </summary>
    /// <param name="node">The node.</param>
    /// <returns><c>true</c> if the specified node is among the right siblings
    /// of this node; otherwise, <c>false</c>.</returns>
    public bool HasRightSibling(TreeNode<T> node) =>
        GetRightSiblings().Any(sibling => sibling == node);
    #endregion

    /// <summary>
    /// Dumps this node into a multiline string representing an ASCII tree.
    /// </summary>
    /// <returns>String.</returns>
    public string Dump()
    {
        StringBuilder sb = new();
        Traverse((node) =>
        {
            int y = node.GetY();
            sb.Append(' ', y - 1)
              .Append(node.HasChildren ? "+ " : "- ");
            sb.Append(node).AppendLine();
            return true;
        });
        return sb.ToString();
    }

    /// <summary>
    /// Converts to string.
    /// </summary>
    /// <returns>String</returns>
    public override string ToString()
    {
        (int y, int x) = GetYX();
        StringBuilder sb = new();
        sb.Append('[').Append(y).Append('.').Append(x).Append("] ");

        if (IsExpanded) sb.Append("V| ");
        else if (HasChildren) sb.Append(">| ");

        if (Label != null) sb.Append(Label).Append(": ");
        if (Payload != null) sb.Append(Payload);
        return sb.ToString();
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
