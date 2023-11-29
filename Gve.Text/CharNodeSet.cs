using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Gve.Text;

/// <summary>
/// A set of <see cref="CharNode"/>'s. In this set, each node represents the
/// character of an original text. The nodes are linked to each other by their
/// <see cref="CharNode.LeftNode"/> property, which points to the node at the
/// left of the current node. Nodes are all siblings, and the first node in the
/// set has no left node.
/// </summary>
/// <remarks>This set allows operations on its nodes based on the ordinal numbers
/// of the characters in the original text.</remarks>
public class CharNodeSet : IEnumerable<CharNode>
{
    private readonly object _locker = new();
    private int _maxId;
    private readonly List<CharNode> _nodes;

    /// <summary>
    /// Initializes a new instance of the <see cref="CharNodeSet"/> class.
    /// </summary>
    /// <param name="text">The original text.</param>
    public CharNodeSet(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        _nodes = [];
        SetText(text);
    }

    /// <summary>
    /// Gets the first node in this set.
    /// </summary>
    public CharNode? GetFirstNode() => _nodes.Find(n => n.LeftNode == null);

    /// <summary>
    /// Gets the last node in this set.
    /// </summary>
    public CharNode? GetLastNode()
    {
        // collect the ID of all the left nodes in set
        HashSet<int> leftIds = [];
        foreach (CharNode? node in _nodes.Select(n => n.LeftNode))
        {
            if (node != null) leftIds.Add(node.Id);
        }

        // find the node whose id is not among them
        return _nodes.Find(n => !leftIds.Contains(n.Id));
    }

    private int GetNextId()
    {
        lock (_locker)
        {
            return Interlocked.Increment(ref _maxId);
        }
    }

    /// <summary>
    /// Gets the count of the nodes in this set.
    /// </summary>
    /// <param name="deleted">The deleted filter.</param>
    /// <returns>Count.</returns>
    public int GetCount(bool? deleted = null)
    {
        return deleted == null
            ? _nodes.Count
            : _nodes.Count(n => n.IsDeleted == deleted);
    }

    /// <summary>
    /// Sets the original text for this set.
    /// </summary>
    /// <param name="text">The original text.</param>
    public void SetText(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        _nodes.Clear();
        for (int i = 0; i < text.Length; i++)
        {
            _nodes.Add(new CharNode(GetNextId(), i + 1)
            {
                Value = text[i],
                LeftNode = i > 0 ? _nodes[i - 1] : null
            });
        }
    }

    /// <summary>
    /// Gets the enumerator.
    /// </summary>
    /// <returns>Enumerator.</returns>
    public IEnumerator<CharNode> GetEnumerator()
    {
        // find the first node (the one without a left sibling)
        CharNode? firstNode = _nodes.Find(node => node.LeftNode == null);

        if (firstNode != null)
        {
            // yield the first node
            yield return firstNode;

            // find and yield the remaining nodes
            CharNode? currentNode = firstNode;
            while ((currentNode = _nodes.Find(
                node => node.LeftNode == currentNode)) != null)
            {
                yield return currentNode;
            }
        }
    }

    /// <summary>
    /// Gets the enumerator.
    /// </summary>
    /// <returns>Enumerator.</returns>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    /// Gets the text from the characters in this set.
    /// </summary>
    /// <returns>String.</returns>
    public string GetText() => string.Join("",
        this.Where(n => !n.IsDeleted).Select(n => n.Value));

    /// <summary>
    /// Gets one new node for each character in <paramref name="text"/>.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <returns>Nodes.</returns>
    public IList<CharNode> GetNodesFor(string text)
    {
        List<CharNode> nodes = [];

        for (int i = 0; i < text.Length; i++)
        {
            nodes.Add(new CharNode(GetNextId(), 0)
            {
                Value = text[i],
                LeftNode = i > 0 ? nodes[i - 1] : null
            });
        }

        return nodes;
    }

