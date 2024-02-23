using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gve.Text;

/// <summary>
/// A generic tree node.
/// </summary>
public class TreeNode
{
    private List<TreeNode>? _children;

    /// <summary>
    /// Gets or sets an optional identifier for this node.
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    /// Gets or sets the optional label for this node.
    /// </summary>
    public string? Label { get; set; }

    /// <summary>
    /// Gets or sets the payload data.
    /// </summary>
    public object? Data { get; set; }

    /// <summary>
    /// Gets or sets the parent.
    /// </summary>
    public TreeNode? Parent { get; set; }

    /// <summary>
    /// Gets or sets the children.
    /// </summary>
    public List<TreeNode> Children
    {
        get => _children ??= [];
        set => _children = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary>
    /// Gets a value indicating whether this instance has children.
    /// </summary>
    public bool HasChildren => _children?.Count > 0;

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
    /// Initializes a new instance of the <see cref="TreeNode"/> class.
    /// </summary>
    /// <param name="data">The optional payload data.</param>
    public TreeNode(object? data = null)
    {
        Data = data;
    }

    /// <summary>
    /// Gets the data cast to the specified type.
    /// </summary>
    /// <typeparam name="T">Type to cast to.</typeparam>
    /// <returns>Data or default for its type.</returns>
    public T? GetData<T>() => Data is T data ? data : default;

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
        TreeNode? node = this;
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
        foreach (TreeNode sibling in Parent.Children)
        {
            if (sibling == this) return Tuple.Create(y, x);
            x++;
        }
        throw new InvalidOperationException("Parent does not contain this node.");
    }

    /// <summary>
    /// Gets the path relative to this node, represented by a string where
    /// each level is represented by <c>/</c> followed by the sibling number
    /// (1-N=X) of the child to pick from the current node. For instance,
    /// <c>/2/1</c> means that the second child of this node is picked, and
    /// then the first child of that node is picked.
    /// </summary>
    /// <returns>String.</returns>
    public string GetPath()
    {
        StringBuilder sb = new();
        TreeNode? node = this;
        while (node.Parent != null)
        {
            int x = 1;
            foreach (TreeNode sibling in node.Parent.Children)
            {
                if (sibling == node)
                {
                    sb.Insert(0, $"/{x}");
                    break;
                }
                x++;
            }
            node = node.Parent;
        }
        return sb.ToString();
    }

    /// <summary>
    /// Walks the specified path to get its target node, relative to this node.
    /// </summary>
    /// <param name="path">The path, as generated by <see cref="GetPath"/>.
    /// An empty path just returns this node.</param>
    /// <returns>Target node, or null if not found.</returns>
    /// <exception cref="ArgumentNullException">path</exception>
    public TreeNode? WalkPathTo(string path)
    {
        ArgumentNullException.ThrowIfNull(path);

        if (path.Length == 0) return this;
        if (path[0] != '/') path = '/' + path;

        string[] parts = path.Split('/');
        TreeNode? node = this;
        // we skip 1st which is empty
        foreach (string part in parts.Skip(1))
        {
            if (node == null) return null;

            if (!int.TryParse(part, out int x)) return null;
            if (x < 1 || x > node.Children.Count) return null;

            node = node.Children[x - 1];
        }

        return node;
    }
    #endregion

    #region Clone
    /// <summary>
    /// Deeply clones this instance and all its descendants. This creates a new
    /// tree node from this node, and clones all the descendants recursively.
    /// Payload data are not cloned.
    /// </summary>
    /// <returns>New node.</returns>
    public TreeNode Clone()
    {
        TreeNode clone = new(Data)
        {
            Id = Id,
            Label = Label
        };
        foreach (TreeNode child in Children) clone.AddChild(child.Clone());
        return clone;
    }
    #endregion

    #region Add or Remove
    /// <summary>
    /// Adds the specified node as a child of this node.
    /// </summary>
    /// <param name="child">The child.</param>
    /// <exception cref="ArgumentNullException">child</exception>
    public void AddChild(TreeNode child)
    {
        ArgumentNullException.ThrowIfNull(child);

        child.Parent = this;
        Children.Add(child);
    }

    /// <summary>
    /// Adds a new node as a child of this node creating it from the specified
    /// payload data.
    /// </summary>
    /// <param name="data">The payload data.</param>
    /// <returns>The newly created child node.</returns>
    public TreeNode AddChildFromData(object? data)
    {
        TreeNode child = new(data);
        AddChild(child);
        return child;
    }

