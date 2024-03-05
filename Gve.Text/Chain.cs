using System;
using System.Collections.Generic;
using System.Linq;

namespace Gve.Text;

/// <summary>
/// A chain data structure. This data structure consists of a linear sequence
/// of nodes, each connected to the previous and the next node by a link.
/// Additionally, each link has a version tag, which adds the dimension of time
/// to this structure. Internally, this structure collects nodes and links,
/// and once they are added, they cannot be removed (unless we are using a
/// single version). Any modification is done by adding new nodes and links,
/// which connect or disconnect or reoder the existing nodes.
/// </summary>
public class Chain<T>
{
    /// <summary>
    /// Gets or sets the nodes in this chain.
    /// </summary>
    protected List<ChainNode<T>> Nodes { get; set; } = [];

    /// <summary>
    /// Gets or sets the links connecting nodes in this chain.
    /// </summary>
    protected List<ChainLink<T>> Links { get; set; } = [];

    /// <summary>
    /// Gets the a list of unique tags from all the links in this chain.
    /// </summary>
    /// <returns>List, sorted alphabetically.</returns>
    public IList<string> GetDistinctTags()
    {
        return [.. Links.Select(l => l.Tag).Distinct().OrderBy(t => t)];
    }

    /// <summary>
    /// Gets the first node for the specified version tag.
    /// </summary>
    /// <param name="tag">The version tag.</param>
    /// <returns>Node or null.</returns>
    public ChainNode<T>? GetFirstNode(string tag)
    {
        if (Nodes.Count == 0) return null;
        if (Nodes.Count == 1) return Nodes[0];

        // find the node not being the target of any link for the specified tag
        return Nodes.Find(n => !Links.Any(l => l.Tag == tag && l.Target == n));
    }

    /// <summary>
    /// Gets the last node for the specified version tag.
    /// </summary>
    /// <param name="tag">The tag.</param>
    /// <returns>Node or null.</returns>
    public ChainNode<T>? GetLastNode(string tag)
    {
        if (Nodes.Count == 0) return null;
        if (Nodes.Count == 1) return Nodes[0];

        // find the node not being the source of any link for the specified tag
        return Nodes.Find(n => !Links.Any(l => l.Tag == tag && l.Source == n));
    }

    /// <summary>
    /// Gets the left link of the specified node.
    /// </summary>
    /// <param name="tag">The version tag.</param>
    /// <param name="node">The reference node or null.</param>
    /// <returns>Link or null if not found or input node was null.</returns>
    public ChainLink<T>? GetLeftLink(string tag, ChainNode<T>? node)
    {
        if (node == null) return null;
        return Links.Find(l => l.Tag == tag && l.Target == node);
    }

    /// <summary>
    /// Gets the left link of the specified link.
    /// </summary>
    /// <param name="tag">The version tag.</param>
    /// <param name="link">The reference link or null.</param>
    /// <returns>Link or null if not found or input node was null.</returns>
    public ChainLink<T>? GetLeftLink(string tag, ChainLink<T>? link)
    {
        if (link == null) return null;
        return Links.Find(l => l.Tag == tag && l.Target == link.Source);
    }

    /// <summary>
    /// Gets the right link of the specified node.
    /// </summary>
    /// <param name="tag">The version tag.</param>
    /// <param name="node">The reference node or null.</param>
    /// <returns>Link or null if not found or input node was null.</returns>
    public ChainLink<T>? GetRightLink(string tag, ChainNode<T>? node)
    {
        if (node == null) return null;
        return Links.Find(l => l.Tag == tag && l.Source == node);
    }

    /// <summary>
    /// Gets the right link of the specified link.
    /// </summary>
    /// <param name="tag">The version tag.</param>
    /// <param name="link">The reference link.</param>
    /// <returns>Link or null if not found or input node was null.</returns>
    public ChainLink<T>? GetRightLink(string tag, ChainLink<T>? link)
    {
        if (link == null) return null;
        return Links.Find(l => l.Tag == tag && l.Source == link.Target);
    }

    /// <summary>
    /// Deletes the specified node unless it's not connected.
    /// </summary>
    /// <param name="tag">The current version tag.</param>
    /// <param name="newTag">The new version tag.</param>
    /// <param name="node">The node to delete.</param>
    /// <returns>True if deleted.</returns>
    public bool Delete(string tag, string newTag, ChainNode<T> node)
    {
        ArgumentNullException.ThrowIfNull(tag);
        ArgumentNullException.ThrowIfNull(newTag);
        ArgumentNullException.ThrowIfNull(node);

        // delete Z:
        // [A] A:Z [Z] Z:B [B]
        // =>
        // [A] A:B* [B] (old: A:Z, Z:B)

        // the left link is A:Z (target=Z) and the right link is Z:B (source=Z)
        ChainLink<T>? left = Links.Find(l => l.Target == node);
        ChainLink<T>? right = Links.Find(l => l.Source == node);

        // nope if node is not connected to any other node
        if (left == null && right == null) return false;

        // [A] A:B [B]
        Links.Add(new ChainLink<T>(newTag, left?.Source, right?.Target));

        // if newTag is the same as tag, delete the old links
        if (tag == newTag)
        {
            if (left != null) Links.Remove(left);
            if (right != null) Links.Remove(right);
        }

        return true;
    }