    /// <summary>
    /// Gets the node at the specified ordinal.
    /// </summary>
    /// <param name="at">The ordinal.</param>
    /// <returns>Node or null if not found.</returns>
    /// <exception cref="ArgumentOutOfRangeException">at less than 1</exception>
    public CharNode? GetNode(int at)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(at, 1);
        return _nodes.Find(n => n.Ordinal == at);
    }

    /// <summary>
    /// Gets the node at the left of the specified node.
    /// </summary>
    /// <param name="node">The node.</param>
    /// <param name="deleted">The deleted filter.</param>
    /// <returns>Node or null.</returns>
    public CharNode? GetLeftNodeOf(CharNode? node, bool? deleted = null)
    {
        if (node == null) return null;
        CharNode? left = node.LeftNode;
        if (deleted == null) return left;

        while (left != null && left.IsDeleted != deleted)
        {
            left = left.LeftNode;
        }
        return left;
    }

    /// <summary>
    /// Gets the node at the right of the specified node.
    /// </summary>
    /// <param name="node">The node.</param>
    /// <param name="deleted">The deleted filter.</param>
    /// <returns>Node or null.</returns>
    public CharNode? GetRightNodeOf(CharNode? node, bool? deleted = null)
    {
        if (node == null) return null;
        CharNode? right = _nodes.Find(n => n.LeftNode == node);
        if (deleted == null) return right;

        while (right != null && right.IsDeleted != deleted)
        {
            right = _nodes.Find(n => n.LeftNode == right);
        }
        return right;
    }

    /// <summary>
    /// Deletes the nodes starting at the specified ordinal for the specified
    /// run of consecutive nodes.
    /// </summary>
    /// <param name="at">The ordinal.</param>
    /// <param name="run">The run.</param>
    /// <returns>The deleted nodes.</returns>
    /// <exception cref="ArgumentOutOfRangeException">at less than 1 or
    /// negative run</exception>
    public IList<CharNode> Delete(int at, int run = 1)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(at, 1);
        ArgumentOutOfRangeException.ThrowIfNegative(run);

        List<CharNode> nodes = [];
        for (int i = 0; i < run; i++)
        {
            CharNode? node = _nodes.Find(n => n.Ordinal == at + i);
            if (node != null)
            {
                node.IsDeleted = true;
                nodes.Add(node);
            }
        }
        return nodes;
    }

    /// <summary>
    /// Inserts the specified nodes before the node at the specified ordinal.
    /// </summary>
    /// <param name="at">The reference node ordinal.</param>
    /// <param name="nodes">The nodes to insert.</param>
    /// <returns>The inserted nodes.</returns>
    /// <exception cref="ArgumentNullException">nodes</exception>
    /// <exception cref="ArgumentOutOfRangeException">at less than 1</exception>
    public IList<CharNode> InsertBefore(int at, IList<CharNode> nodes)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(at, 1);
        ArgumentNullException.ThrowIfNull(nameof(nodes));

        if (nodes.Count == 0) return nodes;

        CharNode? refNode = _nodes.Find(n => n.Ordinal == at)
            ?? throw new InvalidOperationException(
                $"Reference node not found at ordinal {at}.");

        // inserting XY to the left of B in A[B] means:
        // A<-X
        nodes[0].LeftNode = refNode.LeftNode;
        // Y<-B
        refNode.LeftNode = nodes[^1];

        // update the LeftNode of the node to the right of the inserted nodes
        CharNode? right = GetRightNodeOf(nodes[^1]);
        if (right != null) right.LeftNode = nodes[^1];

        _nodes.AddRange(nodes);
        return nodes;
    }

    /// <summary>
    /// Inserts the specified new text before the node at the specified ordinal.
    /// </summary>
    /// <param name="at">The reference node ordinal.</param>
    /// <param name="text">The text to insert.</param>
    /// <returns>The inserted nodes.</returns>
    /// <exception cref="ArgumentNullException">nodes</exception>
    public IList<CharNode> InsertBefore(int at, string text)
        => InsertBefore(at, GetNodesFor(text));

    /// <summary>
    /// Inserts the specified new char before the node at the specified ordinal.
    /// </summary>
    /// <param name="at">The reference node ordinal.</param>
    /// <param name="c">The character to insert.</param>
    /// <returns>The inserted nodes.</returns>
    /// <exception cref="ArgumentNullException">nodes</exception>
    public IList<CharNode> InsertBefore(int at, char c)
        => InsertBefore(at, GetNodesFor(c.ToString()));

    /// <summary>
    /// Inserts the specified nodes after the node at the specified ordinal.
    /// </summary>
    /// <param name="at">The reference node ordinal.</param>
    /// <param name="nodes">The nodes to insert.</param>
    /// <returns>The inserted nodes.</returns>
    /// <exception cref="ArgumentNullException">nodes</exception>
    /// <exception cref="ArgumentOutOfRangeException">at less than 1</exception>
    public IList<CharNode> InsertAfter(int at, IList<CharNode> nodes)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(at, 1);
        ArgumentNullException.ThrowIfNull(nameof(nodes));

        if (nodes.Count == 0) return nodes;

        CharNode? refNode = _nodes.Find(n => n.Ordinal == at)
            ?? throw new InvalidOperationException(
                $"Reference node not found at ordinal {at}.");

        // inserting XY to the right of A in [A]<-B means:
        // A<-X
        nodes[0].LeftNode = refNode;
        // Y<-B
        // refNode.LeftNode = nodes[^1];
        CharNode? right = GetRightNodeOf(refNode);
        if (right != null) right.LeftNode = nodes[^1];

        _nodes.AddRange(nodes);
        return nodes;
    }

    /// <summary>
    /// Inserts the specified new text after the node at the specified ordinal.
    /// </summary>
    /// <param name="at">The reference node ordinal.</param>
    /// <param name="text">The text to insert.</param>
    /// <returns>The inserted nodes.</returns>
    /// <exception cref="ArgumentNullException">nodes</exception>
    public IList<CharNode> InsertAfter(int at, string text)
        => InsertAfter(at, GetNodesFor(text));

    /// <summary>
    /// Inserts the specified new char after the node at the specified ordinal.
    /// </summary>
    /// <param name="at">The reference node ordinal.</param>
    /// <param name="c">The character to insert.</param>
    /// <returns>The inserted nodes.</returns>
    /// <exception cref="ArgumentNullException">nodes</exception>
    public IList<CharNode> InsertAfter(int at, char c)
        => InsertAfter(at, GetNodesFor(c.ToString()));

    /// <summary>
    /// Replaces the nodes starting from the specified ordinal, for the specified
    /// run of nodes, with the specified nodes.
    /// </summary>
    /// <param name="at">The start ordinal.</param>
    /// <param name="run">The run.</param>
    /// <param name="nodes">The nodes to replace.</param>
    /// <returns>The replaced nodes.</returns>
    /// <exception cref="ArgumentNullException">nodes</exception>
    /// <exception cref="ArgumentOutOfRangeException">at less than 1 or
    /// negative run</exception>
    public IList<CharNode> Replace(int at, int run, IList<CharNode> nodes)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(at, 1);
        ArgumentNullException.ThrowIfNull(nameof(nodes));
        ArgumentOutOfRangeException.ThrowIfNegative(run);

        if (nodes.Count == 0) return Delete(at, run);
        if (run == 0) return InsertBefore(at, nodes);

        List<CharNode> replaced = [];
        for (int i = 0; i < nodes.Count && i < run; i++)
        {
            CharNode? node = _nodes.Find(n => n.Ordinal == at + i);
            // reference node not found
            if (node == null && i == 0)
            {
                throw new InvalidOperationException(
                    $"Reference node not found at ordinal {at}.");
            }

            // if target not found, insert the node as new
            if (node == null)
            {
                replaced.AddRange(InsertBefore(at + i, new[] { nodes[i] }));
            }
            // else replace target value
            else if (node.Value != nodes[i].Value)
            {
                node.Value = nodes[i].Value;
                replaced.Add(node);
            }
        }

        // delete or insert tail
        if (run > nodes.Count)
        {
            replaced.AddRange(Delete(at + nodes.Count, run - nodes.Count));
        }
        else if (run < nodes.Count)
        {
            replaced.AddRange(InsertBefore(at + replaced.Count,
                nodes.Skip(run).ToList()));
        }

        return replaced;
    }

    /// <summary>
    /// Replaces the nodes starting from the specified ordinal, for the specified
    /// run of nodes, with the specified text.
    /// </summary>
    /// <param name="at">The start ordinal.</param>
    /// <param name="run">The run.</param>
    /// <param name="text">The new text to replace.</param>
    /// <returns>The replaced nodes.</returns>
    /// <exception cref="ArgumentNullException">nodes</exception>
    /// <exception cref="ArgumentOutOfRangeException">at less than 1 or
    /// negative run</exception>
    public IList<CharNode> Replace(int at, int run, string text)
        => Replace(at, run, GetNodesFor(text));

    /// <summary>
    /// Replaces the nodes starting from the specified ordinal, for the specified
    /// run of nodes, with the specified character.
    /// </summary>
    /// <param name="at">The start ordinal.</param>
    /// <param name="run">The run.</param>
    /// <param name="c">The new character to replace.</param>
    /// <returns>The replaced nodes.</returns>
    /// <exception cref="ArgumentNullException">nodes</exception>
    /// <exception cref="ArgumentOutOfRangeException">at less than 1 or
    /// negative run</exception>
    public IList<CharNode> Replace(int at, int run, char c)
        => Replace(at, run, GetNodesFor(c.ToString()));

    /// <summary>
    /// Moves the nodes starting with the node with ordinal <paramref name="from"/>
    /// to the position before the node with ordinal <paramref name="to"/>.
    /// </summary>
    /// <param name="from">The ordinal of the first node to move.</param>
    /// <param name="run">The number of nodes to cut from the first.</param>
    /// <param name="to">The ordinal of the node to insert cut nodes at the
    /// left of.</param>
    /// <returns>True if moved, false if source or target not found.</returns>
    /// <exception cref="ArgumentOutOfRangeException">from or to less than 1,
    /// or negative run.</exception>
    public bool Move(int from, int run, int to)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(from, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(to, 1);
        ArgumentOutOfRangeException.ThrowIfNegative(run);
        if (run == 0) return false;

        // collect the nodes to move
        List<CharNode> nodes = [];
        for (int i = 0; i < run; i++)
        {
            CharNode? node = _nodes.Find(n => n.Ordinal == from + i);
            if (node == null && i == 0) return false;
            if (node != null) nodes.Add(node);
        }

        // link the node to the right of the last collected node
        // to the node to the left of the first collected node
        CharNode? left = GetLeftNodeOf(nodes[0]);
        CharNode? right = GetRightNodeOf(nodes[^1]);

        if (right != null) right.LeftNode = left;

        // adjust to if it refers to a node in the sequence being moved
        if (to > from && to <= from + run) to += run;

        // insert the nodes
        InsertBefore(to, nodes);

        return true;
    }

    /// <summary>
    /// Swaps the set of nodes specified by two ranges.
    /// </summary>
    /// <param name="at1">The ordinal of the first node of set 1.</param>
    /// <param name="run1">The run for set 1.</param>
    /// <param name="at2">The ordinal of the first node of set 2.</param>
    /// <param name="run2">The run for set 2.</param>
    /// <returns>
    /// True if swapped, false if both runs are 0 or sets reference
    /// nodes are not found.
    /// </returns>
    /// <exception cref="InvalidOperationException">Cannot swap overlapping nodes.</exception>
    /// <exception cref="ArgumentOutOfRangeException">at1, at2 less than 1,
    /// run1, run2 negative.</exception>
    public bool Swap(int at1, int run1, int at2, int run2)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(at1, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(at2, 1);
        ArgumentOutOfRangeException.ThrowIfNegative(run1);
        ArgumentOutOfRangeException.ThrowIfNegative(run2);

        if (run1 == 0 && run2 == 0) return false;

        // collect 1 nodes
        List<CharNode> nodes1 = [];
        for (int i = 0; i < run1; i++)
        {
            CharNode? node = _nodes.Find(n => n.Ordinal == at1 + i);
            if (node == null && i == 0) return false;
            if (node != null) nodes1.Add(node);
        }

        // collect 2 nodes
        List<CharNode> nodes2 = [];
        for (int i = 0; i < run2; i++)
        {
            CharNode? node = _nodes.Find(n => n.Ordinal == at2 + i);
            if (node == null && i == 0) return false;
            if (node != null) nodes2.Add(node);
        }

        // ensure there is no overlap between the two sets
        if (nodes1.Any(nodes2.Contains))
            throw new InvalidOperationException("Cannot swap overlapping nodes.");

        // collect anchors: l1 and r1 for left and right nodes of 1st set,
        // l2 and r2 for left and right nodes of 2nd set
        CharNode? l1 = GetLeftNodeOf(nodes1[0]);
        CharNode? r1 = GetRightNodeOf(nodes1[^1]);

        CharNode? l2 = GetLeftNodeOf(nodes2[0]);
        CharNode? r2 = GetRightNodeOf(nodes2[^1]);

        // corner case: adjacent is a move
        if (r1?.LeftNode == l2)
        {
            return Move(at1, run1, at2);
        }

        // swap via anchors
        nodes1[0].LeftNode = l2;
        if (r1 != null) r1.LeftNode = nodes2[^1];
        nodes2[0].LeftNode = l1;
        if (r2 != null) r2.LeftNode = nodes1[^1];

        return true;
    }
}