    /// <summary>
    /// Adds the specified nodes as children of this node.
    /// </summary>
    /// <param name="children">The children.</param>
    /// <exception cref="ArgumentNullException">children</exception>
    public void AddChildren(IEnumerable<TreeNode> children)
    {
        ArgumentNullException.ThrowIfNull(children);

        foreach (TreeNode child in children) AddChild(child);
    }

    /// <summary>
    /// Adds new nodes as children of this node creating them from the
    /// specified payload data.
    /// </summary>
    /// <param name="data">The payload data.</param>
    public void AddChildrenFromData(IEnumerable<object?> data)
    {
        foreach (object? d in data) AddChildFromData(d);
    }

    /// <summary>
    /// Adds the specified node before this node.
    /// </summary>
    /// <param name="node">The node.</param>
    /// <exception cref="InvalidOperationException">Parent is null,  or
    /// Parent does not contain this node.</exception>
    public void AddBeforeSelf(TreeNode node)
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
    /// Adds a new node before this node creating it from the specified payload
    /// data.
    /// </summary>
    /// <param name="data">The data.</param>
    /// <returns>The newly added node.</returns>
    /// <exception cref="InvalidOperationException">Parent is null,  or
    /// Parent does not contain this node.</exception>
    public TreeNode AddBeforeSelfFromData(object? data)
    {
        TreeNode node = new(data);
        AddBeforeSelf(node);
        return node;
    }

    /// <summary>
    /// Adds the specified node after this node.
    /// </summary>
    /// <param name="node">The node.</param>
    /// <exception cref="InvalidOperationException">Parent is null,  or
    /// Parent does not contain this node.</exception>
    public void AddAfterSelf(TreeNode node)
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

