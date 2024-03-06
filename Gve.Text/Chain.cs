using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

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
    private readonly object _locker = new();
    private int _maxId;

    /// <summary>
    /// Gets or sets the nodes in this chain.
    /// </summary>
    protected List<ChainNode<T>> _nodes = [];

    /// <summary>
    /// Gets or sets the links connecting nodes in this chain.
    /// </summary>
    protected List<ChainLink<T>> _links = [];

    /// <summary>
    /// Gets the nodes in this chain.
    /// </summary>
    public IReadOnlyList<ChainNode<T>> Nodes => _nodes;

    /// <summary>
    /// Gets the links in this chain.
    /// </summary>
    public IReadOnlyList<ChainLink<T>> Links => _links;

    #region ID
    /// <summary>
    /// Resets the identifier to the specified seed value. This means that the
    /// next node added to this chain will have an identifier equal to the seed
    /// plus one.
    /// </summary>
    /// <param name="seed">The seed.</param>
    public void ResetId(int seed)
    {
        lock (_locker)
        {
            _maxId = seed;
        }
    }

    /// <summary>
    /// Gets the maximum ordinal among the nodes of all the versions in this
    /// chain.
    /// </summary>
    /// <returns>Max ordinal number.</returns>
    public int GetMaxOrdinal()
    {
        lock (_locker)
        {
            return _nodes.Count == 0 ? 0 : _nodes.Max(n => n.Id);
        }
    }

    /// <summary>
    /// Gets the next available node identifier in this chain.
    /// </summary>
    /// <returns>ID.</returns>
    public int GetNextId()
    {
        lock (_locker)
        {
            return Interlocked.Increment(ref _maxId);
        }
    }
    #endregion

    #region Navigation
    /// <summary>
    /// Gets the node count.
    /// </summary>
    /// <param name="tag">The optional tag filter.</param>
    /// <returns>Count.</returns>
    public int GetNodeCount(string? tag = null)
    {
        if (_nodes.Count == 1) return _nodes[0].SourceTag == tag? 1 : 0;

        return tag == null ? _nodes.Count : _links.Count(l => l.Tag == tag) + 1;
    }

    /// <summary>
    /// Gets the link count.
    /// </summary>
    /// <param name="tag">The tag filter.</param>
    /// <returns>Count.</returns>
    public int GetLinkCount(string? tag = null)
    {
        return tag == null ? _links.Count : _links.Count(l => l.Tag == tag);
    }

    /// <summary>
    /// Gets the a list of unique tags from this chain.
    /// </summary>
    /// <returns>List, sorted alphabetically.</returns>
    public IList<string> GetDistinctTags()
    {
        HashSet<string> tags = new(_nodes.Select(n => n.SourceTag));
        foreach (ChainLink<T> link in _links) tags.Add(link.Tag);
        return [.. tags.OrderBy(t => t)];
    }

    /// <summary>
    /// Finds the node with the specified ID.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <returns>Node or null if not found.</returns>
    public ChainNode<T>? FindNode(int id) => _nodes.Find(n => n.Id == id);

    /// <summary>
    /// Determines whether <paramref name="node"/> is included in the version
    /// defined by <paramref name="tag"/>.
    /// </summary>
    /// <param name="node">The node.</param>
    /// <param name="tag">The tag.</param>
    /// <returns><c>true</c> if in version; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">node or tag</exception>
    public bool IsNodeInVersion(ChainNode<T> node, string tag)
    {
        ArgumentNullException.ThrowIfNull(node);
        ArgumentNullException.ThrowIfNull(tag);

        return _links.Exists(l => l.Tag == tag &&
                             (l.Source == node || l.Target == node));
    }

    /// <summary>
    /// Gets the first node for the specified version tag.
    /// </summary>
    /// <param name="tag">The version tag.</param>
    /// <returns>Node or null.</returns>
    /// <exception cref="ArgumentNullException">tag</exception>
    public ChainNode<T>? GetFirstNode(string tag)
    {
        ArgumentNullException.ThrowIfNull(tag);

        if (_nodes.Count == 0) return null;
        if (_nodes.Count == 1)
            return _nodes[0].SourceTag == tag ? _nodes[0] : null;

        if (!HasVersion(tag)) return null;

        // find the node not being the target of any link for the specified tag
        return _nodes.Find(n => IsNodeInVersion(n, tag) &&
            !_links.Exists(l => l.Tag == tag && l.Target == n));
    }

    /// <summary>
    /// Gets the last node for the specified version tag.
    /// </summary>
    /// <param name="tag">The tag.</param>
    /// <returns>Node or null.</returns>
    /// <exception cref="ArgumentNullException">tag</exception>
    public ChainNode<T>? GetLastNode(string tag)
    {
        ArgumentNullException.ThrowIfNull(tag);

        if (_nodes.Count == 0) return null;
        if (_nodes.Count == 1)
            return _nodes[0].SourceTag == tag ? _nodes[0] : null;

        if (!HasVersion(tag)) return null;

        // find the node not being the source of any link for the specified tag
        return _nodes.Find(n => IsNodeInVersion(n, tag) &&
            !_links.Exists(l => l.Tag == tag && l.Source == n));
    }

    /// <summary>
    /// Gets the next node from <paramref name="node"/>.
    /// </summary>
    /// <param name="tag">The version tag.</param>
    /// <param name="node">The node.</param>
    /// <returns>Node or null.</returns>
    public ChainNode<T>? GetNextNode(string tag, ChainNode<T> node)
    {
        ArgumentNullException.ThrowIfNull(tag);
        ArgumentNullException.ThrowIfNull(node);

        return _links.Find(l => l.Tag == tag && l.Source == node)?.Target;
    }

    /// <summary>
    /// Gets the previous node from <paramref name="node"/>.
    /// </summary>
    /// <param name="tag">The version tag.</param>
    /// <param name="node">The node.</param>
    /// <returns>Node or null.</returns>
    public ChainNode<T>? GetPreviousNode(string tag, ChainNode<T> node)
    {
        ArgumentNullException.ThrowIfNull(tag);
        ArgumentNullException.ThrowIfNull(node);

        return _links.Find(l => l.Tag == tag && l.Target == node)?.Source;
    }

    /// <summary>
    /// Determines whether <paramref name="node"/> is at any distance before
    /// <paramref name="refNode"/> in the version defined by
    /// <paramref name="tag"/>.
    /// </summary>
    /// <param name="tag">The versopm tag.</param>
    /// <param name="node">The node.</param>
    /// <param name="refNode">The reference node.</param>
    /// <returns><c>true</c> if before; otherwise, <c>false</c>.</returns>
    public bool IsNodeBefore(string tag, ChainNode<T> node, ChainNode<T> refNode)
    {
        ArgumentNullException.ThrowIfNull(tag);
        ArgumentNullException.ThrowIfNull(node);
        ArgumentNullException.ThrowIfNull(refNode);

        ChainNode<T>? n = node;
        while (n != null)
        {
            if (n == refNode) return true;
            n = GetNextNode(tag, n);
        }
        return false;
    }

    /// <summary>
    /// Determines whether <paramref name="node"/> is at any distance after
    /// <paramref name="refNode"/> in the version defined by
    /// <paramref name="tag"/>.
    /// </summary>
    /// <param name="tag">The version tag.</param>
    /// <param name="node">The node.</param>
    /// <param name="refNode">The other node.</param>
    /// <returns><c>true</c> if after; otherwise, <c>false</c>.</returns>
    public bool IsNodeAfter(string tag, ChainNode<T> node, ChainNode<T> refNode)
    {
        ArgumentNullException.ThrowIfNull(tag);
        ArgumentNullException.ThrowIfNull(node);
        ArgumentNullException.ThrowIfNull(refNode);

        ChainNode<T>? n = node;
        while (n != null)
        {
            if (n == refNode) return true;
            n = GetPreviousNode(tag, n);
        }
        return false;
    }

    /// <summary>
    /// Gets the left link of the specified node.
    /// </summary>
    /// <param name="tag">The version tag.</param>
    /// <param name="node">The reference node or null.</param>
    /// <returns>Link or null if not found or input node was null.</returns>
    /// <exception cref="ArgumentNullException">tag or node</exception>
    public ChainLink<T>? GetNodeLeftLink(string tag, ChainNode<T>? node)
    {
        if (node == null || !HasVersion(tag)) return null;

        return _links.Find(l => l.Tag == tag && l.Target == node);
    }

    /// <summary>
    /// Gets the left link of the specified link.
    /// </summary>
    /// <param name="tag">The version tag.</param>
    /// <param name="link">The reference link or null.</param>
    /// <returns>Link or null if not found or input node was null.</returns>
    /// <exception cref="ArgumentNullException">tag</exception>
    public ChainLink<T>? GetLeftLink(string tag, ChainLink<T>? link)
    {
        ArgumentNullException.ThrowIfNull(tag);

        if (link == null || !HasVersion(tag)) return null;

        return _links.Find(l => l.Tag == tag && l.Target == link.Source);
    }

    /// <summary>
    /// Gets the right link of the specified node.
    /// </summary>
    /// <param name="tag">The version tag.</param>
    /// <param name="node">The reference node or null.</param>
    /// <returns>Link or null if not found or input node was null.</returns>
    /// <exception cref="ArgumentNullException">tag</exception>
    public ChainLink<T>? GetNodeRightLink(string tag, ChainNode<T>? node)
    {
        ArgumentNullException.ThrowIfNull(tag);

        if (node == null || !HasVersion(tag)) return null;

        return _links.Find(l => l.Tag == tag && l.Source == node);
    }

    /// <summary>
    /// Gets the right link of the specified link.
    /// </summary>
    /// <param name="tag">The version tag.</param>
    /// <param name="link">The reference link.</param>
    /// <returns>Link or null if not found or input node was null.</returns>
    /// <exception cref="ArgumentNullException">tag</exception>
    public ChainLink<T>? GetRightLink(string tag, ChainLink<T>? link)
    {
        ArgumentNullException.ThrowIfNull(tag);

        if (link == null || !HasVersion(tag)) return null;

        return _links.Find(l => l.Tag == tag && l.Source == link.Target);
    }

    /// <summary>
    /// Gets the nodes for the specified version tag, in their order.
    /// </summary>
    /// <param name="tag">The version tag.</param>
    /// <returns>The nodes.</returns>
    /// <exception cref="ArgumentNullException">tag</exception>
    public IList<ChainNode<T>> GetTagNodes(string tag)
    {
        ArgumentNullException.ThrowIfNull(tag);

        if (_nodes.Count == 0) return [];
        if (_nodes.Count == 1) return _nodes[0].SourceTag == tag ? _nodes : [];

        if (!_links.Exists(l => l.Tag == tag))
        {
            // if there only one single node with source tag equal to tag, return it
            List<ChainNode<T>> tagNodes = _nodes.Where(n => n.SourceTag == tag)
                .ToList();
            return tagNodes.Count == 1 ? tagNodes : (IList<ChainNode<T>>)([]);
        }

        ChainNode<T>? first = GetFirstNode(tag);
        if (first == null) return [];

        List<ChainNode<T>> nodes = [first];
        ChainLink<T>? link = GetNodeRightLink(tag, first);
        while (link?.Target != null)
        {
            nodes.Add(link.Target);
            link = GetRightLink(tag, link);
        }
        return nodes;
    }

    /// <summary>
    /// Enumerates the nodes for the specified version tag, in their order.
    /// </summary>
    /// <param name="tag">The version tag.</param>
    /// <returns>The nodes.</returns>
    /// <exception cref="ArgumentNullException">tag</exception>
    public IEnumerable<ChainNode<T>> EnumerateTagNodes(string tag)
    {
        ArgumentNullException.ThrowIfNull(tag);

        return InnerEnumerate();

        IEnumerable<ChainNode<T>> InnerEnumerate()
        {
            if (_nodes.Count == 1)
            {
                if (_nodes[0].SourceTag == tag) yield return _nodes[0];
                yield break;
            }

            ChainNode<T>? first = GetFirstNode(tag);
            if (first == null) yield break;

            yield return first;
            ChainLink<T>? link = GetNodeRightLink(tag, first);
            while (link?.Target != null)
            {
                yield return link.Target;
                link = GetRightLink(tag, link);
            }
        }
    }
    #endregion

    #region Delete
    /// <summary>
    /// Deletes the specified node unless it's not connected.
    /// </summary>
    /// <remarks>Deleting a node implies adding new links which short-circuit
    /// the connection between its left and right nodes. If current and new
    /// tags are equal, the old links will be removed.</remarks>
    /// <param name="tag">The current version tag.</param>
    /// <param name="newTag">The new version tag.</param>
    /// <param name="node">The node to delete.</param>
    /// <returns>True if deleted.</returns>
    /// <exception cref="ArgumentNullException">tag, newTag, node</exception>
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
        ChainLink<T>? left = _links.Find(l => l.Tag == tag && l.Target == node);
        ChainLink<T>? right = _links.Find(l => l.Tag == tag && l.Source == node);

        // nope if node is not connected to any other node
        if (left == null && right == null) return false;

        // first node: [A] A:B [B]
        if (left == null)
        {
            AddVersion(tag, newTag, right!);
            return true;
        }

        // last node: B in [A] A:B [B]
        if (right == null)
        {
            AddVersion(tag, newTag, left!);
            return true;
        }

        _links.Add(new ChainLink<T>(newTag, left.Source, right.Target));

        // if newTag is the same as tag, delete the old links
        if (tag == newTag)
        {
            _links.Remove(left);
            _links.Remove(right);
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
    /// <exception cref="ArgumentNullException">tag, newTag, firstNode, lastNode
    /// </exception>
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
        ChainLink<T>? left = _links.Find(
            l => l.Tag == tag && l.Target == firstNode);
        ChainLink<T>? right = _links.Find(
            l => l.Tag == tag && l.Source == lastNode);

        // nope if nodes are not connected to any other node
        if (left == null || right == null) return false;

        // [A] A:B [B]
        _links.Add(new ChainLink<T>(newTag, left.Source, right.Target));

        // if newTag is the same as tag, delete the old links
        if (tag == newTag)
        {
            // remove all the links connecting nodes between left and right
            ChainLink<T>? l = right;
            while (l != null && l.Source != firstNode)
            {
                _links.Remove(l);
                l = GetLeftLink(tag, l);
            }
            _links.Remove(left);
        }

        return true;
    }
    #endregion

    #region Insert
    /// <summary>
    /// Supplies the identifier and the source tag to the specified node
    /// where not present.
    /// </summary>
    /// <param name="node">The node.</param>
    /// <param name="sourceTag">The source tag.</param>
    /// <exception cref="ArgumentNullException">node or sourceTag</exception>
    protected void SupplyNodeMetadata(ChainNode<T> node, string sourceTag)
    {
        ArgumentNullException.ThrowIfNull(node);
        ArgumentNullException.ThrowIfNull(sourceTag);

        if (node.Id == 0) node.Id = GetNextId();
        if (node.SourceTag.Length == 0) node.SourceTag = sourceTag;
    }

    /// <summary>
    /// Supply the identifiers and the source tag to the specified nodes
    /// where not present.
    /// </summary>
    /// <param name="nodes">The nodes.</param>
    /// <param name="sourceTag">The source tag.</param>
    /// <exception cref="ArgumentNullException">node or sourceTag</exception>
    protected void SupplyNodeMetadata(IList<ChainNode<T>> nodes, string sourceTag)
    {
        ArgumentNullException.ThrowIfNull(nodes);
        ArgumentNullException.ThrowIfNull(sourceTag);

        foreach (ChainNode<T> node in nodes.Where(n => n.Id == 0))
        {
            node.Id = GetNextId();
            if (node.SourceTag.Length == 0) node.SourceTag = sourceTag;
        }
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
    /// <exception cref="ArgumentNullException">tag, newTag, refNode, newNodes
    /// </exception>
    public bool InsertBefore(string tag, string newTag, ChainNode<T> refNode,
        ChainNode<T> newNode)
    {
        ArgumentNullException.ThrowIfNull(tag);
        ArgumentNullException.ThrowIfNull(newTag);
        ArgumentNullException.ThrowIfNull(refNode);
        ArgumentNullException.ThrowIfNull(newNode);

        // insert B before C:
        // [A] A:C ([C]) C:D [D]
        // =>
        // [A] (A:B* [B]* B:C*) [C] C:D [D] (old: A:C)

        // get the link connecting the reference node to the previous node (A:C)
        ChainLink<T>? link = GetNodeLeftLink(tag, refNode);

        // nope if node is not connected to any other node
        if (link == null) return false;

        // supply ID to newNode if it does not have one
        SupplyNodeMetadata(newNode, newTag);

        // copy the source state into a new one excluding the link to be removed
        AddVersion(tag, newTag, link);

        // [A] A:B* [B]
        if (link.Source != null)
            _links.Add(new ChainLink<T>(newTag, link.Source, newNode));

        // [B] B:C* [C]
        _links.Add(new ChainLink<T>(newTag, newNode, refNode));

        // if newTag is the same as tag, delete the old link
        if (tag == newTag) _links.Remove(link);

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
    /// <exception cref="ArgumentNullException">tag, newTag, refNode, newNodes
    /// </exception>
    public bool InsertAfter(string tag, string newTag, ChainNode<T> refNode,
        ChainNode<T> newNode)
    {
        ArgumentNullException.ThrowIfNull(tag);
        ArgumentNullException.ThrowIfNull(newTag);
        ArgumentNullException.ThrowIfNull(refNode);
        ArgumentNullException.ThrowIfNull(newNode);

        // insert B after A:
        // ([A]) A:C [C] C:D [D]
        // =>
        // [A] (A:B* [B]* B:C*) [C] C:D [D] (old: A:C)

        // get the link connecting the reference node to the next node (A:C)
        ChainLink<T>? link = GetNodeRightLink(tag, refNode);

        // nope if node is not connected to any other node
        if (link == null) return false;

        // supply ID to newNode if it does not have one
        SupplyNodeMetadata(newNode, newTag);

        // copy the source state into a new one excluding the link to be removed
        AddVersion(tag, newTag, link);

        // [A] A:B* [B]
        _links.Add(new ChainLink<T>(newTag, refNode, newNode));

        // [B] B:C* [C]
        if (link.Target != null)
            _links.Add(new ChainLink<T>(newTag, newNode, link.Target));

        // if newTag is the same as tag, delete the old link
        if (tag == newTag) _links.Remove(link);

        return true;
    }

    /// <summary>
    /// Inserts the specified range of nodes before <paramref name="refNode"/>.
    /// </summary>
    /// <param name="tag">The current version tag.</param>
    /// <param name="newTag">The new version tag.</param>
    /// <param name="refNode">The reference node.</param>
    /// <param name="newNodes">The new nodes.</param>
    /// <returns>True if inserted.</returns>
    /// <exception cref="ArgumentNullException">tag, newTag, refNode, newNodes
    /// </exception>
    public bool InsertRangeBefore(string tag, string newTag, ChainNode<T> refNode,
        IList<ChainNode<T>> newNodes)
    {
        ArgumentNullException.ThrowIfNull(tag);
        ArgumentNullException.ThrowIfNull(newTag);
        ArgumentNullException.ThrowIfNull(refNode);
        ArgumentNullException.ThrowIfNull(newNodes);

        // insert [B] B:C [C] before [D]:
        // [A] A:D ([D]) D:E [E]
        // =>
        // [A] (A:B* [B]* B:C* [C]* C:D*) [D] D:E [E] (old: A:D)

        // get the link connecting the reference node to the previous node
        ChainLink<T>? link = GetNodeLeftLink(tag, refNode);
        if (link == null) return false;

        // supply IDs to newNodes if they do not have one
        SupplyNodeMetadata(newNodes, newTag);

        // copy the source state into a new one excluding the link to be removed
        AddVersion(tag, newTag, link);

        // add links between new nodes
        _links.AddRange(BuildLinksFrom(newTag, newNodes));
        // add nodes
        _nodes.AddRange(newNodes);

        // add A:B
        _links.Add(new ChainLink<T>(newTag, link.Source, newNodes[0]));
        // add C:D
        _links.Add(new ChainLink<T>(newTag, newNodes[^1], refNode));

        // if newTag is the same as tag, delete the old link
        if (tag == newTag) _links.Remove(link);

        return true;
    }

    /// <summary>
    /// Inserts the specified range of nodes after <paramref name="refNode"/>.
    /// </summary>
    /// <param name="tag">The current version tag.</param>
    /// <param name="newTag">The new version tag.</param>
    /// <param name="refNode">The reference node.</param>
    /// <param name="newNodes">The new nodes.</param>
    /// <returns>True if inserted.</returns>
    /// <exception cref="ArgumentNullException">tag, newTag, refNode, newNodes
    /// </exception>
    public bool InsertRangeAfter(string tag, string newTag, ChainNode<T> refNode,
        IList<ChainNode<T>> newNodes)
    {
        ArgumentNullException.ThrowIfNull(tag);
        ArgumentNullException.ThrowIfNull(newTag);
        ArgumentNullException.ThrowIfNull(refNode);
        ArgumentNullException.ThrowIfNull(newNodes);

        // insert [B] B:C [C] after [A]:
        // ([A]) A:D [D] D:E [E]
        // =>
        // [A] (A:B* [B]* B:C* [C]* C:D*) [D] D:E [E] (old: A:D)

        ChainLink<T>? link = GetNodeRightLink(tag, refNode);
        if (link == null) return false;

        // supply IDs to newNodes if they do not have one
        SupplyNodeMetadata(newNodes, newTag);

        // copy the source state into a new one excluding the link to be removed
        AddVersion(tag, newTag, link);

        // add links between new nodes
        _links.AddRange(BuildLinksFrom(newTag, newNodes));
        // add nodes
        _nodes.AddRange(newNodes);

        // add B:C
        _links.Add(new ChainLink<T>(newTag, refNode, newNodes[0]));
        // add C:D
        _links.Add(new ChainLink<T>(newTag, newNodes[^1], link.Target));

        // if newTag is the same as tag, delete the old link
        if (tag == newTag) _links.Remove(link);

        return true;
    }
    #endregion

    #region Append
    /// <summary>
    /// Appends <paramref name="newNode"/> at the end of this chain.
    /// </summary>
    /// <param name="tag">The current version tag.</param>
    /// <param name="newTag">The new version tag.</param>
    /// <param name="newNode">The new node.</param>
    /// <exception cref="ArgumentNullException">tag, newTag, newNode</exception>
    public void Append(string tag, string newTag, ChainNode<T> newNode)
    {
        ArgumentNullException.ThrowIfNull(tag);
        ArgumentNullException.ThrowIfNull(newTag);
        ArgumentNullException.ThrowIfNull(newNode);

        // supply ID to newNode if it does not have one
        SupplyNodeMetadata(newNode, newTag);

        // corner case: if adding to an empty version, just add the node
        if (_nodes.Count == 0)
        {
            _nodes.Add(newNode);
            return;
        }

        // copy the source state into a new one
        AddVersion(tag, newTag);

        // append D:
        // [A] A:B [B] B:C [C]
        // =>
        // [A] A:B [B] B:C [C] C:D* [D]

        // append after last node
        ChainNode<T>? last = GetLastNode(tag);
        if (last != null) _links.Add(new ChainLink<T>(newTag, last, newNode));

        _nodes.Add(newNode);
    }

    /// <summary>
    /// Appends <paramref name="nodes"/> at the end of this chain.
    /// </summary>
    /// <param name="tag">The current version tag.</param>
    /// <param name="newTag">The new version tag.</param>
    /// <param name="nodes">The new nodes.</param>
    public void AppendRange(string tag, string newTag, IList<ChainNode<T>> nodes)
    {
        ArgumentNullException.ThrowIfNull(tag);
        ArgumentNullException.ThrowIfNull(newTag);
        ArgumentNullException.ThrowIfNull(nodes);

        if (nodes.Count == 0) return;

        // copy the source state into a new one
        AddVersion(tag, newTag);

        // supply IDs to newNodes if they do not have one
        SupplyNodeMetadata(nodes, newTag);

        if (_nodes.Count > 0)
        {
            // append after last node
            ChainNode<T>? last = GetLastNode(tag);
            if (last != null) _links.Add(new ChainLink<T>(newTag, last, nodes[0]));
        }

        _links.AddRange(BuildLinksFrom(newTag, nodes));
        _nodes.AddRange(nodes);
    }
    #endregion

    #region Swap
    /// <summary>
    /// Swaps the specified ranges of nodes.
    /// </summary>
    /// <param name="tag">The version tag.</param>
    /// <param name="newTag">The new version tag.</param>
    /// <param name="first1Node">The first node of range 1.</param>
    /// <param name="last1Node">The last node of range 1.</param>
    /// <param name="first2Node">The first node of range 2.</param>
    /// <param name="last2Node">The last node of range 2.</param>
    /// <returns>
    /// True if swapped.
    /// </returns>
    /// <exception cref="InvalidOperationException">The two sequences to
    /// swap overlap.</exception>
    /// <exception cref="ArgumentNullException">tag or newTag or first1Node
    /// or last1Node or first2Node or last2Node</exception>
    public bool SwapRanges(string tag, string newTag,
        ChainNode<T> first1Node, ChainNode<T> last1Node,
        ChainNode<T> first2Node, ChainNode<T> last2Node)
    {
        ArgumentNullException.ThrowIfNull(tag);
        ArgumentNullException.ThrowIfNull(newTag);
        ArgumentNullException.ThrowIfNull(first1Node);
        ArgumentNullException.ThrowIfNull(first2Node);
        ArgumentNullException.ThrowIfNull(last1Node);
        ArgumentNullException.ThrowIfNull(last2Node);

        // nope the two sequences are equal
        if (first1Node == first2Node && last1Node == last2Node) return false;

        // error if the two sequences overlap
        if (IsNodeAfter(tag, last1Node, first2Node) ||
            IsNodeAfter(tag, last2Node, first1Node))
        {
            throw new InvalidOperationException("The two sequences to swap overlap.");
        }

        // if sequence 2 is before sequence 1, swap them
        if (IsNodeAfter(tag, last1Node, first2Node))
        {
            (first1Node, last1Node, first2Node, last2Node) =
                (first2Node, last2Node, first1Node, last1Node);
        }

        // get left links of head nodes
        ChainLink<T>? first1Link = GetNodeLeftLink(tag, first1Node);
        ChainLink<T>? first2Link = GetNodeLeftLink(tag, first2Node);

        // nope if head nodes are not connected to any other node
        if (first1Link == null && first2Link == null) return false;

        // copy into new tag except the head links
        AddVersion(tag, newTag, first1Link, first2Link);

        // are 1 and 2 adjacent (left of first2Node = last of 1)?
        if (first2Link?.Source == last1Node)
        {
            // add link between left-of-1-head and 2-head
            if (first1Link != null)
                _links.Add(new ChainLink<T>(newTag, first1Link.Source, first2Node));

            // add link between 2-tail and 1-head
            _links.Add(new ChainLink<T>(newTag, last2Node, first1Node));
        }
        // else non-adjacent swap
        else
        {
            // add link between left-of-1-head and left-of-2-head
            if (first1Link != null && first2Link != null)
                _links.Add(new ChainLink<T>(newTag, first1Link.Source, first2Node));

            // add link between left-of-b-head and a-head
            _links.Add(new ChainLink<T>(newTag, first2Link?.Source, first1Node));
        }

        return true;
    }
    #endregion

    #region Version
    /// <summary>
    /// Adds the new version <paramref name="newTag"/> to the chain copying it
    /// from the version with the specified <paramref name="tag"/>.
    /// </summary>
    /// <param name="tag">The source tag.</param>
    /// <param name="newTag">The new tag.</param>
    /// <param name="excludedLinks">The optional links to be excluded from the
    /// copy.</param>
    /// <exception cref="ArgumentNullException">tag or newTag</exception>
    public void AddVersion(string tag, string newTag,
        params ChainLink<T>?[] excludedLinks)
    {
        ArgumentNullException.ThrowIfNull(tag);
        ArgumentNullException.ThrowIfNull(newTag);

        // clone all the links of the specified tag into newTag
        List<ChainLink<T>> newLinks = _links
            .Where(l => l.Tag == tag && !excludedLinks.Contains(l))
            .Select(l => new ChainLink<T>(newTag, l.Source, l.Target))
            .ToList();

        // add newLinks unless there is a link with the same tag and nodes
        foreach (ChainLink<T> link in newLinks)
        {
            if (!_links.Exists(l => l.Tag == newTag &&
                                    l.Source == link.Source &&
                                    l.Target == link.Target))
            {
                _links.Add(link);
            }
        }
    }

    /// <summary>
    /// Removes the version with the specified <paramref name="tag"/>.
    /// </summary>
    /// <param name="tag">The version tag.</param>
    public void RemoveVersion(string tag)
    {
        _links.RemoveAll(l => l.Tag == tag);
    }

    /// <summary>
    /// Determines whether this chain contains the specified version.
    /// </summary>
    /// <param name="tag">The version tag.</param>
    /// <returns>
    ///   <c>true</c> if version is present; otherwise, <c>false</c>.
    /// </returns>
    public bool HasVersion(string tag)
    {
        return _links.Exists(l => l.Tag == tag);
    }
    #endregion

    /// <summary>
    /// Clears this chain removing all the versions.
    /// </summary>
    public void Clear()
    {
        _nodes.Clear();
        _links.Clear();
    }

    /// <summary>
    /// Builds the links which connect all the received nodes in their order.
    /// </summary>
    /// <param name="newTag">The new version tag to assign to the created links.
    /// </param>
    /// <param name="nodes">The nodes.</param>
    /// <returns>The links connecting all the nodes.</returns>
    /// <exception cref="ArgumentNullException">newTag or nodes</exception>
    public static IList<ChainLink<T>> BuildLinksFrom(string newTag,
        IList<ChainNode<T>> nodes)
    {
        ArgumentNullException.ThrowIfNull(newTag);
        ArgumentNullException.ThrowIfNull(nodes);

        if (nodes.Count < 2) return [];

        List<ChainLink<T>> links = [];
        for (int i = 0; i < nodes.Count - 1; i++)
        {
            links.Add(new ChainLink<T>(newTag, nodes[i], nodes[i + 1]));
        }
        return links;
    }

    /// <summary>
    /// Converts to string.
    /// </summary>
    /// <returns>string</returns>
    public override string ToString()
    {
        return $"N={_nodes.Count} L={_links.Count} " +
            $"V: {string.Join(", ", GetDistinctTags())}";
    }
}