    /// <summary>
    /// Deletes the specified range of nodes unless the reference nodes are
    /// not connected.
    /// </summary>
    /// <param name="tag">The current version tag.</param>
    /// <param name="newTag">The new version tag.</param>
    /// <param name="firstNode">The first node to delete.</param>
    /// <param name="lastNode">The last node to delete.</param>
    /// <returns>True if deleted.</returns>
    public bool DeleteRange(string tag, string newTag, ChainNode<T> firstNode,
        ChainNode<T> lastNode)
    {
        ArgumentNullException.ThrowIfNull(tag);
        ArgumentNullException.ThrowIfNull(newTag);
        ArgumentNullException.ThrowIfNull(firstNode);
        ArgumentNullException.ThrowIfNull(lastNode);

        // delete XY:
        // [A] A:X [X] X:Y [Y] Y:B [B]
        // =>
        // [A] A:B* [B] (old: A:X, X:Y, Y:B)
        ChainLink<T>? left = Links.Find(l => l.Target == firstNode);
        ChainLink<T>? right = Links.Find(l => l.Source == lastNode);

        // nope if nodes are not connected to any other node
        if (left == null || right == null) return false;

        // [A] A:B [B]
        Links.Add(new ChainLink<T>(newTag, left.Source, right.Target));

        // if newTag is the same as tag, delete the old links
        if (tag == newTag)
        {
            // remove all the links connecting nodes between left and right
            ChainLink<T>? l = right;
            while (l != null && l.Source != firstNode)
            {
                Links.Remove(l);
                l = GetLeftLink(tag, l);
            }
            Links.Remove(left);
        }

        return true;
    }

    /// <summary>
    /// Inserts <paramref name="newNode"/> before <paramref name="refNode"/>,
    /// unless the reference node is not connected.
    /// </summary>
    /// <param name="tag">The current version tag.</param>
    /// <param name="newTag">The new version tag.</param>
    /// <param name="refNode">The reference node.</param>
    /// <param name="newNode">The new node.</param>
    /// <returns>True if inserted.</returns>
    public bool InsertBefore(string tag, string newTag, ChainNode<T> refNode,
        ChainNode<T> newNode)
    {
        ArgumentNullException.ThrowIfNull(tag);
        ArgumentNullException.ThrowIfNull(newTag);
        ArgumentNullException.ThrowIfNull(refNode);
        ArgumentNullException.ThrowIfNull(newNode);

        // insert B before C:
        // [A] A:C [C] C:D [D]
        // =>
        // [A] A:B* [B] B:C* [C] C:D [D] (old: A:C)
        ChainLink<T>? link = Links.Find(l => l.Target == refNode);

        // nope if node is not connected to any other node
        if (link == null) return false;

        // [A] A:B* [B]
        if (link.Source != null)
            Links.Add(new ChainLink<T>(newTag, link.Source, newNode));

        // [B] B:C* [C]
        Links.Add(new ChainLink<T>(newTag, newNode, refNode));

        // if newTag is the same as tag, delete the old link
        if (tag == newTag) Links.Remove(link);

        return true;
    }

    /// <summary>
    /// Inserts <paramref name="newNode"/> after <paramref name="refNode"/>,
    /// unless the reference node is not connected.
    /// </summary>
    /// <param name="tag">The tag.</param>
    /// <param name="newTag">The new tag.</param>
    /// <param name="refNode">The reference node.</param>
    /// <param name="newNode">The new node.</param>
    /// <returns>True if inserted.</returns>
    public bool InsertAfter(string tag, string newTag, ChainNode<T> refNode,
        ChainNode<T> newNode)
    {
        ArgumentNullException.ThrowIfNull(tag);
        ArgumentNullException.ThrowIfNull(newTag);
        ArgumentNullException.ThrowIfNull(refNode);
        ArgumentNullException.ThrowIfNull(newNode);

        // insert B after A:
        // [A] A:C [C] C:D [D]
        // =>
        // [A] A:B* [B] B:C* [C] C:D [D] (old: A:C)
        ChainLink<T>? link = Links.Find(l => l.Source == refNode);

        // nope if node is not connected to any other node
        if (link == null) return false;

        // [A] A:B* [B]
        Links.Add(new ChainLink<T>(newTag, refNode, newNode));

        // [B] B:C* [C]
        if (link.Target != null)
            Links.Add(new ChainLink<T>(newTag, newNode, link.Target));

        // if newTag is the same as tag, delete the old link
        if (tag == newTag) Links.Remove(link);

        return true;
    }

    /// <summary>
    /// Appends <paramref name="newNode"/> after the last node.
    /// </summary>
    /// <param name="tag">The current version tag.</param>
    /// <param name="newTag">The new version tag.</param>
    /// <param name="newNode">The new node.</param>
    public void Append(string tag, string newTag, ChainNode<T> newNode)
    {
        ArgumentNullException.ThrowIfNull(tag);
        ArgumentNullException.ThrowIfNull(newTag);
        ArgumentNullException.ThrowIfNull(newNode);

        // append D:
        // [A] A:B [B] B:C [C]
        // =>
        // [A] A:B [B] B:C [C] C:D* [D]

        // append after last node
        ChainNode<T>? last = GetLastNode(tag);
        Links.Add(new ChainLink<T>(newTag, last, newNode));

        Nodes.Add(newNode);
    }

    /// <summary>
    /// Converts to string.
    /// </summary>
    /// <returns>string</returns>
    public override string ToString()
    {
        return $"N{Nodes.Count}";
    }
}