    /// <summary>
    /// Adds a new node after this node creating it from the specified payload
    /// data.
    /// </summary>
    /// <param name="data">The data.</param>
    /// <returns>The newly added node.</returns>
    /// <exception cref="InvalidOperationException">Parent is null,  or
    /// Parent does not contain this node.</exception>
    public TreeNode AddAfterSelfFromData(object? data)
    {
        TreeNode node = new(data);
        AddAfterSelf(node);
        return node;
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
    public bool RemoveChild(TreeNode child)
    {
        ArgumentNullException.ThrowIfNull(child);

        if (Children.Remove(child))
        {
            child.Parent = null;
            return true;
        }
        return false;
    }
    #endregion

    #region Traversing
    /// <summary>
    /// Traverse the tree in depth-first order.
    /// </summary>
    /// <param name="visitor">The visitor object to use.</param>
    /// <exception cref="ArgumentNullException">visitor</exception>
    public void Traverse(ITreeNodeVisitor visitor)
    {
        ArgumentNullException.ThrowIfNull(visitor);

        visitor.Visit(this);
        foreach (TreeNode child in Children) child.Traverse(visitor);
    }

    /// <summary>
    /// Traverses the tree in depth-first order, calling the specified visitor
    /// function for each node.
    /// </summary>
    /// <param name="visitorFunc">The visitor function. This returns false to
    /// stop the traversal.</param>
    /// <returns>True to continue traversing, false to stop.</returns>
    /// <exception cref="ArgumentNullException">visitorFunc</exception>
    public bool Traverse(Func<TreeNode, bool> visitorFunc)
    {
        ArgumentNullException.ThrowIfNull(visitorFunc);

        if (!visitorFunc(this)) return false;

        foreach (TreeNode child in Children)
        {
            if (!child.Traverse(visitorFunc)) return false;
        }

        return true;
    }

    /// <summary>
    /// Gets the root node.
    /// </summary>
    /// <returns>Root node.</returns>
    public TreeNode GetRoot()
    {
        TreeNode node = this;
        while (node.Parent != null) node = node.Parent;
        return node;
    }

    /// <summary>
    /// Gets all the ascendants of this node.
    /// </summary>
    /// <returns>Nodes.</returns>
    public IEnumerable<TreeNode> GetAscendants()
    {
        TreeNode node = this;
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
    public TreeNode? GetFirstAscendant(Func<TreeNode, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);

        TreeNode node = this;
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
    public TreeNode? GetFirstAscendantOrSelf(
        Func<TreeNode, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);

        TreeNode? node = this;
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
    public IEnumerable<TreeNode> GetDescendants()
    {
        foreach (TreeNode child in Children)
        {
            yield return child;
            foreach (TreeNode grandchild in child.GetDescendants())
                yield return grandchild;
        }
    }

    /// <summary>
    /// Gets the first descendant node matching the specified predicate.
    /// </summary>
    /// <param name="predicate">The predicate.</param>
    /// <returns>Node or null if not found.</returns>
    /// <exception cref="ArgumentNullException">predicate</exception>
    public TreeNode? GetFirstDescendant(Func<TreeNode, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);

        foreach (TreeNode child in Children)
        {
            if (predicate(child)) return child;
            TreeNode? descendant = child.GetFirstDescendant(predicate);
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
    public TreeNode? GetFirstDescendantOrSelf(
        Func<TreeNode, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);

        if (predicate(this)) return this;

        foreach (TreeNode child in Children)
        {
            TreeNode? descendant = child.GetFirstDescendantOrSelf(predicate);
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
    public bool HasAscendant(TreeNode node) =>
        GetAscendants().Any(ascendant => ascendant == node);

    /// <summary>
    /// Determines whether the specified node is a descendant of this node.
    /// </summary>
    /// <param name="node">The node.</param>
    /// <returns><c>true</c> if the specified node is a descendant of this
    /// node; otherwise, <c>false</c>.</returns>
    public bool HasDescendant(TreeNode node) =>
        GetDescendants().Any(descendant => descendant == node);

    /// <summary>
    /// Gets the left siblings of this node.
    /// </summary>
    /// <returns>Nodes.</returns>
    public IEnumerable<TreeNode> GetLeftSiblings()
    {
        if (Parent == null) yield break;

        foreach (TreeNode sibling in Parent.Children)
        {
            if (sibling == this) yield break;
            yield return sibling;
        }
    }

    /// <summary>
    /// Gets the right siblings of this node.
    /// </summary>
    /// <returns>Nodes.</returns>
    public IEnumerable<TreeNode> GetRightSiblings()
    {
        if (Parent == null) yield break;

        bool foundSelf = false;
        foreach (TreeNode sibling in Parent.Children)
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
    public bool HasLeftSibling(TreeNode node) =>
        GetLeftSiblings().Any(sibling => sibling == node);

    /// <summary>
    /// Determines whether the specified node is among the right siblings
    /// of this node.
    /// </summary>
    /// <param name="node">The node.</param>
    /// <returns><c>true</c> if the specified node is among the right siblings
    /// of this node; otherwise, <c>false</c>.</returns>
    public bool HasRightSibling(TreeNode node) =>
        GetRightSiblings().Any(sibling => sibling == node);
    #endregion

    #region Flattening
    /// <summary>
    /// Returns a flat list of tuples representing the tree or sub-tree
    /// starting with this node.
    /// </summary>
    /// <returns>List of items.</returns>
    public IList<IFlatTreeNodeItem> ToFlatList()
    {
        List<IFlatTreeNodeItem> items = [];

        Traverse((n) =>
        {
            items.Add(new FlatTreeNodeItem(
                n.Id!,
                n.Parent?.Id,
                n.Data));
            return true;
        });

        return items;
    }

    /// <summary>
    /// Creates a tree from a flat list of items, each having an ID and a
    /// parent ID.
    /// </summary>
    /// <param name="items">The items.</param>
    /// <returns>The root node of the output tree or null.</returns>
    /// <exception cref="ArgumentNullException">items</exception>
    public static TreeNode? FromFlatList(IList<IFlatTreeNodeItem> items)
    {
        ArgumentNullException.ThrowIfNull(items);

        if (items.Count == 0) return null;

        // create a dictionary of nodes keyed by IDs
        Dictionary<string, TreeNode> dct =
            items.ToDictionary(
                i => i.Id,
                v => new TreeNode(v.Data));

        // place each node under its parent
        TreeNode? root = null;
        foreach (IFlatTreeNodeItem item in items)
        {
            TreeNode itemNode = dct[item.Id];
            if (item.ParentId != null &&
                dct.TryGetValue(item.ParentId, out TreeNode? value))
            {
                value.AddChild(itemNode);
            }
            else
            {
                root = itemNode;
            }
        }

        return root;
    }
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
        if (Data != null) sb.Append(Data);
        return sb.ToString();
    }
}

/// <summary>
/// Interface for a tree node visitor in <see cref="TreeNode"/>.
/// </summary>
public interface ITreeNodeVisitor
{
    /// <summary>
    /// Visits the specified node.
    /// </summary>
    /// <param name="node">The node.</param>
    void Visit(TreeNode node);
}
